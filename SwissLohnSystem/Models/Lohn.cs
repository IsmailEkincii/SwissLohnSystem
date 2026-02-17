using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class Lohn
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Month { get; set; }
        public int Year { get; set; }

        // --- Temel rakamlar ---
        [Column(TypeName = "decimal(18,4)")]
        public decimal BruttoSalary { get; set; } // ✅ 13th dahil brüt

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal NetSalary { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ChildAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal HolidayAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OvertimePay { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MonthlyHours { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MonthlyOvertimeHours { get; set; }

        // --- Ek kalemler ---
        [Column(TypeName = "decimal(18,4)")]
        public decimal Bonus { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExtraAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnpaidDeduction { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OtherDeduction { get; set; }

        // ✅ MA1 benzeri
        [Column(TypeName = "decimal(18,4)")]
        public decimal PrivateBenefitAmount { get; set; } // brüte ek

        [Column(TypeName = "decimal(18,4)")]
        public decimal ManualAdjustment { get; set; } // +/- brüt düzeltme

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }
        public DateTime? FinalizedAt { get; set; }

        // ===============================
        // Excel snapshot
        // ===============================

        [Column(TypeName = "decimal(18,4)")]
        public decimal PauschalExpenses { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EffectiveExpenses { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ShortTimeWorkDeduction { get; set; }

        public bool Include13thSalary { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ThirteenthSalaryAmount { get; set; }

        public int CanteenDays { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CanteenDailyRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CanteenDeduction { get; set; }

        // Flags snapshot
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }
        public bool ApplyKTG { get; set; }

        [MaxLength(1)]
        public string Gender { get; set; } = "M";

        [MaxLength(20)]
        public string? PermitType { get; set; }

        [MaxLength(5)]
        public string? Canton { get; set; }

        public bool ChurchMember { get; set; }

        [MaxLength(50)]
        public string? WithholdingTaxCode { get; set; }

        public string? Comment { get; set; }

        // ===============================
        // Snapshot: Employee deductions (AN)
        // ===============================
        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeAhvIvEo { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeAlv1 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeAlv2 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeNbu { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeBvg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeKtg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployeeQst { get; set; }

        // ===============================
        // Snapshot: Employer contributions (AG)
        // ===============================
        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerAhvIvEo { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerAlv1 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerAlv2 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerBu { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerBvg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerKtg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerFak { get; set; }

        // ✅ NEW: VK
        [Column(TypeName = "decimal(18,4)")]
        public decimal EmployerVk { get; set; }

        [MaxLength(100)]
        public string? BvgPlanCodeUsed { get; set; }
    }
}
