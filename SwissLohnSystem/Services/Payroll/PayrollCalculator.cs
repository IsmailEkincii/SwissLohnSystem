using System;
using System.Collections.Generic;
using System.Linq;
using SwissLohnSystem.API.DTOs.Payroll;

namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class PayrollCalculator : IPayrollCalculator
    {
        private readonly ISettingsProvider _settings;
        public PayrollCalculator(ISettingsProvider settings) => _settings = settings;

        public PayrollResponseDto Calculate(PayrollRequestDto req)
        {
            var cfg = _settings.GetEffectiveSettings(req.Canton ?? "ZH");

            decimal RoundStep(decimal val, decimal step)
            {
                if (step == 0.01m)
                    return Math.Round(val, 2, MidpointRounding.AwayFromZero);

                var factor = 1m / step; // örn. 0.05 => 20
                return Math.Round(val * factor, 0, MidpointRounding.AwayFromZero) / factor;
            }

            var gross = req.GrossMonthly;
            var annualGross = gross * 12m;

            // AHV/IV/EO
            var ahvEmp = RoundStep(gross * cfg.AhvEmployee, cfg.IntermediateRoundingStep);
            var ahvEr = RoundStep(gross * cfg.AhvEmployer, cfg.IntermediateRoundingStep);

            // ALV (cap/12; YTD sonraya)
            var alvBase = Math.Min(gross, cfg.AlvAnnualCap / 12m);
            var alvEmp = RoundStep(alvBase * cfg.AlvRateTotal * cfg.AlvEmployeeShare, cfg.IntermediateRoundingStep);
            var alvEr = RoundStep(alvBase * cfg.AlvRateTotal * cfg.AlvEmployerShare, cfg.IntermediateRoundingStep);

            // UVG
            var nbuEmp = (req.WeeklyHours >= cfg.UvgNbuMinWeeklyHours)
                ? RoundStep(gross * cfg.UvgNbuEmployeeRate, cfg.IntermediateRoundingStep)
                : 0m;

            var buEr = RoundStep(gross * cfg.UvgBuEmployerRate, cfg.IntermediateRoundingStep);

            // BVG
            decimal bvgEmp = 0m, bvgEr = 0m;
            if (annualGross >= cfg.BvgEntryThresholdAnnual)
            {
                var coordAnnual = Math.Max(0m, Math.Min(cfg.BvgUpperLimitAnnual, annualGross) -
                    (req.BvgPlan?.CoordinationDeductionAnnualOverride ?? cfg.BvgCoordinationDedAnnual));

                var coordMonthly = coordAnnual / 12m;
                var empRate = req.BvgPlan?.EmployeeRateOverride ?? cfg.BvgEmployeeRate;
                var erRate = req.BvgPlan?.EmployerRateOverride ?? cfg.BvgEmployerRate;

                bvgEmp = RoundStep(coordMonthly * empRate, cfg.IntermediateRoundingStep);
                bvgEr = RoundStep(coordMonthly * erRate, cfg.IntermediateRoundingStep);
            }

            // Quellensteuer (henüz tablo yok)
            var qst = 0m;

            // FAK (işveren)
            var fakEr = RoundStep(gross * cfg.FakEmployerRate, cfg.IntermediateRoundingStep);

            // Item listesi
            var items = new List<PayrollItemDto>
            {
                new() { Code="AHV",    Title="AHV/IV/EO (Arbeitnehmer)", Type="deduction",    Amount=ahvEmp, Basis="Brutto",             Rate=cfg.AhvEmployee, Side="employee" },
                new() { Code="ALV",    Title="ALV (Arbeitnehmer)",       Type="deduction",    Amount=alvEmp, Basis="Brutto bis Cap",     Rate=cfg.AlvRateTotal * cfg.AlvEmployeeShare, Side="employee" },
                new() { Code="NBU",    Title="Nichtberufsunfall (AN)",   Type="deduction",    Amount=nbuEmp, Basis="Brutto",             Rate=cfg.UvgNbuEmployeeRate, Side="employee" },
                new() { Code="BVG",    Title="BVG (Arbeitnehmer)",       Type="deduction",    Amount=bvgEmp, Basis="Koord. Lohn",        Rate=req.BvgPlan?.EmployeeRateOverride ?? cfg.BvgEmployeeRate, Side="employee" },
                new() { Code="QST",    Title="Quellensteuer",            Type="deduction",    Amount=qst,    Basis="Tarif (v1:0)",        Rate=0m, Side="employee" },

                new() { Code="AHV_ER", Title="AHV/IV/EO (Arbeitgeber)",  Type="contribution", Amount=ahvEr,  Basis="Brutto",             Rate=cfg.AhvEmployer, Side="employer" },
                new() { Code="ALV_ER", Title="ALV (Arbeitgeber)",        Type="contribution", Amount=alvEr,  Basis="Brutto bis Cap",     Rate=cfg.AlvRateTotal * cfg.AlvEmployerShare, Side="employer" },
                new() { Code="BU",     Title="Berufsunfall (AG)",        Type="contribution", Amount=buEr,   Basis="Brutto",             Rate=cfg.UvgBuEmployerRate, Side="employer" },
                new() { Code="BVG_ER", Title="BVG (Arbeitgeber)",        Type="contribution", Amount=bvgEr,  Basis="Koord. Lohn",        Rate=req.BvgPlan?.EmployerRateOverride ?? cfg.BvgEmployerRate, Side="employer" },
                new() { Code="FAK",    Title="Familienausgleichskasse",  Type="contribution", Amount=fakEr,  Basis="Brutto",             Rate=cfg.FakEmployerRate, Side="employer" }
            };

            // Hesaplama: Net maaş ve toplam işveren maliyeti
            var empDeductions = items.Where(i => i.Side == "employee" && i.Type == "deduction").Sum(i => i.Amount);
            var netRaw = gross - empDeductions;
            if (netRaw > gross) netRaw = gross; // koruma

            var netRounded = RoundStep(netRaw, cfg.FinalRoundingStep);
            var employerTotal = items.Where(i => i.Side == "employer").Sum(i => i.Amount);

            return new PayrollResponseDto
            {
                Period = $"{req.Period:yyyy-MM}",
                NetToPay = netRounded,
                EmployerTotalCost = employerTotal,
                Employee = new MoneyBreakdownDto
                {
                    AHV_IV_EO = ahvEmp,
                    ALV = alvEmp,
                    UVG_NBU = nbuEmp,
                    BVG = bvgEmp,
                    WithholdingTax = qst,
                    Other = 0m
                },
                Employer = new MoneyBreakdownDto
                {
                    AHV_IV_EO = ahvEr,
                    ALV = alvEr,
                    UVG_NBU = 0m,
                    BVG = bvgEr,
                    WithholdingTax = 0m,
                    Other = fakEr
                },
                Items = items
            };
        }
    }
}
