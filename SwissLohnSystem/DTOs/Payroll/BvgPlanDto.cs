namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class BvgPlanDto
    {
        public decimal? EmployeeRateOverride { get; set; }
        public decimal? EmployerRateOverride { get; set; }
        public decimal? CoordinationDeductionAnnualOverride { get; set; }
        // ✅ Yeni: Settings prefix seçimi için
        public string? PlanCode { get; set; } // örn: "PK_ZURICH_STD_2026"

    }
}
