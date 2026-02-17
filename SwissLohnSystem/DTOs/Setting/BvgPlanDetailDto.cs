namespace SwissLohnSystem.API.DTOs.Setting
{
    public sealed class BvgPlanDetailDto
    {
        public int CompanyId { get; set; } // ✅ EKLE
        public string PlanCode { get; set; } = null!;
        public string PlanBaseCode { get; set; } = null!;
        public int Year { get; set; }

        public decimal CoordinationDedAnnual { get; set; }
        public decimal EntryThresholdAnnual { get; set; }
        public decimal UpperLimitAnnual { get; set; }

        public decimal Rate25_34_Employee { get; set; }
        public decimal Rate25_34_Employer { get; set; }

        public decimal Rate35_44_Employee { get; set; }
        public decimal Rate35_44_Employer { get; set; }

        public decimal Rate45_54_Employee { get; set; }
        public decimal Rate45_54_Employer { get; set; }

        public decimal Rate55_65_Employee { get; set; }
        public decimal Rate55_65_Employer { get; set; }
    }
}
