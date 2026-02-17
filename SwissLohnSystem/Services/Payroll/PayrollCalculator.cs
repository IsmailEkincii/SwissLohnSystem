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

        private static string NormalizeGenderOrDefault(string? g)
        {
            if (string.IsNullOrWhiteSpace(g)) return "M";
            g = g.Trim().ToUpperInvariant();

            return g switch
            {
                "M" => "M",
                "F" => "F",
                "X" => "X",
                _ => "M" // güvenli default (istersen validation'da reject edebilirsin)
            };
        }

        public PayrollResponseDto Calculate(PayrollRequestDto req)
        {
            if (req.CompanyId <= 0)
                throw new InvalidOperationException("CompanyId required for company-scoped settings.");

            var planCode = req.BvgPlan?.PlanCode;
            var cfg = _settings.GetEffectiveSettings(req.CompanyId, req.Canton ?? "ZH", planCode);

            decimal RoundStep(decimal val, decimal step)
            {
                if (step <= 0m) step = 0.01m;

                if (step == 0.01m)
                    return Math.Round(val, 2, MidpointRounding.AwayFromZero);

                var factor = 1m / step;
                return Math.Round(val * factor, 0, MidpointRounding.AwayFromZero) / factor;
            }

            // ✅ IMPORTANT: gross burada zaten 13. maaş dahil olmalı (LohnService set ediyor)
            var gross = req.GrossMonthly;

            // Excel: Kantin AN deduction (net’ten düşer)
            var canteenDeductionRaw = req.GetCanteenDeduction();
            var canteenDeduction = RoundStep(Math.Max(0m, canteenDeductionRaw), cfg.IntermediateRoundingStep);

            // =========================================================
            // 1) AHV/IV/EO
            // =========================================================
            decimal ahvEmp = 0m, ahvEr = 0m;
            if (req.ApplyAHV)
            {
                ahvEmp = RoundStep(gross * cfg.AhvEmployee, cfg.IntermediateRoundingStep);
                ahvEr = RoundStep(gross * cfg.AhvEmployer, cfg.IntermediateRoundingStep);
            }

            // =========================================================
            // 2) ALV (ALV1 + ALV2)
            // =========================================================
            decimal alv1Emp = 0m, alv1Er = 0m;
            decimal alv2Emp = 0m, alv2Er = 0m;

            if (req.ApplyALV)
            {
                var capMonthly = cfg.AlvAnnualCap / 12m;

                var base1 = Math.Min(gross, capMonthly);
                alv1Emp = RoundStep(base1 * cfg.AlvRateTotal * cfg.AlvEmployeeShare, cfg.IntermediateRoundingStep);
                alv1Er = RoundStep(base1 * cfg.AlvRateTotal * cfg.AlvEmployerShare, cfg.IntermediateRoundingStep);

                if (cfg.Alv2RateTotal > 0m)
                {
                    var base2 = Math.Max(0m, gross - capMonthly);
                    alv2Emp = RoundStep(base2 * cfg.Alv2RateTotal * cfg.Alv2EmployeeShare, cfg.IntermediateRoundingStep);
                    alv2Er = RoundStep(base2 * cfg.Alv2RateTotal * cfg.Alv2EmployerShare, cfg.IntermediateRoundingStep);
                }
            }

            // =========================================================
            // 3) UVG (NBU / BU) + ✅ CAP
            // =========================================================
            var uvgCapMonthly = cfg.UvgCapAnnual / 12m;
            var uvgBase = Math.Min(gross, uvgCapMonthly);

            decimal nbuEmp = 0m;
            if (req.ApplyNBU && req.WeeklyHours >= cfg.UvgNbuMinWeeklyHours)
                nbuEmp = RoundStep(uvgBase * cfg.UvgNbuEmployeeRate, cfg.IntermediateRoundingStep);

            decimal buEr = 0m;
            if (req.ApplyBU)
                buEr = RoundStep(uvgBase * cfg.UvgBuEmployerRate, cfg.IntermediateRoundingStep);

            // =========================================================
            // 4) KTG (gender-based)
            // =========================================================
            decimal ktgEmp = 0m, ktgEr = 0m;
            if (req.ApplyKTG)
            {
                var g = NormalizeGenderOrDefault(req.Gender);

                // X gelirse hangi rate? (şimdilik M fallback)
                var isF = (g == "F");

                var empRate = isF ? cfg.KtgFEmployeeRate : cfg.KtgMEmployeeRate;
                var erRate = isF ? cfg.KtgFEmployerRate : cfg.KtgMEmployerRate;

                ktgEmp = RoundStep(gross * empRate, cfg.IntermediateRoundingStep);
                ktgEr = RoundStep(gross * erRate, cfg.IntermediateRoundingStep);
            }

            // =========================================================
            // 5) BVG (Excel: FIX)
            // =========================================================
            decimal bvgEmp = 0m, bvgEr = 0m;
            if (req.ApplyBVG)
            {
                bvgEmp = RoundStep(Math.Max(0m, req.BvgFixEmployee), cfg.IntermediateRoundingStep);
                bvgEr = RoundStep(Math.Max(0m, req.BvgFixEmployer), cfg.IntermediateRoundingStep);
            }

            // =========================================================
            // 6) QST (Quellensteuer) - ✅ Excel: START baz
            // =========================================================
            decimal qst = 0m;
            decimal qstRate = 0m;
            PayrollItemDto? qstItem = null;

            var qstStart =
                gross
                + req.ChildAllowance
                + req.PauschalExpenses
                - req.ShortTimeWorkDeduction;

            if (qstStart < 0m) qstStart = 0m;

            if (req.ApplyQST &&
                !string.IsNullOrWhiteSpace(req.Canton) &&
                !string.IsNullOrWhiteSpace(req.WithholdingTaxCode) &&
                !string.IsNullOrWhiteSpace(req.PermitType))
            {
                var tariff = _settings.GetQstTariff(
                    req.CompanyId,
                    req.Canton!,
                    req.WithholdingTaxCode!,
                    req.PermitType!,
                    req.ChurchMember,
                    qstStart
                );

                if (tariff is not null)
                {
                    qstRate = tariff.Rate;
                    qst = RoundStep(qstStart * qstRate, cfg.WithholdingRoundingStep);

                    qstItem = new PayrollItemDto
                    {
                        Code = "QST",
                        Title = $"Quellensteuer {tariff.Code}",
                        Type = "deduction",
                        Amount = qst,
                        Basis = "QST-Startbasis",
                        Rate = qstRate,
                        Side = "employee"
                    };
                }
                else
                {
                    qstItem = new PayrollItemDto
                    {
                        Code = "QST-MISS",
                        Title = "Quellensteuer-Tarif nicht gefunden",
                        Type = "info",
                        Amount = 0m,
                        Basis = "QST-Startbasis",
                        Rate = 0m,
                        Side = "employee"
                    };
                }
            }

            // =========================================================
            // 7) FAK (AG)
            // =========================================================
            decimal fakEr = 0m;
            if (req.ApplyFAK)
                fakEr = RoundStep(gross * cfg.FakEmployerRate, cfg.IntermediateRoundingStep);

            // =========================================================
            // 8) ✅ VK (AHV total * vkRate)
            // =========================================================
            decimal vkEr = 0m;
            if (cfg.AhvAdminCostRate > 0m && (ahvEmp + ahvEr) > 0m)
                vkEr = RoundStep((ahvEmp + ahvEr) * cfg.AhvAdminCostRate, cfg.IntermediateRoundingStep);

            // =========================================================
            // Items list
            // =========================================================
            var items = new List<PayrollItemDto>();

            // AN deductions
            items.Add(new PayrollItemDto { Code = "AHV", Title = "AHV/IV/EO (Arbeitnehmer)", Type = "deduction", Amount = ahvEmp, Basis = "Brutto", Rate = cfg.AhvEmployee, Side = "employee" });
            items.Add(new PayrollItemDto { Code = "ALV1", Title = "ALV1 (Arbeitnehmer)", Type = "deduction", Amount = alv1Emp, Basis = "Brutto bis Cap", Rate = cfg.AlvRateTotal * cfg.AlvEmployeeShare, Side = "employee" });
            items.Add(new PayrollItemDto { Code = "ALV2", Title = "ALV2 (Arbeitnehmer)", Type = "deduction", Amount = alv2Emp, Basis = "Brutto über Cap", Rate = cfg.Alv2RateTotal * cfg.Alv2EmployeeShare, Side = "employee" });
            items.Add(new PayrollItemDto { Code = "NBU", Title = "Nichtberufsunfall (AN)", Type = "deduction", Amount = nbuEmp, Basis = $"Brutto (Cap {uvgCapMonthly:N2})", Rate = cfg.UvgNbuEmployeeRate, Side = "employee" });

            if (req.ApplyKTG)
            {
                var g = NormalizeGenderOrDefault(req.Gender);
                var r = (g == "F") ? cfg.KtgFEmployeeRate : cfg.KtgMEmployeeRate;

                items.Add(new PayrollItemDto
                {
                    Code = "KTG",
                    Title = "KTG (Arbeitnehmer)",
                    Type = "deduction",
                    Amount = ktgEmp,
                    Basis = "Brutto",
                    Rate = r,
                    Side = "employee"
                });
            }

            items.Add(new PayrollItemDto { Code = "BVG", Title = "BVG (Arbeitnehmer) (Fix)", Type = "deduction", Amount = bvgEmp, Basis = "Fix", Rate = 0m, Side = "employee" });

            if (canteenDeduction > 0m)
            {
                items.Add(new PayrollItemDto
                {
                    Code = "CANTEEN",
                    Title = "Kantine",
                    Type = "deduction",
                    Amount = canteenDeduction,
                    Basis = "Tage x Satz",
                    Rate = 0m,
                    Side = "employee"
                });
            }

            if (qstItem is not null)
                items.Add(qstItem);

            // AG contributions
            items.Add(new PayrollItemDto { Code = "AHV_ER", Title = "AHV/IV/EO (Arbeitgeber)", Type = "contribution", Amount = ahvEr, Basis = "Brutto", Rate = cfg.AhvEmployer, Side = "employer" });
            items.Add(new PayrollItemDto { Code = "ALV1_ER", Title = "ALV1 (Arbeitgeber)", Type = "contribution", Amount = alv1Er, Basis = "Brutto bis Cap", Rate = cfg.AlvRateTotal * cfg.AlvEmployerShare, Side = "employer" });
            items.Add(new PayrollItemDto { Code = "ALV2_ER", Title = "ALV2 (Arbeitgeber)", Type = "contribution", Amount = alv2Er, Basis = "Brutto über Cap", Rate = cfg.Alv2RateTotal * cfg.Alv2EmployerShare, Side = "employer" });
            items.Add(new PayrollItemDto { Code = "BU", Title = "Berufsunfall (AG)", Type = "contribution", Amount = buEr, Basis = $"Brutto (Cap {uvgCapMonthly:N2})", Rate = cfg.UvgBuEmployerRate, Side = "employer" });
            items.Add(new PayrollItemDto { Code = "BVG_ER", Title = "BVG (Arbeitgeber) (Fix)", Type = "contribution", Amount = bvgEr, Basis = "Fix", Rate = 0m, Side = "employer" });

            if (req.ApplyKTG)
            {
                var g = NormalizeGenderOrDefault(req.Gender);
                var r = (g == "F") ? cfg.KtgFEmployerRate : cfg.KtgMEmployerRate;

                items.Add(new PayrollItemDto
                {
                    Code = "KTG_ER",
                    Title = "KTG (Arbeitgeber)",
                    Type = "contribution",
                    Amount = ktgEr,
                    Basis = "Brutto",
                    Rate = r,
                    Side = "employer"
                });
            }

            items.Add(new PayrollItemDto { Code = "FAK", Title = "Familienausgleichskasse", Type = "contribution", Amount = fakEr, Basis = "Brutto", Rate = cfg.FakEmployerRate, Side = "employer" });

            if (vkEr > 0m)
            {
                items.Add(new PayrollItemDto
                {
                    Code = "VK",
                    Title = "Verwaltungskosten (VK)",
                    Type = "contribution",
                    Amount = vkEr,
                    Basis = "AHV total (AN+AG)",
                    Rate = cfg.AhvAdminCostRate,
                    Side = "employer"
                });
            }

            // =========================================================
            // Totals (Excel)
            // =========================================================
            var empDeductions = items
                .Where(i => i.Side == "employee" && i.Type == "deduction")
                .Sum(i => i.Amount);

            var netRaw =
                gross
                + req.ChildAllowance
                + req.PauschalExpenses
                + req.EffectiveExpenses
                - req.ShortTimeWorkDeduction
                - empDeductions;

            var netRounded = RoundStep(netRaw, cfg.FinalRoundingStep);

            var employerContrib = items.Where(i => i.Side == "employer").Sum(i => i.Amount);

            // ✅ Excel mantığı: Firma maliyeti = Brutto + Zulagen/Spesen - Kurzarbeit + AG Beiträge
            var employerTotalCostRaw =
                gross
                + req.ChildAllowance
                + req.PauschalExpenses
                + req.EffectiveExpenses
                - req.ShortTimeWorkDeduction
                + employerContrib;

            var employerTotalCost = RoundStep(employerTotalCostRaw, cfg.FinalRoundingStep);

            var alvEmpTotal = alv1Emp + alv2Emp;
            var alvErTotal = alv1Er + alv2Er;

            return new PayrollResponseDto
            {
                Period = $"{req.Period:yyyy-MM}",
                NetToPay = netRounded,

                // ✅ artık gerçek "total cost"
                EmployerTotalCost = employerTotalCost,

                Employee = new MoneyBreakdownDto
                {
                    AHV_IV_EO = ahvEmp,
                    ALV = alvEmpTotal,
                    UVG_NBU = nbuEmp,
                    UVG_BU = buEr,
                    BVG = bvgEmp,
                    KTG = ktgEmp,
                    WithholdingTax = qst,
                    Other = canteenDeduction
                },
                Employer = new MoneyBreakdownDto
                {
                    AHV_IV_EO = ahvEr,
                    ALV = alvErTotal,

                    // ✅ DTO’da UVG_BU alanın varsa bunu kullan
                    // UVG_BU = buEr,
                    // Eğer yoksa en azından şimdiki property’e BU yazdığını bil:
                    UVG_NBU = buEr,

                    BVG = bvgEr,
                    KTG = ktgEr,
                    WithholdingTax = 0m,
                    Other = fakEr + vkEr
                },
                Items = items
            };
        }
    }
}
