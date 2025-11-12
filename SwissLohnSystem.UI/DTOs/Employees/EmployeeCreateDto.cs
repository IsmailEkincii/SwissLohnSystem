using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Employees
{
    public class EmployeeCreateDto
    {
        [Required] public int CompanyId { get; set; }

        [Required, MaxLength(100)] public string FirstName { get; set; } = null!;
        [Required, MaxLength(100)] public string LastName { get; set; } = null!;

        [EmailAddress, MaxLength(200)] public string? Email { get; set; }
        [MaxLength(100)] public string? Position { get; set; }

        public DateTime? BirthDate { get; set; }
        [MaxLength(20)] public string? MaritalStatus { get; set; }   // "ledig" | "verheiratet"
        [Range(0, 20)] public int ChildCount { get; set; }

        [Required, RegularExpression("^(Monthly|Hourly)$", ErrorMessage = "SalaryType muss 'Monthly' oder 'Hourly' sein.")]
        public string SalaryType { get; set; } = null!;

        [Range(0, 1_000_000)] public decimal HourlyRate { get; set; }
        [Range(0, 400)] public int MonthlyHours { get; set; }
        [Range(0, 1_000_000)] public decimal BruttoSalary { get; set; }

        [Required] public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool Active { get; set; } = true;

        // Sigorta & kimlik
        [MaxLength(20)] public string? AHVNumber { get; set; }
        [MaxLength(100)] public string? Krankenkasse { get; set; }
        [MaxLength(100)] public string? BVGPlan { get; set; }

        // Parametreler
        [Range(0, 100)] public decimal? PensumPercent { get; set; }   // vars. 100
        [Range(0, 100)] public decimal? HolidayRate { get; set; }     // vars. 8.33
        [Range(0, 10)] public decimal? OvertimeRate { get; set; }    // vars. 1.25
        [MaxLength(50)] public string? WithholdingTaxCode { get; set; }

        // Adres & iletişim
        [MaxLength(200)] public string? Address { get; set; }
        [MaxLength(20)] public string? Zip { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
    }
}
