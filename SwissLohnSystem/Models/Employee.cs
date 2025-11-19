using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // ---- Basisdaten ----
        [Required, MaxLength(150)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(150)]
        public string LastName { get; set; } = null!;

        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? Position { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? MaritalStatus { get; set; }

        public int ChildCount { get; set; }

        // ---- Gehalt ----
        // "Monthly" | "Hourly"
        [Required, MaxLength(20)]
        public string SalaryType { get; set; } = "Monthly";

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public int MonthlyHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BruttoSalary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool Active { get; set; } = true;

        // ---- Arbeitszeit / Lohnparameter ----
        public int WeeklyHours { get; set; } = 42;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PensumPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HolidayRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OvertimeRate { get; set; }

        public bool HolidayEligible { get; set; }
        public bool ThirteenthEligible { get; set; }
        public bool ThirteenthProrated { get; set; }

        // ---- Sozialversicherungs-Flags ----
        public bool ApplyAHV { get; set; } = true;
        public bool ApplyALV { get; set; } = true;
        public bool ApplyNBU { get; set; } = true;
        public bool ApplyBU { get; set; } = true;
        public bool ApplyBVG { get; set; } = true;
        public bool ApplyFAK { get; set; } = true;
        public bool ApplyQST { get; set; }

        // ---- Steuer / Kanton ----
        [MaxLength(5)]
        public string PermitType { get; set; } = "B";

        public bool ChurchMember { get; set; }

        [MaxLength(2)]
        public string Canton { get; set; } = "ZH";

        [MaxLength(10)]
        public string? WithholdingTaxCode { get; set; }

        // ---- Sozialversicherung ----
        [MaxLength(50)]
        public string? AHVNumber { get; set; }

        [MaxLength(100)]
        public string? Krankenkasse { get; set; }

        [MaxLength(100)]
        public string? BVGPlan { get; set; }

        // ---- Adresse & Kontakt ----
        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Zip { get; set; }

        [MaxLength(150)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }
    }
}
