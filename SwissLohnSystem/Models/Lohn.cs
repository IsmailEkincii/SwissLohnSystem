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

        /// <summary>Gehaltsmonat (1-12)</summary>
        public int Month { get; set; }

        /// <summary>Gehaltsjahr (z.B. 2025)</summary>
        public int Year { get; set; }

        // --- Temel rakamlar ---
        [Column(TypeName = "decimal(18,4)")]
        public decimal BruttoSalary { get; set; }

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

        // --- Ek kalemler (her ay için) ---
        [Column(TypeName = "decimal(18,4)")]
        public decimal Bonus { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExtraAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnpaidDeduction { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OtherDeduction { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>Wenn true, Lohn ist final und kann nicht mehr überschrieben werden.</summary>
        public bool IsFinal { get; set; }

        // ===============================
        //  SNAPSHOT PARAMETER PRO MONAT
        // ===============================

        // --- Sozialversicherungs-Flags ---
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        // --- Steuer / Bewilligung ---
        [MaxLength(20)]
        public string? PermitType { get; set; }   // z.B. B, C, L, G, F, N

        [MaxLength(5)]
        public string? Canton { get; set; }       // z.B. ZH, AG, LU

        public bool ChurchMember { get; set; }

        [MaxLength(50)]
        public string? WithholdingTaxCode { get; set; }

        // --- Ferien-Parameter ---
        [Column(TypeName = "decimal(18,4)")]
        public decimal? HolidayRate { get; set; } // z.B. 0.0833, 0.1027

        public bool HolidayEligible { get; set; }

        // --- Kommentar ---
        public string? Comment { get; set; }
    }
}
