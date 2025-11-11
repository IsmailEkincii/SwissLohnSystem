using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Companies;

public class CompanyDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kanton ist erforderlich.")]
    public string Canton { get; set; } = string.Empty;

    public string? Address { get; set; }

    [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail eingeben.")]
    public string? Email { get; set; }

    public string? Phone { get; set; }
    public string? TaxNumber { get; set; }
}
