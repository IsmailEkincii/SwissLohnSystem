using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;
using System;

namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class EfSettingsProvider : ISettingsProvider
    {
        private readonly ApplicationDbContext _db;
        public EfSettingsProvider(ApplicationDbContext db) => _db = db;

        public EffectivePayrollSettings GetEffectiveSettings(int companyId, string canton, string? bvgPlanCode)
        {
            var dict = _db.Settings
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

            decimal GetDec(string key, decimal fallback = 0m)
            {
                if (!dict.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                    return fallback;

                var s = v.Trim().Replace(',', '.');

                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return d;

                return fallback;
            }

            var cfg = new EffectivePayrollSettings
            {
                AhvEmployee = GetDec("AHV_AN_RATE", 0.0435m),
                AhvEmployer = GetDec("AHV_AG_RATE", 0.0435m),

                AlvRateTotal = GetDec("ALV_TOTAL_RATE", 0.022m),
                AlvEmployeeShare = GetDec("ALV_AN_SHARE", 0.5m),
                AlvEmployerShare = GetDec("ALV_AG_SHARE", 0.5m),
                AlvAnnualCap = GetDec("ALV_ANNUAL_CAP", 148200m),

                UvgCapAnnual = GetDec("UVG_CAP_ANNUAL", 148200m),
                UvgNbuMinWeeklyHours = GetDec("UVG_NBU_MIN_WEEKLY_HOURS", 8m),
                UvgNbuEmployeeRate = GetDec("NBU_AN_RATE", 0.012m),
                UvgBuEmployerRate = GetDec("BU_AG_RATE", 0.008m),

                FakEmployerRate = GetDec("FAK_AG_RATE", 0.02m),

                KtgMEmployeeRate = GetDec("KTG_M_AN_RATE", 0m),
                KtgMEmployerRate = GetDec("KTG_M_AG_RATE", 0m),
                KtgFEmployeeRate = GetDec("KTG_F_AN_RATE", 0m),
                KtgFEmployerRate = GetDec("KTG_F_AG_RATE", 0m),

                AhvAdminCostRate = GetDec("AHV_ADMIN_COST_RATE", 0m),

                IntermediateRoundingStep = GetDec("ROUND_INTERMEDIATE_STEP", 0.01m),
                WithholdingRoundingStep = GetDec("ROUND_WITHHOLDING_STEP", 0.05m),
                FinalRoundingStep = GetDec("ROUND_FINAL_STEP", 0.01m),
            };

            // canton/bvgPlanCode şu an config üzerinde ayrıca override etmiyoruz.
            // İstersen ileride canton bazlı settings key prefix yaparız.

            return cfg;
        }

        public QstTariff? GetQstTariff(int companyId, string canton, string code, string permitType, bool churchMember, decimal income)
        {
            canton = (canton ?? "ZH").Trim().ToUpperInvariant();
            code = code.Trim().ToUpperInvariant();
            permitType = permitType.Trim().ToUpperInvariant();

            return _db.QstTariffs
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId
                            && x.Canton == canton
                            && x.Code == code
                            && x.PermitType == permitType
                            && x.ChurchMember == churchMember
                            && x.IncomeFrom <= income
                            && x.IncomeTo >= income)
                .OrderByDescending(x => x.IncomeFrom)
                .FirstOrDefault();
        }
    }
}
