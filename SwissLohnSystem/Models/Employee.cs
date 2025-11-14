using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SwissLohnSystem.API.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [JsonIgnore]
        public Company? Company { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        public string? Email { get; set; }
        public string? Position { get; set; }

        public DateTime? BirthDate { get; set; }

        // DE UI kullanıyorsun ama API içinde istersen "single/married"e normalize edebilirsin
        public string? MaritalStatus { get; set; }

        public int ChildCount { get; set; }

        // Çalışma tipi & ücret
        [Required]
        public string SalaryType { get; set; } = "Monthly";   // "Monthly" | "Hourly"

        [Column(TypeName = "decimal(18,4)")]
        public decimal HourlyRate { get; set; }

        public int MonthlyHours { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BruttoSalary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int WorkedHours { get; set; }
        public int OvertimeHours { get; set; }
        public bool Active { get; set; } = true;

        // Sigorta & kimlik
        [MaxLength(20)]
        public string? AHVNumber { get; set; }

        public string? Krankenkasse { get; set; }
        public string? BVGPlan { get; set; }

        // Parametreler (vardı, dokunmadık)
        [Range(0, 100)]
        public decimal? PensumPercent { get; set; }      // %

        [Column(TypeName = "decimal(18,4)")]
        public decimal? HolidayRate { get; set; }        // örn 0.0833

        [Column(TypeName = "decimal(18,4)")]
        public decimal? OvertimeRate { get; set; }       // örn 1.25

        public string? WithholdingTaxCode { get; set; }

        // --- YENİ: Bordro varsayılanları / flag'ler ---

        public int WeeklyHours { get; set; } 

        public bool ApplyAHV { get; set; } = true;
        public bool ApplyALV { get; set; } = true;
        public bool ApplyNBU { get; set; } = true;
        public bool ApplyBU { get; set; } = true;
        public bool ApplyBVG { get; set; } = true;
        public bool ApplyFAK { get; set; } = true;
        public bool ApplyQST { get; set; } = false;

        public bool HolidayEligible { get; set; } = false;
        public bool ThirteenthEligible { get; set; } = false;
        public bool ThirteenthProrated { get; set; } = false;

        public string PermitType { get; set; } = "B";
        public string Canton { get; set; } = "ZH";
        public bool ChurchMember { get; set; } = false;


        // İletişim & adres
        public string? Address { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
    }
}
