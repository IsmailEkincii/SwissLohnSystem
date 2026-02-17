namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class PayrollSettingsSnapshot
    {
        // Rounding
        public decimal IntermediateRoundingStep { get; init; } = 0.01m;
        public decimal FinalRoundingStep { get; init; } = 0.05m;
        public decimal WithholdingRoundingStep { get; init; } = 0.05m;

        // AHV
        public decimal AhvEmployee { get; init; } = 0.053m;
        public decimal AhvEmployer { get; init; } = 0.053m;

        // ✅ VK (Verwaltungskosten) - Excel’de AHV toplamı üzerinden % (örn 1%)
        public decimal AhvAdminCostRate { get; init; } = 0.01m;

        // ALV1 (standard)
        public decimal AlvRateTotal { get; init; } = 0.022m;
        public decimal AlvEmployeeShare { get; init; } = 0.5m;
        public decimal AlvEmployerShare { get; init; } = 0.5m;
        public decimal AlvAnnualCap { get; init; } = 148_200m;

        // ✅ ALV2 (solidarity / cap üstü) - default 0 => kapalı
        public decimal Alv2RateTotal { get; init; } = 0m;
        public decimal Alv2EmployeeShare { get; init; } = 0.5m;
        public decimal Alv2EmployerShare { get; init; } = 0.5m;

        // UVG
        public decimal UvgBuEmployerRate { get; init; } = 0.0125m;
        public decimal UvgNbuEmployeeRate { get; init; } = 0.009m;
        public int UvgNbuMinWeeklyHours { get; init; } = 8;

        // ✅ UVG Cap (annual) - Excel: 148’200 / yıl
        public decimal UvgCapAnnual { get; init; } = 148_200m;

        // BVG (plan-aware değerler)
        public decimal BvgEntryThresholdAnnual { get; init; } = 22_680m;
        public decimal BvgCoordinationDedAnnual { get; init; } = 26_460m;
        public decimal BvgUpperLimitAnnual { get; init; } = 90_720m;
        public decimal BvgEmployeeRate { get; init; } = 0.04m;
        public decimal BvgEmployerRate { get; init; } = 0.05m;
        public decimal BvgMinInsuredAnnual { get; init; } = 0m;
        public bool BvgCoordinationProRata { get; init; } = true;

        public decimal BvgEmpRate25_34 { get; init; }
        public decimal BvgEmpRate35_44 { get; init; }
        public decimal BvgEmpRate45_54 { get; init; }
        public decimal BvgEmpRate55_65 { get; init; }

        public decimal BvgErRate25_34 { get; init; }
        public decimal BvgErRate35_44 { get; init; }
        public decimal BvgErRate45_54 { get; init; }
        public decimal BvgErRate55_65 { get; init; }

        // FAK
        public decimal FakEmployerRate { get; init; } = 0.012m;

        // ✅ KTG (gender-based)
        public decimal KtgMEmployeeRate { get; init; } = 0m;
        public decimal KtgMEmployerRate { get; init; } = 0m;
        public decimal KtgFEmployeeRate { get; init; } = 0m;
        public decimal KtgFEmployerRate { get; init; } = 0m;
    }
}
