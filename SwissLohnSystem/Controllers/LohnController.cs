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

        // =====================================================
        // POST: api/Lohn/calc
        // - Sadece hesaplama, DB'ye yazmaz
        // - Flag ve Stammdaten her zaman Employee'den alınır
        // =====================================================
        [HttpPost("calc")]
        public async Task<ActionResult<ApiResponse<PayrollResponseDto>>> Calculate([FromBody] PayrollRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PayrollResponseDto>.Fail("Ungültige Eingabedaten."));

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

            if (employee is null)
                return NotFound(ApiResponse<PayrollResponseDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            // --- Zeitraum & Active-Flag prüfen ---
            var periodMonthStart = new DateTime(request.Period.Year, request.Period.Month, 1);
            if (!employee.Active ||
                employee.StartDate > periodMonthStart ||
                (employee.EndDate.HasValue && employee.EndDate.Value < periodMonthStart))
            {
                return BadRequest(ApiResponse<PayrollResponseDto>.Fail(
                    "Mitarbeiter ist in diesem Monat nicht aktiv."
                ));
            }

            // --- Stammdaten & Flags IMMER vom Employee lesen ---
            request.Canton = string.IsNullOrWhiteSpace(employee.Canton) ? "ZH" : employee.Canton;
            request.ApplyAHV = employee.ApplyAHV;
            request.ApplyALV = employee.ApplyALV;
            request.ApplyBVG = employee.ApplyBVG;
            request.ApplyNBU = employee.ApplyNBU;
            request.ApplyBU = employee.ApplyBU;
            request.ApplyFAK = employee.ApplyFAK;
            request.ApplyQST = employee.ApplyQST;
            request.WeeklyHours = employee.WeeklyHours;
            request.PermitType = employee.PermitType;
            request.ChurchMember = employee.ChurchMember;
            request.WithholdingTaxCode = employee.WithholdingTaxCode;

            // --- Bruttolohn (Monat) bestimmen ---
            if (employee.SalaryType == "Monthly")
            {
                // Fixer Monatslohn
                request.GrossMonthly = employee.BruttoSalary;
            }
            else // SalaryType == "Hourly"
            {
                var year = request.Period.Year;
                var month = request.Period.Month;

                var workDays = await _context.WorkDays
                    .AsNoTracking()
                    .Where(w => w.EmployeeId == employee.Id &&
                                w.Date.Year == year &&
                                w.Date.Month == month)
                    .ToListAsync();

                if (workDays.Count == 0)
                {
                    return BadRequest(ApiResponse<PayrollResponseDto>.Fail(
                        "Keine Arbeitszeit-Daten für diesen stundenbasierten Mitarbeiter in diesem Monat gefunden."
                    ));
                }

                var totalHours = workDays.Sum(w => w.HoursWorked);
                var totalOvertime = workDays.Sum(w => w.OvertimeHours);

                var overtimeFactor = (employee.OvertimeRate.HasValue && employee.OvertimeRate.Value > 0m)
                    ? employee.OvertimeRate.Value
                    : 1m;

                var normalPay = employee.HourlyRate * totalHours;
                var overtimePay = employee.HourlyRate * overtimeFactor * totalOvertime;

                // ⭐ Ferienentschädigung (nur Stundenlohn)
                decimal holidayAllowance = 0m;
                if (employee.HolidayRate.HasValue && employee.HolidayRate.Value > 0m)
                {
                    var holidayRateFactor = employee.HolidayRate.Value / 100m;
                    holidayAllowance = Math.Round(normalPay * holidayRateFactor, 2);
                }

                request.GrossMonthly = normalPay + overtimePay + holidayAllowance;
            }

            var result = _calculator.Calculate(request);
            return Ok(ApiResponse<PayrollResponseDto>.Ok(result, "Berechnung erfolgreich."));
        }

        // =====================================================
        // POST: api/Lohn/calc-and-save
        // - Hesapla + Lohn tablosuna ENTWURF olarak upsert et
        // =====================================================
        [HttpPost("calc-and-save")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> CalculateAndSave([FromBody] PayrollRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LohnDto>.Fail("Ungültige Eingabedaten."));

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

            if (employee is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            // ---- Zeitraum & Active-Flag prüfen ----
            var periodMonthStart = new DateTime(request.Period.Year, request.Period.Month, 1);
            if (!employee.Active ||
                employee.StartDate > periodMonthStart ||
                (employee.EndDate.HasValue && employee.EndDate.Value < periodMonthStart))
            {
                return BadRequest(ApiResponse<LohnDto>.Fail(
                    "Mitarbeiter ist in diesem Monat nicht aktiv."
                ));
            }

            // ---- Stammdaten & Flags IMMER vom Employee ----
            request.Canton = string.IsNullOrWhiteSpace(employee.Canton) ? "ZH" : employee.Canton;
            request.ApplyAHV = employee.ApplyAHV;
            request.ApplyALV = employee.ApplyALV;
            request.ApplyBVG = employee.ApplyBVG;
            request.ApplyNBU = employee.ApplyNBU;
            request.ApplyBU = employee.ApplyBU;
            request.ApplyFAK = employee.ApplyFAK;
            request.ApplyQST = employee.ApplyQST;
            request.WeeklyHours = employee.WeeklyHours;
            request.PermitType = employee.PermitType;
            request.ChurchMember = employee.ChurchMember;
            request.WithholdingTaxCode = employee.WithholdingTaxCode;

            // ---- Bruttolohn (Monat) + Überstundenbetrag + Ferienentschädigung ----
            decimal overtimePayForLohn = 0m;
            decimal monthlyHours = 0m;
            decimal monthlyOvertimeHours = 0m;
            decimal holidayAllowance = 0m;
            decimal childAllowance = 0m; // TODO: später Kindergeld-Logik

            if (employee.SalaryType == "Monthly")
            {
                request.GrossMonthly = employee.BruttoSalary;

                // Aylık çalışan için şimdilik sadece planlanan aylık saat saklıyoruz
                monthlyHours = employee.MonthlyHours;
                monthlyOvertimeHours = 0m;
            }
            else // "Hourly"
            {
                var year = request.Period.Year;
                var month = request.Period.Month;

                var workDays = await _context.WorkDays
                    .AsNoTracking()
                    .Where(w => w.EmployeeId == employee.Id &&
                                w.Date.Year == year &&
                                w.Date.Month == month)
                    .ToListAsync();

                if (workDays.Count == 0)
                {
                    return BadRequest(ApiResponse<LohnDto>.Fail(
                        "Keine Arbeitszeit-Daten für diesen stundenbasierten Mitarbeiter in diesem Monat gefunden."
                    ));
                }

                monthlyHours = workDays.Sum(w => w.HoursWorked);
                monthlyOvertimeHours = workDays.Sum(w => w.OvertimeHours);

                var overtimeFactor = (employee.OvertimeRate.HasValue && employee.OvertimeRate.Value > 0m)
                    ? employee.OvertimeRate.Value
                    : 1m;

                var normalPay = employee.HourlyRate * monthlyHours;
                overtimePayForLohn = employee.HourlyRate * overtimeFactor * monthlyOvertimeHours;

                // ⭐ Ferienentschädigung (nur Stundenlohn)
                if (employee.HolidayRate.HasValue && employee.HolidayRate.Value > 0m)
                {
                    var holidayRateFactor = employee.HolidayRate.Value / 100m;
                    holidayAllowance = Math.Round(normalPay * holidayRateFactor, 2);
                }

                request.GrossMonthly = normalPay + overtimePayForLohn + holidayAllowance;
            }

            // ---- Modern hesap motorunu çağır ----
            var result = _calculator.Calculate(request);

            var yearForLohn = request.Period.Year;
            var monthForLohn = request.Period.Month;

            var gross = request.GrossMonthly;
            var net = result.NetToPay;
            var empDeductions = result.Employee.Total;
            var overtimePay = overtimePayForLohn;

            // Idempotent upsert...
            var existing = await _context.Lohns
                .FirstOrDefaultAsync(l =>
                    l.EmployeeId == employee.Id &&
                    l.Year == yearForLohn &&
                    l.Month == monthForLohn);

            Lohn lohn;
            if (existing is not null)
            {
                if (existing.IsFinal)
                    return BadRequest(ApiResponse<LohnDto>.Fail(
                        "Diese Lohnabrechnung ist bereits finalisiert und kann nicht geändert werden."
                    ));

                existing.BruttoSalary = gross;
                existing.NetSalary = net;
                existing.TotalDeductions = empDeductions;
                existing.OvertimePay = overtimePay;
                existing.HolidayAllowance = holidayAllowance;
                existing.ChildAllowance = childAllowance;
                existing.MonthlyHours = monthlyHours;
                existing.MonthlyOvertimeHours = monthlyOvertimeHours;
                existing.CreatedAt = DateTime.Now;

                lohn = existing;
            }
            else
            {
                lohn = new Lohn
                {
                    EmployeeId = employee.Id,
                    Month = monthForLohn,
                    Year = yearForLohn,
                    BruttoSalary = gross,
                    NetSalary = net,
                    TotalDeductions = empDeductions,
                    OvertimePay = overtimePay,
                    HolidayAllowance = holidayAllowance,
                    ChildAllowance = childAllowance,
                    MonthlyHours = monthlyHours,
                    MonthlyOvertimeHours = monthlyOvertimeHours,
                    CreatedAt = DateTime.Now,
                    IsFinal = false
                };

                _context.Lohns.Add(lohn);
            }

            await _context.SaveChangesAsync();

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung wurde berechnet und als Entwurf gespeichert.");
        }

        // =============================
        // GET: api/Lohn
        // =============================
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

        // =============================
        // GET: api/Lohn/{id}
        // =============================
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

        // ======================================
        // POST: api/Lohn/finalize
        // ======================================
        [HttpPost("finalize")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> FinalizePayroll([FromBody] LohnCalculateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LohnDto>.Fail("Ungültige Eingabedaten."));

            var lohn = await _context.Lohns
                .FirstOrDefaultAsync(l =>
                    l.EmployeeId == dto.EmployeeId &&
                    l.Month == dto.Month &&
                    l.Year == dto.Year);

            if (lohn is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Lohnabrechnung wurde nicht gefunden."));

            if (lohn.IsFinal)
                return BadRequest(ApiResponse<LohnDto>.Fail("Diese Lohnabrechnung ist bereits final."));

            lohn.IsFinal = true;
            await _context.SaveChangesAsync();

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung wurde finalisiert.");
        }

        // =====================================
        // GET: api/Lohn/by-employee/{employeeId}
        // =====================================
        [HttpGet("by-employee/{employeeId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetByEmployee(int employeeId)
        {
            var list = await _context.Lohns
                .AsNoTracking()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.Year)
                .ThenByDescending(l => l.Month)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(list, "Löhne erfolgreich geladen.");
        }

        // =========================================================
        // GET: api/Lohn/by-company/{companyId}?period=YYYY-MM
        // =========================================================
        [HttpGet("by-company/{companyId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetByCompany(
            int companyId,
            [FromQuery] string? period = null)
        {
            int? month = null, year = null;

            if (IsValidPeriod(period))
                (year, month) = ParsePeriod(period!);

            var query = _context.Lohns
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l => l.Employee.CompanyId == companyId);

            if (month.HasValue && year.HasValue)
            {
                query = query.Where(l => l.Month == month.Value && l.Year == year.Value);
            }

            var list = await query
                .OrderByDescending(l => l.Year)
                .ThenByDescending(l => l.Month)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(list, "Löhne erfolgreich geladen.");
        }

        // =========================================================
        // GET: api/Lohn/by-company/{companyId}/monthly?period=YYYY-MM
        // =========================================================
        [HttpGet("by-company/{companyId:int}/monthly")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnMonthlyRowDto>>>> GetCompanyMonthly(
            int companyId,
            [FromQuery] string? period = null)
        {
            if (!IsValidPeriod(period))
                return BadRequest(ApiResponse<IEnumerable<LohnMonthlyRowDto>>.Fail("Ungültiger Zeitraum. Erwartet: YYYY-MM."));

            var (year, month) = ParsePeriod(period!);

            var rows = await _context.Lohns
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l => l.Employee.CompanyId == companyId &&
                            l.Year == year &&
                            l.Month == month)
                .OrderBy(l => l.Employee.LastName)
                .ThenBy(l => l.Employee.FirstName)
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
            !string.IsNullOrWhiteSpace(p) &&
            p!.Length == 7 &&
            p[4] == '-' &&
            int.TryParse(p.AsSpan(0, 4), out _) &&
            int.TryParse(p.AsSpan(5, 2), out var m) &&
            m is >= 1 and <= 12;

        private static (int year, int month) ParsePeriod(string p) =>
            (int.Parse(p.AsSpan(0, 4)), int.Parse(p.AsSpan(5, 2)));
    }
}
