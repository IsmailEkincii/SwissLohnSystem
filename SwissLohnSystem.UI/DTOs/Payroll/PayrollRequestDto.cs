namespace SwissLohnSystem.UI.DTOs.Payroll
{
    public sealed class PayrollRequestDto
    {
        public Guid CompanyId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Canton { get; set; } = "ZH";
        public DateOnly Period { get; set; }
        public decimal GrossMonthly { get; set; }
        public bool ThirteenthSalaryProrated { get; set; }
        public int Age { get; set; }
        public int WeeklyHours { get; set; }
        public string PermitType { get; set; } = "B";
        public string MaritalStatus { get; set; } = "single";
        public int Children { get; set; }
        public bool ChurchMember { get; set; }
        public BvgPlanDto? BvgPlan { get; set; }
    }

    public sealed class BvgPlanDto
    {
        public decimal? EmployeeRateOverride { get; set; }
        public decimal? EmployerRateOverride { get; set; }
        public decimal? CoordinationDeductionAnnualOverride { get; set; }
    }
}
