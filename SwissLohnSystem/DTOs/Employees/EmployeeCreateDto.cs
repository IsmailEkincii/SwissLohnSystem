using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Employees
{
    public sealed record EmployeeCreateDto
    {
        [Required]
        public int CompanyId { get; init; }

        [Required, StringLength(100)]
        public string FirstName { get; init; } = null!;

        [Required, StringLength(100)]
        public string LastName { get; init; } = null!;

        [EmailAddress]
        public string? Email { get; init; }

        [StringLength(100)]
        public string? Position { get; init; }

        public DateTime? BirthDate { get; init; }
        public string? MaritalStatus { get; init; }
        public int ChildCount { get; init; }

        [Required]
        [RegularExpression("Monthly|Hourly", ErrorMessage = "SalaryType muss 'Monthly' oder 'Hourly' sein.")]
        public string SalaryType { get; init; } = "Monthly";

        // SalaryType == "Hourly" ise kullanılır
        [Range(0, 1_000_000)]
        public decimal HourlyRate { get; init; }

        public int MonthlyHours { get; init; }

        // SalaryType == "Monthly" ise kullanılır
        [Range(0, 1_000_000)]
        public decimal BruttoSalary { get; init; }

        [Required]
        public DateTime StartDate { get; init; }

        public DateTime? EndDate { get; init; }

        public bool Active { get; init; } = true;

        // Sigorta & kimlik
        public string? AHVNumber { get; init; }
        public string? Krankenkasse { get; init; }
        public string? BVGPlan { get; init; }

        // Parametreler
        public decimal? PensumPercent { get; init; }
        public decimal? HolidayRate { get; init; }
        public decimal? OvertimeRate { get; init; }
        public string? WithholdingTaxCode { get; init; }

        public int WeeklyHours { get; init; } = 42;   // istersen değiştir

        public bool ApplyAHV { get; init; } = true;
        public bool ApplyALV { get; init; } = true;
        public bool ApplyNBU { get; init; } = true;
        public bool ApplyBU { get; init; } = true;
        public bool ApplyBVG { get; init; } = true;
        public bool ApplyFAK { get; init; } = true;
        public bool ApplyQST { get; init; }

        public bool HolidayEligible { get; init; } = true;
        public bool ThirteenthEligible { get; init; } = true;
        public bool ThirteenthProrated { get; init; } = true;

        public string PermitType { get; init; } = "B";
        public bool ChurchMember { get; init; }
        public string Canton { get; init; } = "ZH";

        // Adres & iletişim
        public string? Address { get; init; }
        public string? Zip { get; init; }
        public string? City { get; init; }
        public string? Phone { get; init; }
    }
}
