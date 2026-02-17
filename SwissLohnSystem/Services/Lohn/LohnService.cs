using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Services.Payroll;

namespace SwissLohnSystem.API.Services.Lohn
{
    public sealed class LohnService : ILohnService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayrollCalculator _calculator;

        public LohnService(ApplicationDbContext db, IPayrollCalculator calculator)
        {
            _db = db;
            _calculator = calculator;
        }

        public async Task<LohnDto> CalculateAsync(PayrollRequestDto request, CancellationToken ct = default)
        {
            if (request.CompanyId <= 0) throw new InvalidOperationException("CompanyId is required.");
            if (request.EmployeeId <= 0) throw new InvalidOperationException("EmployeeId is required.");
            if (request.Period == default) throw new InvalidOperationException("Period is required.");

            var year = request.Period.Year;
            var month = request.Period.Month;

            // Employee + Company validate
            var employee = await _db.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, ct);

            if (employee is null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.CompanyId != request.CompanyId)
                throw new InvalidOperationException("Employee does not belong to this company.");

            // Duplicate period check
            var existing = await _db.Lohns
                .FirstOrDefaultAsync(l =>
                    l.EmployeeId == request.EmployeeId &&
                    l.Year == year &&
                    l.Month == month,
                    ct);

            if (existing != null)
                throw new InvalidOperationException("Lohn for this period already exists.");

            // Fallbacks (company canton)
            request.Canton ??= employee.Canton ?? employee.Company.Canton;
            request.PermitType ??= employee.PermitType;
            request.WithholdingTaxCode ??= employee.WithholdingTaxCode;
            request.Gender ??= employee.Gender;

            // ✅ IMPORTANT: request.GrossMonthly brüt olmalı (13th dahil)
            if (request.GrossMonthly <= 0m)
                throw new InvalidOperationException("GrossMonthly must be > 0. GrossMonthly must include 13th if enabled.");

            var result = _calculator.Calculate(request);

            // Total AN deductions (employee side deductions)
            var empDeductions = result.Items
                .Where(i => i.Side == "employee" && i.Type == "deduction")
                .Sum(i => i.Amount);

            // ✅ Snapshot helper’ları: items tek doğru kaynak
            decimal Emp(string code) =>
                result.Items.FirstOrDefault(i => i.Code == code && i.Side == "employee")?.Amount ?? 0m;

            decimal Er(string code) =>
                result.Items.FirstOrDefault(i => i.Code == code && i.Side == "employer")?.Amount ?? 0m;

            var lohn = new Models.Lohn
            {
                EmployeeId = request.EmployeeId,
                Year = request.Period.Year,
                Month = request.Period.Month,

                BruttoSalary = request.GrossMonthly,
                NetSalary = result.NetToPay,
                TotalDeductions = empDeductions,

                ChildAllowance = request.ChildAllowance,

                // PayrollRequestDto’da yok -> entity’de varsa 0
                HolidayAllowance = 0m,
                OvertimePay = 0m,
                MonthlyHours = 0m,
                MonthlyOvertimeHours = 0m,

                Bonus = request.Bonus,
                ExtraAllowance = request.ExtraAllowance,
                UnpaidDeduction = request.UnpaidDeduction,
                OtherDeduction = request.OtherDeduction,

                PrivateBenefitAmount = request.PrivateBenefitAmount,
                ManualAdjustment = request.ManualAdjustment,

                CreatedAt = DateTime.UtcNow,
                IsFinal = false,

                PauschalExpenses = request.PauschalExpenses,
                EffectiveExpenses = request.EffectiveExpenses,
                ShortTimeWorkDeduction = request.ShortTimeWorkDeduction,

                Include13thSalary = request.Include13thSalary,
                ThirteenthSalaryAmount = request.ThirteenthSalaryAmount,

                CanteenDays = request.CanteenDays,
                CanteenDailyRate = request.CanteenDailyRate,
                CanteenDeduction = result.Items.FirstOrDefault(i => i.Code == "CANTEEN")?.Amount ?? 0m,

                ApplyAHV = request.ApplyAHV,
                ApplyALV = request.ApplyALV,
                ApplyBVG = request.ApplyBVG,
                ApplyNBU = request.ApplyNBU,
                ApplyBU = request.ApplyBU,
                ApplyFAK = request.ApplyFAK,
                ApplyQST = request.ApplyQST,
                ApplyKTG = request.ApplyKTG,

                Gender = request.Gender ?? "M",
                PermitType = request.PermitType,
                Canton = request.Canton,
                ChurchMember = request.ChurchMember,
                WithholdingTaxCode = request.WithholdingTaxCode,

                Comment = null,
                BvgPlanCodeUsed = request.BvgPlan?.PlanCode,

                // ===============================
                // Snapshots (items -> kesin doğru)
                // ===============================
                EmployeeAhvIvEo = Emp("AHV"),
                EmployeeAlv1 = Emp("ALV1"),
                EmployeeAlv2 = Emp("ALV2"),
                EmployeeNbu = Emp("NBU"),
                EmployeeBvg = Emp("BVG"),
                EmployeeKtg = Emp("KTG"),
                EmployeeQst = Emp("QST"),

                EmployerAhvIvEo = Er("AHV_ER"),
                EmployerAlv1 = Er("ALV1_ER"),
                EmployerAlv2 = Er("ALV2_ER"),
                EmployerBu = Er("BU"),
                EmployerBvg = Er("BVG_ER"),
                EmployerKtg = Er("KTG_ER"),
                EmployerFak = Er("FAK"),
                EmployerVk = Er("VK"),
            };

            _db.Lohns.Add(lohn);
            await _db.SaveChangesAsync(ct);

            var dto = lohn.ToDto();
            dto.Items = result.Items; // UI detay ekranı için (anlık)
            return dto;
        }

        public async Task FinalizeAsync(int lohnId, CancellationToken ct = default)
        {
            var lohn = await _db.Lohns.FirstOrDefaultAsync(l => l.Id == lohnId, ct);
            if (lohn is null)
                throw new InvalidOperationException("Lohn not found.");

            if (lohn.IsFinal)
                return;

            lohn.IsFinal = true;
            lohn.FinalizedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        // =====================================================
        // ✅ NEW: Firma bazlı aylık Lohn listesi (UI: Firma -> Löhne)
        // GET /api/lohn/by-company/{companyId}?year=2026&month=1
        // =====================================================
        public async Task<List<CompanyMonthlyLohnDto>> GetCompanyMonthlyAsync(int companyId, int year, int month, CancellationToken ct = default)
        {
            if (companyId <= 0) throw new InvalidOperationException("CompanyId is required.");
            if (year < 2000 || year > 2100) throw new InvalidOperationException("Invalid year.");
            if (month < 1 || month > 12) throw new InvalidOperationException("Invalid month.");

            // Company var mı kontrol
            var exists = await _db.Companies.AsNoTracking().AnyAsync(c => c.Id == companyId, ct);
            if (!exists)
                throw new InvalidOperationException("Company not found.");

            // ✅ CompanyMonthlyLohnDto senin projende “row dto”
            var rows = await (
                from l in _db.Lohns.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on l.EmployeeId equals e.Id
                where e.CompanyId == companyId && l.Year == year && l.Month == month
                orderby e.LastName, e.FirstName
                select new CompanyMonthlyLohnDto
                {
                    Id = l.Id,
                    EmployeeId = e.Id,
                    EmployeeName = (e.FirstName + " " + e.LastName).Trim(),
                    Month = l.Month,
                    Year = l.Year,
                    BruttoSalary = l.BruttoSalary,
                    NetSalary = l.NetSalary,
                    TotalDeductions = l.TotalDeductions,
                    IsFinal = l.IsFinal
                }
            ).ToListAsync(ct);

            return rows;
        }


        // =====================================================
        // ✅ NEW: Lohn Details (JSON) - UI: Detay sayfası
        // GET /api/lohn/{id}
        // =====================================================
        public async Task<LohnDetailsDto> GetDetailsAsync(int lohnId, CancellationToken ct = default)
        {
            var data = await (
                from l in _db.Lohns.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on l.EmployeeId equals e.Id
                join c in _db.Companies.AsNoTracking() on e.CompanyId equals c.Id
                where l.Id == lohnId
                select new
                {
                    Lohn = l,
                    EmployeeName = (e.FirstName + " " + e.LastName).Trim(),
                    CompanyId = c.Id,
                    CompanyName = c.Name
                }
            ).FirstOrDefaultAsync(ct);

            if (data is null)
                throw new InvalidOperationException("Lohn not found.");

            // entity -> details dto
            var dto = data.Lohn.ToDetailsDto();
            dto.EmployeeName = data.EmployeeName;
            dto.CompanyId = data.CompanyId;
            dto.CompanyName = data.CompanyName;

            // Items DB’de saklanmıyor => snapshot alanlarından “PDF/Detay için” liste üret
            dto.Items = BuildItemsFromSnapshots(dto);

            return dto;
        }

        private static List<PayrollItemDto> BuildItemsFromSnapshots(LohnDetailsDto d)
        {
            // Rate/Basis burada saklanmıyor => 0/null olarak veriyoruz.
            // UI’da kalem kalem göstermek ve PDF’de tablo basmak için yeterli.
            var list = new List<PayrollItemDto>();

            void AddEmp(string code, string title, decimal amount, string basis = "Snapshot")
            {
                if (amount == 0m) return;
                list.Add(new PayrollItemDto
                {
                    Code = code,
                    Title = title,
                    Type = "deduction",
                    Amount = amount,
                    Basis = basis,
                    Rate = 0m,
                    Side = "employee"
                });
            }

            void AddEr(string code, string title, decimal amount, string basis = "Snapshot")
            {
                if (amount == 0m) return;
                list.Add(new PayrollItemDto
                {
                    Code = code,
                    Title = title,
                    Type = "contribution",
                    Amount = amount,
                    Basis = basis,
                    Rate = 0m,
                    Side = "employer"
                });
            }

            AddEmp("AHV", "AHV/IV/EO (Arbeitnehmer)", d.EmployeeAhvIvEo);
            AddEmp("ALV1", "ALV1 (Arbeitnehmer)", d.EmployeeAlv1);
            AddEmp("ALV2", "ALV2 (Arbeitnehmer)", d.EmployeeAlv2);
            AddEmp("NBU", "Nichtberufsunfall (AN)", d.EmployeeNbu, "Brutto (Cap)");
            AddEmp("BVG", "BVG (Arbeitnehmer)", d.EmployeeBvg, "Fix/Plan");
            AddEmp("KTG", "KTG (Arbeitnehmer)", d.EmployeeKtg);
            AddEmp("CANTEEN", "Kantine", d.CanteenDeduction, "Tage x Satz");
            AddEmp("QST", $"Quellensteuer {d.WithholdingTaxCode}", d.EmployeeQst, "QST-Startbasis");

            AddEr("AHV_ER", "AHV/IV/EO (Arbeitgeber)", d.EmployerAhvIvEo);
            AddEr("ALV1_ER", "ALV1 (Arbeitgeber)", d.EmployerAlv1);
            AddEr("ALV2_ER", "ALV2 (Arbeitgeber)", d.EmployerAlv2);
            AddEr("BU", "Berufsunfall (AG)", d.EmployerBu, "Brutto (Cap)");
            AddEr("BVG_ER", "BVG (Arbeitgeber)", d.EmployerBvg, "Fix/Plan");
            AddEr("KTG_ER", "KTG (Arbeitgeber)", d.EmployerKtg);
            AddEr("FAK", "Familienausgleichskasse", d.EmployerFak);
            AddEr("VK", "Verwaltungskosten (VK)", d.EmployerVk, "AHV total");

            return list;
        }
        public async Task<LohnausweisDto> GetLohnausweisAsync(int employeeId, int year, CancellationToken ct = default)
        {
            if (employeeId <= 0) throw new InvalidOperationException("EmployeeId is required.");
            if (year < 2000 || year > 2100) throw new InvalidOperationException("Invalid year.");

            // Employee + Company
            var emp = await _db.Employees
                .AsNoTracking()
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

            if (emp is null)
                throw new InvalidOperationException("Employee not found.");

            // Year rows
            var rows = await _db.Lohns
                .AsNoTracking()
                .Where(l => l.EmployeeId == employeeId && l.Year == year)
                .OrderBy(l => l.Month)
                .ToListAsync(ct);

            // Eksik aylar / non-final kontrol
            var monthsPresent = rows.Select(r => r.Month).Distinct().ToHashSet();
            var missing = Enumerable.Range(1, 12).Where(m => !monthsPresent.Contains(m)).ToList();
            var nonFinal = rows.Where(r => !r.IsFinal).Select(r => r.Month).Distinct().ToList();

            // ✅ Lohnausweis “kesin olsun” dediğin için:
            // Finalize olmayan ay varsa IsComplete=false yapıyoruz.
            var isComplete = missing.Count == 0 && nonFinal.Count == 0;

            // Toplamlar (snapshot alanlarından)
            decimal Sum(Func<Models.Lohn, decimal> sel) => rows.Sum(sel);

            var dto = new LohnausweisDto
            {
                Year = year,
                IsComplete = isComplete,
                MissingMonths = missing,
                NonFinalMonths = nonFinal,

                EmployeeId = emp.Id,
                EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                EmployeeAddress = emp.Address,
                EmployeeZip = emp.Zip,
                EmployeeCity = emp.City,

                CompanyId = emp.CompanyId,
                CompanyName = emp.Company.Name,
                CompanyAddress = emp.Company.Address,
                CompanyPhone = emp.Company.Phone,
                CompanyEmail = emp.Company.Email,

                // 8 Bruttolohn total:
                // BruttoSalary zaten “GrossMonthly (13 dahil)” snapshot’ı
                BruttoTotal_8 = Sum(x => x.BruttoSalary),

                // 2.2 Private benefit
                PrivateBenefit_2_2 = Sum(x => x.PrivateBenefitAmount),

                // 9 social (employee)
                AhvIvEo_9 = Sum(x => x.EmployeeAhvIvEo),
                AlvTotal_9 = Sum(x => x.EmployeeAlv1 + x.EmployeeAlv2),
                Nbu_9 = Sum(x => x.EmployeeNbu),

                // 10 BVG employee
                Bvg_10 = Sum(x => x.EmployeeBvg),

                // 12 Quellensteuer
                Quellensteuer_12 = Sum(x => x.EmployeeQst),

                // 13 spesen
                PauschalSpesen_13 = Sum(x => x.PauschalExpenses),
                EffektivSpesen_13 = Sum(x => x.EffectiveExpenses),

                // 11 net total
                NetTotal_11 = Sum(x => x.NetSalary),

                // 15 remark (istersen employee/company remark alanın varsa oraya bağlarız)
                Remark_15 = null
            };

            // ✅ İstersen “kesin” kural: incomplete ise hata fırlat
            // müşteri “kesin olsun” dediği için çoğu firma bunu ister.
            // Şimdilik DTO içinde raporluyoruz. UI’da “PDF butonu disabled” yapacağız.
            return dto;
        }
    }
}


