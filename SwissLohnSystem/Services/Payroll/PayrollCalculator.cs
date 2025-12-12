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

            // Yardımcı fonksiyonlar
            decimal NormalizeRate(decimal r) => r > 1m ? r / 100m : (r < 0m ? 0m : r);

            decimal RoundStep(decimal val, decimal step)
            {
                if (step == 0.01m)
                    return Math.Round(val, 2, MidpointRounding.AwayFromZero);

                var factor = 1m / step;
                return Math.Round(val * factor, 0, MidpointRounding.AwayFromZero) / factor;
            }

            var gross = req.GrossMonthly;
            var annualGross = gross * 12m;

            // -------- AHV/IV/EO --------
            decimal ahvEmp = 0m, ahvEr = 0m;
            if (req.ApplyAHV)
            {
                ahvEmp = RoundStep(gross * cfg.AhvEmployee, cfg.IntermediateRoundingStep);
                ahvEr = RoundStep(gross * cfg.AhvEmployer, cfg.IntermediateRoundingStep);
            }

            // -------- ALV --------
            decimal alvEmp = 0m, alvEr = 0m;
            if (req.ApplyALV)
            {
                var alvBase = Math.Min(gross, cfg.AlvAnnualCap / 12m);
                alvEmp = RoundStep(alvBase * cfg.AlvRateTotal * cfg.AlvEmployeeShare, cfg.IntermediateRoundingStep);
                alvEr = RoundStep(alvBase * cfg.AlvRateTotal * cfg.AlvEmployerShare, cfg.IntermediateRoundingStep);
            }

            // -------- UVG (NBU / BU) --------
            decimal nbuEmp = 0m;
            if (req.ApplyNBU && req.WeeklyHours >= cfg.UvgNbuMinWeeklyHours)
            {
                nbuEmp = RoundStep(gross * cfg.UvgNbuEmployeeRate, cfg.IntermediateRoundingStep);
            }

            decimal buEr = 0m;
            if (req.ApplyBU)
            {
                buEr = RoundStep(gross * cfg.UvgBuEmployerRate, cfg.IntermediateRoundingStep);
            }

            // -------- BVG --------
            decimal bvgEmp = 0m, bvgEr = 0m;
            decimal effectiveBvgEmpRate = 0m;
            decimal effectiveBvgErRate = 0m;

            if (req.ApplyBVG && annualGross >= cfg.BvgEntryThresholdAnnual)
            {
                var coordAnnual = Math.Max(
                    0m,
                    Math.Min(cfg.BvgUpperLimitAnnual, annualGross) -
                    (req.BvgPlan?.CoordinationDeductionAnnualOverride ?? cfg.BvgCoordinationDedAnnual)
                );

                var coordMonthly = coordAnnual / 12m;

                // Eğer override varsa onu, yoksa global setting'i kullan
                effectiveBvgEmpRate = NormalizeRate(req.BvgPlan?.EmployeeRateOverride ?? cfg.BvgEmployeeRate);
                effectiveBvgErRate = NormalizeRate(req.BvgPlan?.EmployerRateOverride ?? cfg.BvgEmployerRate);

                bvgEmp = RoundStep(coordMonthly * effectiveBvgEmpRate, cfg.IntermediateRoundingStep);
                bvgEr = RoundStep(coordMonthly * effectiveBvgErRate, cfg.IntermediateRoundingStep);
            }

            // -------- Quellensteuer (QST) --------
            decimal qst = 0m;
            decimal qstRate = 0m;

            // 🔔 QST için ayrı bir item hazırlayacağız (bulunsun veya bulunmasın)
            PayrollItemDto? qstItem = null;

            if (req.ApplyQST &&
                !string.IsNullOrWhiteSpace(req.Canton) &&
                !string.IsNullOrWhiteSpace(req.WithholdingTaxCode) &&
                !string.IsNullOrWhiteSpace(req.PermitType))
            {
                var tariff = _settings.GetQstTariff(
                    req.Canton!,
                    req.WithholdingTaxCode!,
                    req.PermitType!,
                    req.ChurchMember,
                    gross
                );

                if (tariff is not null)
                {
                    qstRate = tariff.Rate;
                    qst = RoundStep(gross * qstRate, cfg.WithholdingRoundingStep);

                    qstItem = new PayrollItemDto
                    {
                        Code = "QST",
                        Title = $"Quellensteuer {tariff.Code}",
                        Type = "deduction",
                        Amount = qst,
                        Basis = "Brutto",
                        Rate = qstRate,
                        Side = "employee"
                    };
                }
                else
                {
                    // ❗ QST uygulanmak isteniyor ama tarif bulunamadı – uyarı kalemi
                    qstItem = new PayrollItemDto
                    {
                        Code = "QST-MISS",
                        Title = "Quellensteuer-Tarif nicht gefunden",
                        Type = "info",
                        Amount = 0m,
                        Basis = "Brutto",
                        Rate = 0m,
                        Side = "employee"
                    };
                    // qst = 0 kalıyor, Employee.WithholdingTax da 0 olacak
                }
            }

            // -------- FAK (Familienausgleichskasse) --------
            decimal fakEr = 0m;
            if (req.ApplyFAK)
            {
                fakEr = RoundStep(gross * cfg.FakEmployerRate, cfg.IntermediateRoundingStep);
            }

            // -------- Kalem listesi --------
            var items = new List<PayrollItemDto>();

            // AN tarafı (çalışan)
            items.Add(new PayrollItemDto
            {
                Code = "AHV",
                Title = "AHV/IV/EO (Arbeitnehmer)",
                Type = "deduction",
                Amount = ahvEmp,
                Basis = "Brutto",
                Rate = cfg.AhvEmployee,
                Side = "employee"
            });

            items.Add(new PayrollItemDto
            {
                Code = "ALV",
                Title = "ALV (Arbeitnehmer)",
                Type = "deduction",
                Amount = alvEmp,
                Basis = "Brutto bis Cap",
                Rate = cfg.AlvRateTotal * cfg.AlvEmployeeShare,
                Side = "employee"
            });

            items.Add(new PayrollItemDto
            {
                Code = "NBU",
                Title = "Nichtberufsunfall (AN)",
                Type = "deduction",
                Amount = nbuEmp,
                Basis = "Brutto",
                Rate = cfg.UvgNbuEmployeeRate,
                Side = "employee"
            });

            items.Add(new PayrollItemDto
            {
                Code = "BVG",
                Title = "BVG (Arbeitnehmer)",
                Type = "deduction",
                Amount = bvgEmp,
                Basis = "Koord. Lohn",
                // override varsa onu, yoksa global oranı göster
                Rate = effectiveBvgEmpRate != 0m ? effectiveBvgEmpRate : cfg.BvgEmployeeRate,
                Side = "employee"
            });

            // 🔥 QST kalemi: bulunduysa gerçek deduction, bulunamadıysa info satırı
            if (qstItem is not null)
            {
                items.Add(qstItem);
            }

            // AG tarafı (işveren)
            items.Add(new PayrollItemDto
            {
                Code = "AHV_ER",
                Title = "AHV/IV/EO (Arbeitgeber)",
                Type = "contribution",
                Amount = ahvEr,
                Basis = "Brutto",
                Rate = cfg.AhvEmployer,
                Side = "employer"
            });

            items.Add(new PayrollItemDto
            {
                Code = "ALV_ER",
                Title = "ALV (Arbeitgeber)",
                Type = "contribution",
                Amount = alvEr,
                Basis = "Brutto bis Cap",
                Rate = cfg.AlvRateTotal * cfg.AlvEmployerShare,
                Side = "employer"
            });

            items.Add(new PayrollItemDto
            {
                Code = "BU",
                Title = "Berufsunfall (AG)",
                Type = "contribution",
                Amount = buEr,
                Basis = "Brutto",
                Rate = cfg.UvgBuEmployerRate,
                Side = "employer"
            });

            items.Add(new PayrollItemDto
            {
                Code = "BVG_ER",
                Title = "BVG (Arbeitgeber)",
                Type = "contribution",
                Amount = bvgEr,
                Basis = "Koord. Lohn",
                Rate = effectiveBvgErRate != 0m ? effectiveBvgErRate : cfg.BvgEmployerRate,
                Side = "employer"
            });

            items.Add(new PayrollItemDto
            {
                Code = "FAK",
                Title = "Familienausgleichskasse",
                Type = "contribution",
                Amount = fakEr,
                Basis = "Brutto",
                Rate = cfg.FakEmployerRate,
                Side = "employer"
            });

            // -------- Toplamlar --------
            var empDeductions = items
                .Where(i => i.Side == "employee" && i.Type == "deduction")
                .Sum(i => i.Amount);

            var netRaw = gross - empDeductions;
            var netRounded = RoundStep(netRaw, cfg.FinalRoundingStep);

            var employerTotal = items
                .Where(i => i.Side == "employer")
                .Sum(i => i.Amount);

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
                    WithholdingTax = qst,  // ❗ Burada hâlâ gerçek QST tutarı (yoksa 0)
                    Other = 0m
                },
                Employer = new MoneyBreakdownDto
                {
                    AHV_IV_EO = ahvEr,
                    ALV = alvEr,
                    UVG_NBU = buEr,           // 🔥 BU (Berufsunfall) artık buraya geliyor
                    BVG = bvgEr,
                    WithholdingTax = 0m,
                    Other = fakEr             // FAK = Other
                },
                Items = items
            };
        }
    }
}
