namespace SwissLohnSystem.API.Services.Payroll
{
    public sealed class EffectivePayrollSettings
    {
        public decimal AhvEmployee { get; set; }
        public decimal AhvEmployer { get; set; }

        public decimal AlvRateTotal { get; set; }
        public decimal AlvEmployeeShare { get; set; }
        public decimal AlvEmployerShare { get; set; }
        public decimal AlvAnnualCap { get; set; }

        public decimal Alv2RateTotal { get; set; } = 0m;
        public decimal Alv2EmployeeShare { get; set; } = 0.5m;
        public decimal Alv2EmployerShare { get; set; } = 0.5m;

        public decimal UvgCapAnnual { get; set; }
        public decimal UvgNbuMinWeeklyHours { get; set; }
        public decimal UvgNbuEmployeeRate { get; set; }
        public decimal UvgBuEmployerRate { get; set; }

        public decimal FakEmployerRate { get; set; }

        public decimal KtgMEmployeeRate { get; set; }
        public decimal KtgMEmployerRate { get; set; }
        public decimal KtgFEmployeeRate { get; set; }
        public decimal KtgFEmployerRate { get; set; }

        public decimal AhvAdminCostRate { get; set; }

        public decimal IntermediateRoundingStep { get; set; } = 0.01m;
        public decimal WithholdingRoundingStep { get; set; } = 0.05m;
        public decimal FinalRoundingStep { get; set; } = 0.01m;
    }
}
