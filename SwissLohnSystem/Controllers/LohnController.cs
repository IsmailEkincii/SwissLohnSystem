using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;
using SwissLohnSystem.API.Services.Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        // - JS: Employees/Details -> lohnverlauf-detail.js
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

            var year = request.Period.Year;
            var month = request.Period.Month;
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var employmentStart = employee.StartDate.Date;
            var employmentEnd = (employee.EndDate ?? DateTime.MaxValue).Date;

            // Mitarbeiter in diesem Monat aktiv?
            if (!employee.Active ||
                employmentEnd < monthStart ||
                employmentStart > monthEnd)
            {
                return BadRequest(ApiResponse<PayrollResponseDto>.Fail(
                    "Mitarbeiter ist in diesem Monat nicht aktiv."
                ));
            }

            // ---- Default parametreleri Employee'den doldur ----
            if (string.IsNullOrWhiteSpace(request.Canton))
                request.Canton = string.IsNullOrWhiteSpace(employee.Canton) ? "ZH" : employee.Canton;

            request.ApplyAHV = employee.ApplyAHV;
            request.ApplyALV = employee.ApplyALV;
            request.ApplyBVG = employee.ApplyBVG;
            request.ApplyNBU = employee.ApplyNBU;
            request.ApplyBU = employee.ApplyBU;
            request.ApplyFAK = employee.ApplyFAK;
            request.ApplyQST = employee.ApplyQST;

            request.WeeklyHours = employee.WeeklyHours;

            request.PermitType ??= employee.PermitType;
            request.ChurchMember = employee.ChurchMember;
            if (string.IsNullOrWhiteSpace(request.WithholdingTaxCode))
                request.WithholdingTaxCode = employee.WithholdingTaxCode;

            // ---- Bruttolohn-Basis + Stundenberechnung ----
            decimal monthlyHours = 0m;
            decimal monthlyOvertimeHours = 0m;
            decimal overtimePay = 0m;
            decimal holidayAllowance = 0m; // Şimdilik otomatik hesaplamıyoruz
            decimal baseGross;

            if (employee.SalaryType == "Monthly")
            {
                var monthDays = (monthEnd - monthStart).Days + 1;

                var effectiveStart = employmentStart < monthStart ? monthStart : employmentStart;
                var effectiveEnd = employmentEnd > monthEnd ? monthEnd : employmentEnd;

                var activeDays = (effectiveEnd - effectiveStart).Days + 1;
                if (activeDays < 0) activeDays = 0;

                // 🔥 Gün mantığı: Worked / Sick / Unpaid
                var worked = request.WorkedDays ?? 0m;
                var sick = request.SickDays ?? 0m;

                decimal unpaid;
                if (request.UnpaidDays.HasValue)
                {
                    unpaid = request.UnpaidDays.Value;
                }
                else
                {
                    unpaid = (decimal)activeDays - (worked + sick);
                    if (unpaid < 0m) unpaid = 0m;
                }

                if (unpaid > activeDays)
                    unpaid = activeDays;

                var paidDays = (decimal)activeDays - unpaid;

                var factor = monthDays == 0 ? 0m : paidDays / (decimal)monthDays;

                var baseMonthly = employee.BruttoSalary;
                var proratedBase = Math.Round(baseMonthly * factor, 2);

                baseGross = proratedBase;
                monthlyHours = employee.MonthlyHours * factor;
                monthlyOvertimeHours = 0m;
            }
            else // "Hourly"
            {
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

                monthlyHours = workDays.Sum(w => w.HoursWorked);
                monthlyOvertimeHours = workDays.Sum(w => w.OvertimeHours);

                var normalPay = employee.HourlyRate * monthlyHours;
                overtimePay = employee.HourlyRate * monthlyOvertimeHours;

                baseGross = normalPay + overtimePay; // Ferienentschädigung yok (ileride HolidayRate ile eklenebilir)
            }

            // Bonus / Zulagen / Abzüge (request’ten)
            var bonus = request.Bonus;
            var extra = request.ExtraAllowance;
            var unpaidDed = request.UnpaidDeduction;
            var otherDed = request.OtherDeduction;

            var adjustments = bonus + extra - unpaidDed - otherDed;

            request.GrossMonthly = baseGross + adjustments;

            var result = _calculator.Calculate(request);
            return Ok(ApiResponse<PayrollResponseDto>.Ok(result, "Berechnung erfolgreich."));
        }

        // =====================================================
        // POST: api/Lohn/calc-and-save
        // - Hesapla + Lohn tablosuna ENTWURF olarak upsert et
        // - UI: Companies/Details -> Löhne tabı (loehne-tab.js)
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

            var year = request.Period.Year;
            var month = request.Period.Month;
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var employmentStart = employee.StartDate.Date;
            var employmentEnd = (employee.EndDate ?? DateTime.MaxValue).Date;

            if (!employee.Active ||
                employmentEnd < monthStart ||
                employmentStart > monthEnd)
            {
                return BadRequest(ApiResponse<LohnDto>.Fail(
                    "Mitarbeiter ist in diesem Monat nicht aktiv."
                ));
            }

            // ---- Default parametreleri Employee'den doldur ----
            if (string.IsNullOrWhiteSpace(request.Canton))
                request.Canton = string.IsNullOrWhiteSpace(employee.Canton) ? "ZH" : employee.Canton;

            if (request.WeeklyHours <= 0)
                request.WeeklyHours = employee.WeeklyHours;

            request.PermitType ??= employee.PermitType;
            request.ChurchMember = request.ChurchMember || employee.ChurchMember;

            if (string.IsNullOrWhiteSpace(request.WithholdingTaxCode))
                request.WithholdingTaxCode = employee.WithholdingTaxCode;

            // ⚠️ Flag'leri artık UI'dan gelen değere bırakıyoruz
            // (ApplyAHV, ApplyALV, ... zaten request içinde geliyor)

            // ---- Bruttolohn-Basis + Stundenberechnung ----
            decimal monthlyHours = 0m;
            decimal monthlyOvertimeHours = 0m;
            decimal overtimePayForLohn = 0m;
            decimal holidayAllowance = 0m;    // Şimdilik hesaplamıyoruz
            decimal childAllowance = 0m;      // TODO: später Kindergeld-Logik

            decimal baseGross;

            if (employee.SalaryType == "Monthly")
            {
                var monthDays = (monthEnd - monthStart).Days + 1;

                var effectiveStart = employmentStart < monthStart ? monthStart : employmentStart;
                var effectiveEnd = employmentEnd > monthEnd ? monthEnd : employmentEnd;

                var activeDays = (effectiveEnd - effectiveStart).Days + 1;
                if (activeDays < 0) activeDays = 0;

                // 🔥 Gün mantığı: Worked / Sick / Unpaid
                var worked = request.WorkedDays ?? 0m;
                var sick = request.SickDays ?? 0m;

                decimal unpaid;
                if (request.UnpaidDays.HasValue)
                {
                    unpaid = request.UnpaidDays.Value;
                }
                else
                {
                    unpaid = (decimal)activeDays - (worked + sick);
                    if (unpaid < 0m) unpaid = 0m;
                }

                if (unpaid > activeDays)
                    unpaid = activeDays;

                var paidDays = (decimal)activeDays - unpaid;

                var factor = monthDays == 0 ? 0m : paidDays / (decimal)monthDays;

                var baseMonthly = employee.BruttoSalary;
                var proratedBase = Math.Round(baseMonthly * factor, 2);

                baseGross = proratedBase;
                monthlyHours = employee.MonthlyHours * factor;
                monthlyOvertimeHours = 0m;
            }
            else // "Hourly"
            {
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

                var normalPay = employee.HourlyRate * monthlyHours;
                overtimePayForLohn = employee.HourlyRate * monthlyOvertimeHours;

                baseGross = normalPay + overtimePayForLohn;
            }

            // ---- Bonus / Zulagen / Abzüge ----
            var bonus = request.Bonus;
            var extra = request.ExtraAllowance;
            var unpaidDed = request.UnpaidDeduction;
            var otherDed = request.OtherDeduction;

            var adjustments = bonus + extra - unpaidDed - otherDed;

            request.GrossMonthly = baseGross + adjustments;

            // ---- Modern hesap motorunu çağır ----
            var result = _calculator.Calculate(request);

            var gross = request.GrossMonthly;
            var net = result.NetToPay;
            var empDeductions = result.Employee.Total;
            var overtimePay = overtimePayForLohn;

            var emp = result.Employee;

            // Idempotent upsert (pro Mitarbeiter+Monat)
            var existing = await _context.Lohns
                .FirstOrDefaultAsync(l =>
                    l.EmployeeId == employee.Id &&
                    l.Year == year &&
                    l.Month == month);

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
                existing.Bonus = bonus;
                existing.ExtraAllowance = extra;
                existing.UnpaidDeduction = unpaidDed;
                existing.OtherDeduction = otherDed;
                existing.CreatedAt = DateTime.Now;

                // AN kesinti snapshot'ları
                existing.EmployeeAhvIvEo = emp.AHV_IV_EO;
                existing.EmployeeAlv = emp.ALV;
                existing.EmployeeNbu = emp.UVG_NBU;
                existing.EmployeeBvg = emp.BVG;
                existing.EmployeeQst = emp.WithholdingTax;

                // Snapshot parametreler
                existing.ApplyAHV = request.ApplyAHV;
                existing.ApplyALV = request.ApplyALV;
                existing.ApplyBVG = request.ApplyBVG;
                existing.ApplyNBU = request.ApplyNBU;
                existing.ApplyBU = request.ApplyBU;
                existing.ApplyFAK = request.ApplyFAK;
                existing.ApplyQST = request.ApplyQST;

                existing.PermitType = request.PermitType;
                existing.Canton = request.Canton;
                existing.ChurchMember = request.ChurchMember;
                existing.WithholdingTaxCode = request.WithholdingTaxCode;

                existing.Comment = result.Period;

                lohn = existing;
            }
            else
            {
                lohn = new Lohn
                {
                    EmployeeId = employee.Id,
                    Month = month,
                    Year = year,
                    BruttoSalary = gross,
                    NetSalary = net,
                    TotalDeductions = empDeductions,
                    OvertimePay = overtimePay,
                    HolidayAllowance = holidayAllowance,
                    ChildAllowance = childAllowance,
                    MonthlyHours = monthlyHours,
                    MonthlyOvertimeHours = monthlyOvertimeHours,
                    Bonus = bonus,
                    ExtraAllowance = extra,
                    UnpaidDeduction = unpaidDed,
                    OtherDeduction = otherDed,
                    CreatedAt = DateTime.Now,
                    IsFinal = false,

                    // AN kesinti snapshot'ları
                    EmployeeAhvIvEo = emp.AHV_IV_EO,
                    EmployeeAlv = emp.ALV,
                    EmployeeNbu = emp.UVG_NBU,
                    EmployeeBvg = emp.BVG,
                    EmployeeQst = emp.WithholdingTax,

                    // Snapshot parametreler
                    ApplyAHV = request.ApplyAHV,
                    ApplyALV = request.ApplyALV,
                    ApplyBVG = request.ApplyBVG,
                    ApplyNBU = request.ApplyNBU,
                    ApplyBU = request.ApplyBU,
                    ApplyFAK = request.ApplyFAK,
                    ApplyQST = request.ApplyQST,

                    PermitType = request.PermitType,
                    Canton = request.Canton,
                    ChurchMember = request.ChurchMember,
                    WithholdingTaxCode = request.WithholdingTaxCode,

                    Comment = null
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

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrerechnung wurde finalisiert.");
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
        // Basit versiyon: Seçilen ay için TÜM Lohn kayıtlarını döner
        // =========================================================
        [HttpGet("by-company/{companyId:int}/monthly")]
        public async Task<ActionResult<ApiResponse<List<CompanyMonthlyLohnDto>>>> GetByCompanyMonthly(
            int companyId,
            [FromQuery] string? period = null,
            CancellationToken ct = default)
        {
            int year;
            int month;

            if (IsValidPeriod(period))
            {
                (year, month) = ParsePeriod(period!);
            }
            else
            {
                var today = DateTime.Today;
                year = today.Year;
                month = today.Month;
            }

            var loehne = await _context.Lohns
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l =>
                    l.Employee.CompanyId == companyId &&
                    l.Year == year &&
                    l.Month == month)
                .OrderBy(l => l.Employee.LastName)
                .ThenBy(l => l.Employee.FirstName)
                .ToListAsync(ct);

            var list = loehne
                .Select(x => new CompanyMonthlyLohnDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null
                        ? (x.Employee.FirstName + " " + x.Employee.LastName).Trim()
                        : ("#" + x.EmployeeId),
                    Year = x.Year,
                    Month = x.Month,
                    BruttoSalary = x.BruttoSalary,
                    NetSalary = x.NetSalary,
                    TotalDeductions = x.TotalDeductions,
                    IsFinal = x.IsFinal
                })
                .ToList();

            return ApiResponse<List<CompanyMonthlyLohnDto>>.Ok(
                list,
                "Löhne für den ausgewählten Monat wurden erfolgreich geladen."
            );
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
