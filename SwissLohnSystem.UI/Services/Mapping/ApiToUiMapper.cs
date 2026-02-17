using System;
using System.Collections.Generic;
using System.Linq;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;

namespace SwissLohnSystem.UI.Services.Mapping
{
    /// <summary>
    /// API'den gelen DTO’ları UI DTO’larına dönüştürür ve zenginleştirir.
    /// </summary>
    public static class ApiToUiMapper
    {
        // =========================
        // Company
        // =========================
        public static CompanyDto ToUi(this CompanyDto api) => new CompanyDto
        {
            Id = api.Id,
            Name = (api.Name ?? "").Trim(),
            Canton = (api.Canton ?? "").Trim(),

            Address = string.IsNullOrWhiteSpace(api.Address) ? null : api.Address.Trim(),
            Email = string.IsNullOrWhiteSpace(api.Email) ? null : api.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(api.Phone) ? null : api.Phone.Trim(),
            TaxNumber = string.IsNullOrWhiteSpace(api.TaxNumber) ? null : api.TaxNumber.Trim(),

            DefaultBvgPlanCode = string.IsNullOrWhiteSpace(api.DefaultBvgPlanCode)
                ? null
                : api.DefaultBvgPlanCode.Trim()
        };

        public static CompanyListItemDto ToListItem(this CompanyDto c, int? employeeCount = null)
            => new CompanyListItemDto
            {
                Id = c.Id,
                Name = (c.Name ?? "").Trim(),
                Canton = (c.Canton ?? "").Trim(),
                Email = string.IsNullOrWhiteSpace(c.Email) ? null : c.Email.Trim(),
                EmployeeCount = employeeCount,
                DefaultBvgPlanCode = string.IsNullOrWhiteSpace(c.DefaultBvgPlanCode) ? null : c.DefaultBvgPlanCode.Trim()
            };

        /// <summary>
        /// Companies/Details sayfası için birleşik view-model.
        /// </summary>
        public static CompanyDetailsDto BuildDetails(CompanyDto company, IEnumerable<EmployeeDto> employees)
            => new CompanyDetailsDto
            {
                Company = company.ToUi(),
                Employees = employees ?? Enumerable.Empty<EmployeeDto>(),
                DefaultBvgPlanCode = string.IsNullOrWhiteSpace(company.DefaultBvgPlanCode) ? null : company.DefaultBvgPlanCode.Trim()
            };

        // =========================
        // Employees
        // =========================
        public static EmployeeListItemDto ToListItem(this EmployeeDto e)
        {
            var fullName = $"{e.FirstName} {e.LastName}".Trim();

            return new EmployeeListItemDto
            {
                Id = e.Id,
                CompanyId = e.CompanyId,
                FirstName = fullName, // mevcut UI DTO’n böyle kullanıyor
                Email = string.IsNullOrWhiteSpace(e.Email) ? null : e.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(e.Phone) ? null : e.Phone.Trim(),
                Position = string.IsNullOrWhiteSpace(e.Position) ? null : e.Position.Trim(),
                Active = e.Active
            };
        }

        // =========================
        // Lohn – Firma aylık liste satırı
        // =========================
        public static CompanyMonthlyLohnDto ToCompanyMonthlyRow(this LohnDto l, string? employeeName = null)
            => new CompanyMonthlyLohnDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? $"#{l.EmployeeId}" : employeeName,
                Month = l.Month,
                Year = l.Year,
                BruttoSalary = l.BruttoSalary,
                NetSalary = l.NetSalary,
                TotalDeductions = l.TotalDeductions,
                IsFinal = l.IsFinal
            };

        public static IEnumerable<CompanyMonthlyLohnDto> ToCompanyMonthlyRows(
            this IEnumerable<LohnDto> loehne,
            IEnumerable<EmployeeDto> employees)
        {
            var nameById = (employees ?? Array.Empty<EmployeeDto>())
                .GroupBy(e => e.Id)
                .ToDictionary(
                    g => g.Key,
                    g => $"{g.First().FirstName} {g.First().LastName}".Trim()
                );

            foreach (var l in (loehne ?? Array.Empty<LohnDto>()))
            {
                var employeeName = nameById.TryGetValue(l.EmployeeId, out var n) ? n : $"#{l.EmployeeId}";
                yield return l.ToCompanyMonthlyRow(employeeName);
            }
        }

        // -------------------------
        // ToDetails (eski imza kalsın)
        // -------------------------
        public static LohnDetailsDto ToDetails(this LohnDto l, EmployeeDto? e = null, CompanyDto? c = null)
            => ToDetails(l, e, c, null);

        // -------------------------
        // ToDetails (Settings rateByName destekli overload)
        // -------------------------
        public static LohnDetailsDto ToDetails(
            this LohnDto l,
            EmployeeDto? e,
            CompanyDto? c,
            IDictionary<string, decimal>? rateByName)
        {
            var details = new LohnDetailsDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                Month = l.Month,
                Year = l.Year,

                BruttoSalary = l.BruttoSalary,
                TotalDeductions = l.TotalDeductions,
                NetSalary = l.NetSalary,

                ChildAllowance = l.ChildAllowance,
                HolidayAllowance = l.HolidayAllowance,
                OvertimePay = l.OvertimePay,

                Bonus = l.Bonus,
                ExtraAllowance = l.ExtraAllowance,
                UnpaidDeduction = l.UnpaidDeduction,
                OtherDeduction = l.OtherDeduction,

                MonthlyHours = l.MonthlyHours,
                MonthlyOvertimeHours = l.MonthlyOvertimeHours,

                CreatedAt = l.CreatedAt,
                IsFinal = l.IsFinal,

                EmployeeName = e is null ? null : $"{e.FirstName} {e.LastName}".Trim(),

                // Company alanları (Employee yoksa Lohn/Company’den fallback)
                CompanyId = (e?.CompanyId > 0) ? e.CompanyId : l.CompanyId,
                CompanyName = c?.Name,
                CompanyAddress = c?.Address,
                CompanyPhone = c?.Phone,
                CompanyEmail = c?.Email,
                CompanyCanton = c?.Canton,

                EmployeeAddress = e?.Address,
                EmployeeZip = e?.Zip,
                EmployeeCity = e?.City,
                EmployeeAhvNumber = e?.AHVNumber,
                EmployeePhone = e?.Phone,

                ApplyAHV = l.ApplyAHV,
                ApplyALV = l.ApplyALV,
                ApplyBVG = l.ApplyBVG,
                ApplyNBU = l.ApplyNBU,
                ApplyBU = l.ApplyBU,
                ApplyFAK = l.ApplyFAK,
                ApplyQST = l.ApplyQST,

                PermitType = l.PermitType,
                Canton = l.Canton,
                ChurchMember = l.ChurchMember,
                WithholdingTaxCode = l.WithholdingTaxCode,

                HolidayRate = l.HolidayRate,
                HolidayEligible = l.HolidayEligible,

                Comment = l.Comment,

                EmployeeAhvIvEo = l.EmployeeAhvIvEo,
                EmployeeAlv = l.EmployeeAlv,
                EmployeeNbu = l.EmployeeNbu,
                EmployeeBvg = l.EmployeeBvg,
                EmployeeQst = l.EmployeeQst,

                EmployerAhvIvEo = l.EmployerAhvIvEo,
                EmployerAlv = l.EmployerAlv,
                EmployerBu = l.EmployerBu,
                EmployerBvg = l.EmployerBvg,
                EmployerFak = l.EmployerFak,
            };

            var bruttoBase = details.BruttoSalary;

            // ✅ Satz mantığı: Settings varsa onu göster.
            // - Settings bazen 0.053 (oran) bazen 5.3 (yüzde) olabilir → normalize et.
            // - Settings yoksa amount/base ile yüzde hesapla.
            decimal? RateFromSettingsOrCalc(string name, decimal @base, decimal amount)
            {
                if (rateByName != null && rateByName.TryGetValue(name, out var r))
                {
                    return r <= 1m ? r * 100m : r; // 0.053 => 5.3 (%)
                }

                if (@base > 0m && amount != 0m)
                    return Math.Round((amount / @base) * 100m, 4);

                return null;
            }

            details.Items = new List<LohnSlipItemDto>
            {
                // Earnings
                new()
                {
                    Group = LohnSlipGroup.Earnings,
                    Title = "Bruttolohn",
                    Side = "—",
                    Base = bruttoBase,
                    Rate = null,
                    RateText = null,
                    Amount = details.BruttoSalary,
                    SortOrder = 10
                },

                // AN
                new()
                {
                    Group = LohnSlipGroup.DeductionsEmployee,
                    Title = "AHV / IV / EO",
                    Side = "AN",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("AHV_AN_RATE", bruttoBase, details.EmployeeAhvIvEo),
                    RateText = null,
                    Amount = details.EmployeeAhvIvEo,
                    SortOrder = 110
                },
                new()
                {
                    Group = LohnSlipGroup.DeductionsEmployee,
                    Title = "ALV",
                    Side = "AN",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("ALV_AN_RATE", bruttoBase, details.EmployeeAlv),
                    RateText = null,
                    Amount = details.EmployeeAlv,
                    SortOrder = 120
                },
                new()
                {
                    Group = LohnSlipGroup.DeductionsEmployee,
                    Title = "NBU",
                    Side = "AN",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("NBU_AN_RATE", bruttoBase, details.EmployeeNbu),
                    RateText = null,
                    Amount = details.EmployeeNbu,
                    SortOrder = 130
                },
                new()
                {
                    Group = LohnSlipGroup.DeductionsEmployee,
                    Title = "BVG",
                    Side = "AN",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("BVG_AN_RATE", bruttoBase, details.EmployeeBvg),
                    RateText = null,
                    Amount = details.EmployeeBvg,
                    SortOrder = 140
                },
                new()
                {
                    Group = LohnSlipGroup.DeductionsEmployee,
                    Title = "Quellensteuer (QST)",
                    Side = "AN",
                    Base = bruttoBase,
                    Rate = null,
                    RateText = string.IsNullOrWhiteSpace(details.WithholdingTaxCode)
                        ? null
                        : $"{details.WithholdingTaxCode} / {details.Canton}".Trim(' ', '/'),
                    Amount = details.EmployeeQst,
                    SortOrder = 150
                },

                // AG
                new()
                {
                    Group = LohnSlipGroup.ContributionsEmployer,
                    Title = "AHV / IV / EO",
                    Side = "AG",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("AHV_AG_RATE", bruttoBase, details.EmployerAhvIvEo),
                    RateText = null,
                    Amount = details.EmployerAhvIvEo,
                    SortOrder = 210
                },
                new()
                {
                    Group = LohnSlipGroup.ContributionsEmployer,
                    Title = "ALV",
                    Side = "AG",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("ALV_AG_RATE", bruttoBase, details.EmployerAlv),
                    RateText = null,
                    Amount = details.EmployerAlv,
                    SortOrder = 220
                },
                new()
                {
                    Group = LohnSlipGroup.ContributionsEmployer,
                    Title = "BU",
                    Side = "AG",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("BU_AG_RATE", bruttoBase, details.EmployerBu),
                    RateText = null,
                    Amount = details.EmployerBu,
                    SortOrder = 230
                },
                new()
                {
                    Group = LohnSlipGroup.ContributionsEmployer,
                    Title = "BVG",
                    Side = "AG",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("BVG_AG_RATE", bruttoBase, details.EmployerBvg),
                    RateText = null,
                    Amount = details.EmployerBvg,
                    SortOrder = 240
                },
                new()
                {
                    Group = LohnSlipGroup.ContributionsEmployer,
                    Title = "FAK",
                    Side = "AG",
                    Base = bruttoBase,
                    Rate = RateFromSettingsOrCalc("FAK_AG_RATE", bruttoBase, details.EmployerFak),
                    RateText = null,
                    Amount = details.EmployerFak,
                    SortOrder = 250
                },
            }
            .Where(x => x.Amount != 0m)
            .OrderBy(x => x.Group)
            .ThenBy(x => x.SortOrder)
            .ToList();

            return details;
        }
    }
}
