namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class PayrollRequestDto
    {
        public int EmployeeId { get; set; }

        // 🔥 API tarafında kesinlikle DateOnly
        public DateOnly Period { get; set; }

        public decimal GrossMonthly { get; set; }
        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        // Sosyal sigorta bayrakları
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        public int WeeklyHours { get; set; }

        // QST / Steuer
        public string Canton { get; set; } = "ZH";
        public string PermitType { get; set; } = "B";
        public bool ChurchMember { get; set; }
        public string? WithholdingTaxCode { get; set; }

        // BVG override
        public BvgPlanDto? BvgPlan { get; set; }

        public decimal? WorkedDays { get; set; }
        public decimal? SickDays { get; set; }
        public decimal? UnpaidDays { get; set; }
    }
}
