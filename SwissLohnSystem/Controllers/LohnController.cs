using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;
using SwissLohnSystem.API.Services.Payroll;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LohnController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollCalculator _calculator;

        public LohnController(ApplicationDbContext context, IPayrollCalculator calculator)
        {
            _context = context;
            _calculator = calculator;
        }

        // =========================
        // NEW: Modern hesap motoru
        // =========================
        // NOT: Bu uç yalnızca hesaplar ve sonucu döner (persist ETMEZ).
        // Oranlar/kurallar PayrollCalculator içinde Settings'ten okunmalıdır.
       
        [HttpPost("calc")]
        public ActionResult<ApiResponse<PayrollResponseDto>> Calculate([FromBody] PayrollRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PayrollResponseDto>.Fail("Ungültige Eingabedaten."));

            var result = _calculator.Calculate(request);
            return Ok(ApiResponse<PayrollResponseDto>.Ok(result, "Berechnung erfolgreich."));
        }


        // ==============================================
        // LEGACY: Eski yöntem (DB'ye yazan idempotent akış)
        // ==============================================
        // Geçici olarak farklı route altında tutuluyor; ileride kaldırılabilir.
        [HttpPost("calc-legacy")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> CalculateLegacy([FromBody] LohnCalculateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LohnDto>.Fail("Ungültige Eingabedaten."));

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            var date = new DateTime(dto.Year, dto.Month, 1);
            if (employee.StartDate > date || (employee.EndDate.HasValue && employee.EndDate.Value < date))
                return BadRequest(ApiResponse<LohnDto>.Fail("Mitarbeiter ist in diesem Monat nicht aktiv."));

            // Einstellungen (tek seferde çek) - LEGACY: yüzdelik ölçek (%)
            var settings = await _context.Settings.AsNoTracking().ToListAsync();
            decimal ahv = settings.FirstOrDefault(s => s.Name == "AHV")?.Value ?? 0m;
            decimal alv = settings.FirstOrDefault(s => s.Name == "ALV")?.Value ?? 0m;
            decimal bvg = settings.FirstOrDefault(s => s.Name == "BVG")?.Value ?? 0m;
            decimal steuer = settings.FirstOrDefault(s => s.Name == "Steuer")?.Value ?? 0m;
            decimal childAllowanceValue = settings.FirstOrDefault(s => s.Name == "ChildAllowance")?.Value ?? 0m;
            decimal holidayRate = settings.FirstOrDefault(s => s.Name == "HolidayRate")?.Value ?? 8.33m;
            decimal overtimeRate = settings.FirstOrDefault(s => s.Name == "OvertimeRate")?.Value ?? 1.25m;

            var workDays = await _context.WorkDays
                .Where(w => w.EmployeeId == dto.EmployeeId && w.Date.Month == dto.Month && w.Date.Year == dto.Year)
                .ToListAsync();

            decimal totalHours = employee.SalaryType == "Monthly"
                ? employee.MonthlyHours
                : workDays.Sum(w => w.HoursWorked);

            decimal totalOvertime = workDays.Sum(w => w.OvertimeHours);

            if (employee.SalaryType == "Hourly" && workDays.Count == 0)
                return BadRequest(ApiResponse<LohnDto>.Fail("Keine Arbeitszeit-Daten für diesen stundenbasierten Mitarbeiter gefunden."));

            decimal brutto = employee.SalaryType == "Monthly"
                ? employee.BruttoSalary
                : employee.HourlyRate * totalHours;

            decimal overtimePay = totalOvertime * employee.HourlyRate * overtimeRate;
            decimal holidayAllowance = brutto * holidayRate / 100m;

            // DİKKAT: Legacy kod yüzdelik ölçek kullanır (%, 100'e böler).
            decimal totalDeductions = (brutto + overtimePay) * (ahv + alv + bvg + steuer) / 100m;

            decimal childAllowance = employee.ChildCount * childAllowanceValue;

            // Net: Brutto + (ek ödemeler) - (kesintiler)
            decimal netSalary = brutto + overtimePay + holidayAllowance + childAllowance - totalDeductions;

            // Idempotent upsert: aynı (EmployeeId, Month, Year) varsa güncelle; yoksa oluştur.
            var existing = await _context.Lohns
                .FirstOrDefaultAsync(l => l.EmployeeId == employee.Id && l.Month == dto.Month && l.Year == dto.Year);

            if (existing is not null)
            {
                if (existing.IsFinal)
                    return BadRequest(ApiResponse<LohnDto>.Fail("Diese Lohnabrechnung ist bereits finalisiert und kann nicht geändert werden."));

                existing.BruttoSalary = brutto;
                existing.OvertimePay = overtimePay;
                existing.HolidayAllowance = holidayAllowance;
                existing.TotalDeductions = totalDeductions;
                existing.ChildAllowance = childAllowance;
                existing.NetSalary = netSalary;
                existing.CreatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return ApiResponse<LohnDto>.Ok(existing.ToDto(), "Lohnabrechnung erfolgreich neu berechnet (aktualisiert).");
            }

            var lohn = new Lohn
            {
                EmployeeId = employee.Id,
                Month = dto.Month,
                Year = dto.Year,
                BruttoSalary = brutto,
                OvertimePay = overtimePay,
                HolidayAllowance = holidayAllowance,
                TotalDeductions = totalDeductions,
                ChildAllowance = childAllowance,
                NetSalary = netSalary,
                CreatedAt = DateTime.Now,
                IsFinal = false
            };

            _context.Lohns.Add(lohn);
            await _context.SaveChangesAsync();
            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung erfolgreich berechnet und gespeichert.");
        }

        // GET: api/Lohn
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetLohns()
        {
            var lohns = await _context.Lohns
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(lohns, "Alle Lohnabrechnungen wurden erfolgreich geladen.");
        }

        // GET: api/Lohn/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> GetLohn(int id)
        {
            var lohn = await _context.Lohns
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lohn is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Lohnabrechnung wurde nicht gefunden."));

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung erfolgreich geladen.");
        }

        // POST: api/Lohn/finalize
        [HttpPost("finalize")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> FinalizePayroll([FromBody] LohnCalculateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LohnDto>.Fail("Ungültige Eingabedaten."));

            var lohn = await _context.Lohns
                .FirstOrDefaultAsync(l => l.EmployeeId == dto.EmployeeId && l.Month == dto.Month && l.Year == dto.Year);

            if (lohn is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Lohnabrechnung wurde nicht gefunden."));

            if (lohn.IsFinal)
                return BadRequest(ApiResponse<LohnDto>.Fail("Diese Lohnabrechnung ist bereits final."));

            lohn.IsFinal = true;
            await _context.SaveChangesAsync();
            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung wurde finalisiert.");
        }

        // GET: api/Lohn/by-employee/{employeeId}
        [HttpGet("by-employee/{employeeId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetByEmployee(int employeeId)
        {
            var list = await _context.Lohns
                .AsNoTracking()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.Year).ThenByDescending(l => l.Month)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(list, "Löhne erfolgreich geladen.");
        }

        // GET: api/Lohn/by-company/{companyId}?period=YYYY-MM
        [HttpGet("by-company/{companyId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetByCompany(int companyId, [FromQuery] string? period = null)
        {
            int? month = null, year = null;

            if (IsValidPeriod(period))
            {
                (year, month) = ParsePeriod(period!);
            }

            var query = _context.Lohns
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l => l.Employee.CompanyId == companyId);

            if (month.HasValue && year.HasValue)
                query = query.Where(l => l.Month == month.Value && l.Year == year.Value);

            var list = await query
                .OrderByDescending(l => l.Year).ThenByDescending(l => l.Month)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(list, "Löhne erfolgreich geladen.");
        }

        // GET: api/Lohn/by-company/{companyId}/monthly?period=YYYY-MM
        [HttpGet("by-company/{companyId:int}/monthly")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnMonthlyRowDto>>>> GetCompanyMonthly(int companyId, [FromQuery] string? period = null)
        {
            if (!IsValidPeriod(period))
                return BadRequest(ApiResponse<IEnumerable<LohnMonthlyRowDto>>.Fail("Ungültiger Zeitraum. Erwartet: YYYY-MM."));

            var (year, month) = ParsePeriod(period!);

            var rows = await _context.Lohns
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l => l.Employee.CompanyId == companyId && l.Year == year && l.Month == month)
                .OrderBy(l => l.Employee.LastName).ThenBy(l => l.Employee.FirstName)
                .Select(l => new LohnMonthlyRowDto(
                    l.EmployeeId,
                    l.Employee.FirstName + " " + l.Employee.LastName,
                    l.Month,
                    l.Year,
                    l.BruttoSalary,
                    l.NetSalary,
                    l.TotalDeductions,
                    l.OvertimePay,
                    l.HolidayAllowance
                ))
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnMonthlyRowDto>>.Ok(rows, "Monatliche Löhne erfolgreich geladen.");
        }

        // --------- helpers ---------
        private static bool IsValidPeriod(string? p) =>
            !string.IsNullOrWhiteSpace(p) && p!.Length == 7 && p[4] == '-' &&
            int.TryParse(p.AsSpan(0, 4), out var y) &&
            int.TryParse(p.AsSpan(5, 2), out var m) &&
            m is >= 1 and <= 12;

        private static (int year, int month) ParsePeriod(string p) =>
            (int.Parse(p.AsSpan(0, 4)), int.Parse(p.AsSpan(5, 2)));
    }
}
