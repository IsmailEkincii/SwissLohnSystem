namespace SwissLohnSystem.API.DTOs.Companies
{
    public record CompanyDto(
        int Id,
        string Name,
        string? Address,
        string Canton,
        string? Email,
        string? Phone,
        string? TaxNumber,
        string? DefaultBvgPlanCode

    );
}
