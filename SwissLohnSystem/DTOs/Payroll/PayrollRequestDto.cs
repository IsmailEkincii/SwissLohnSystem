using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class PayrollRequestDto
    {
        // 🔥 UI sadece bunları gönderiyor → bunlar Required
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime Period { get; set; }

        // 🔥 Geri kalan tüm alanlar optional
        public string? Canton { get; set; }

        public decimal GrossMonthly { get; set; }

        public bool ApplyAHV { get; set; } = true;
        public bool ApplyALV { get; set; } = true;
        public bool ApplyBVG { get; set; } = true;
        public bool ApplyNBU { get; set; } = true;
        public bool ApplyBU { get; set; } = true;
        public bool ApplyFAK { get; set; } = true;
        public bool ApplyQST { get; set; } = false;

        public int WeeklyHours { get; set; }

        public string? PermitType { get; set; }
        public string? WithholdingTaxCode { get; set; }
        public bool ChurchMember { get; set; }

        public BvgPlanDto? BvgPlan { get; set; }
    }
}
