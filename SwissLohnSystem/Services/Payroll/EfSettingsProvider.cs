using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class EfSettingsProvider : ISettingsProvider
    {
        private readonly ApplicationDbContext _db;
        public EfSettingsProvider(ApplicationDbContext db) => _db = db;

        public PayrollSettingsSnapshot GetEffectiveSettings(string canton)
        {
            var dict = _db.Settings.AsNoTracking().ToDictionary(s => s.Name, s => s.Value);

            decimal GetRate(string key, decimal def)
                => dict.TryGetValue(key, out var v) ? (v > 1m ? v / 100m : v) : def;

            decimal GetDec(string key, decimal def)
                => dict.TryGetValue(key, out var v) ? v : def;

            int GetInt(string key, int def)
                => dict.TryGetValue(key, out var v) ? (int)decimal.Round(v, 0) : def;

            return new PayrollSettingsSnapshot
            {
                IntermediateRoundingStep = GetDec("Rounding.Intermediate", 0.01m),
                FinalRoundingStep = GetDec("Rounding.Final", 0.05m),
                WithholdingRoundingStep = GetDec("Rounding.Withholding", 0.05m),

                AhvEmployee = GetRate("AHV.Employee", 0.053m),
                AhvEmployer = GetRate("AHV.Employer", 0.053m),

                AlvRateTotal = GetRate("ALV.Total", 0.022m),
                AlvEmployeeShare = GetDec("ALV.Split.Employee", 0.5m),
                AlvEmployerShare = GetDec("ALV.Split.Employer", 0.5m),
                AlvAnnualCap = GetDec("ALV.CapAnnual", 148_200m),

                UvgBuEmployerRate = GetRate("UVG.BU.Employer", 0.0125m),
                UvgNbuEmployeeRate = GetRate("UVG.NBU.Employee", 0.009m),
                UvgNbuMinWeeklyHours = GetInt("UVG.NBU.MinWeeklyHours", 8),

                BvgEntryThresholdAnnual = GetDec("BVG.EntryThresholdAnnual", 22_680m),
                BvgCoordinationDedAnnual = GetDec("BVG.CoordinationDedAnnual", 26_460m),
                BvgUpperLimitAnnual = GetDec("BVG.UpperLimitAnnual", 90_720m),
                BvgEmployeeRate = GetRate("BVG.EmployeeRate", 0.04m),
                BvgEmployerRate = GetRate("BVG.EmployerRate", 0.05m),

                FakEmployerRate = GetRate("FAK.EmployerRate", 0.012m),
            };
        }

        // 💡 Yeni: QST tarifini DB’den bul
        public QstTariff? GetQstTariff(
            string canton,
            string? code,
            string permitType,
            bool churchMember,
            decimal grossMonthly)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            canton = canton.ToUpperInvariant();
            code = code.ToUpperInvariant();
            permitType = permitType.ToUpperInvariant();

            return _db.QstTariffs
                .AsNoTracking()
                .Where(t =>
                    t.Canton == canton &&
                    t.Code == code &&
                    t.PermitType == permitType &&
                    t.ChurchMember == churchMember &&
                    t.IncomeFrom <= grossMonthly &&
                    t.IncomeTo >= grossMonthly)
                .OrderBy(t => t.IncomeFrom)
                .FirstOrDefault();
        }
    }
}
