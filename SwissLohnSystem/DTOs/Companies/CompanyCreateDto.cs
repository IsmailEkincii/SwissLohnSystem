using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Companies;

public class CompanyCreateDto
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Canton { get; set; } = null!;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxNumber { get; set; }
}