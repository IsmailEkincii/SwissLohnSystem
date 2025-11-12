namespace SwissLohnSystem.API.DTOs.Payroll
{
    public sealed class PayrollRequestDto
    {
        public Guid CompanyId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Canton { get; set; } = "ZH"; // örn. Zürich
        public DateOnly Period { get; set; }       // YYYY-MM ayı
        public decimal GrossMonthly { get; set; }  // Brüt maaş
        public bool ThirteenthSalaryProrated { get; set; } = false;
        public int Age { get; set; }
        public int WeeklyHours { get; set; }
        public string PermitType { get; set; } = "B";
        public string MaritalStatus { get; set; } = "single";
        public int Children { get; set; }
        public bool ChurchMember { get; set; } = false;
        public BvgPlanDto? BvgPlan { get; set; }
    }

    public sealed class BvgPlanDto
    {
        public decimal? EmployeeRateOverride { get; set; }
        public decimal? EmployerRateOverride { get; set; }
        public decimal? CoordinationDeductionAnnualOverride { get; set; }
    }
}
