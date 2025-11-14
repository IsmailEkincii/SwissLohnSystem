namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class BvgPlanDto
    {
        public decimal? EmployeeRateOverride { get; set; }
        public decimal? EmployerRateOverride { get; set; }
        public decimal? CoordinationDeductionAnnualOverride { get; set; }
    }
}
