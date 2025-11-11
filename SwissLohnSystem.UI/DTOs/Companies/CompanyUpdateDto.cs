namespace SwissLohnSystem.UI.DTOs.Companies;

public class CompanyUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Canton { get; set; } = null!;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxNumber { get; set; }
}
