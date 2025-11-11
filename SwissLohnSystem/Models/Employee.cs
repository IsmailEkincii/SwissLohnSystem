using SwissLohnSystem.API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Employee
{
    [Key] public int Id { get; set; }

    [ForeignKey("Company")] public int CompanyId { get; set; }

    [JsonIgnore] public Company? Company { get; set; }  // JSON'a navigation yok

    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }

    public string? Email { get; set; }
    public string? Position { get; set; }

    public DateTime? BirthDate { get; set; }
    public string? MaritalStatus { get; set; }   // "ledig" | "verheiratet"
    public int ChildCount { get; set; }

    [Required] public string SalaryType { get; set; }    // "Monthly" | "Hourly"
    public decimal HourlyRate { get; set; }
    public int MonthlyHours { get; set; }
    public decimal BruttoSalary { get; set; }

    [Required] public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int WorkedHours { get; set; }
    public int OvertimeHours { get; set; }
    public bool Active { get; set; } = true;

    // 🔽 Yeni eklenen alanlar (opsiyonel)
    // 2) Sigorta & kimlik
    [MaxLength(20)]
    public string? AHVNumber { get; set; }           // örn. 756.1234.5678.97 veya 7561234567897
    public string? Krankenkasse { get; set; }
    public string? BVGPlan { get; set; }

    // 3) Çalışma & maaş parametreleri
    [Range(0, 100)]
    public decimal? PensumPercent { get; set; }      // % (varsayılan 100)
    public decimal? HolidayRate { get; set; }        // % (varsayılan 8.33)
    public decimal? OvertimeRate { get; set; }       // örn. 1.25
    public string? WithholdingTaxCode { get; set; }  // Quellensteuer-Tarif

    // 4) İletişim & adres
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
}
