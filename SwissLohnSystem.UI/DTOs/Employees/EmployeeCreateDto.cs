using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Employees;

public class EmployeeCreateDto
{
    [Required] public int CompanyId { get; set; }
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;

    public string? Email { get; set; }
    public string? Position { get; set; }

    public DateTime? BirthDate { get; set; }
    public string? MaritalStatus { get; set; }   // "ledig" | "verheiratet" (opsiyonel)
    public int ChildCount { get; set; }

    [Required] public string SalaryType { get; set; } = null!;   // "Monthly" | "Hourly"
    public decimal HourlyRate { get; set; }
    public int MonthlyHours { get; set; }
    public decimal BruttoSalary { get; set; }

    [Required] public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool Active { get; set; } = true;

    // Sigorta & kimlik
    public string? AHVNumber { get; set; }
    public string? Krankenkasse { get; set; }
    public string? BVGPlan { get; set; }

    // Parametreler
    public decimal? PensumPercent { get; set; }   // vars. 100
    public decimal? HolidayRate { get; set; }     // vars. 8.33
    public decimal? OvertimeRate { get; set; }    // vars. 1.25
    public string? WithholdingTaxCode { get; set; }

    // Adres & iletişim
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
}
