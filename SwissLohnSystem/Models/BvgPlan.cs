using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class BvgPlan
    {
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required, MaxLength(80)]
        public string PlanCode { get; set; } = null!; // PK_ZURICH_STD_2026

        [Required, MaxLength(60)]
        public string PlanBaseCode { get; set; } = null!; // PK_ZURICH_STD

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

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
