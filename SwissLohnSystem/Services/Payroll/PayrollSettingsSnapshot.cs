namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class PayrollSettingsSnapshot
    {
        public decimal IntermediateRoundingStep { get; init; } = 0.01m;
        public decimal FinalRoundingStep { get; init; } = 0.05m;
        public decimal WithholdingRoundingStep { get; init; } = 0.05m;

        public decimal AhvEmployee { get; init; } = 0.053m;
        public decimal AhvEmployer { get; init; } = 0.053m;

        public decimal AlvRateTotal { get; init; } = 0.022m;
        public decimal AlvEmployeeShare { get; init; } = 0.5m;
        public decimal AlvEmployerShare { get; init; } = 0.5m;
        public decimal AlvAnnualCap { get; init; } = 148_200m;

        public decimal UvgBuEmployerRate { get; init; } = 0.0125m;
        public decimal UvgNbuEmployeeRate { get; init; } = 0.009m;
        public int UvgNbuMinWeeklyHours { get; init; } = 8;

        public decimal BvgEntryThresholdAnnual { get; init; } = 22_680m;
        public decimal BvgCoordinationDedAnnual { get; init; } = 26_460m;
        public decimal BvgUpperLimitAnnual { get; init; } = 90_720m;
        public decimal BvgEmployeeRate { get; init; } = 0.04m;
        public decimal BvgEmployerRate { get; init; } = 0.05m;

        public decimal FakEmployerRate { get; init; } = 0.012m;
    }
}
