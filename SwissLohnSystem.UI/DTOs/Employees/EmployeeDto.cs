namespace SwissLohnSystem.UI.DTOs.Employees;

public record EmployeeDto(
    int Id,
    int CompanyId,
    string FirstName,
    string LastName,
    string? Email,
    string? Position,
    DateTime? BirthDate,
    string? MaritalStatus,
    int ChildCount,
    string SalaryType,      // "Monthly" | "Hourly"
    decimal HourlyRate,
    int MonthlyHours,
    decimal BruttoSalary,
    DateTime StartDate,
    DateTime? EndDate,
    bool Active,
    // Sigorta & kimlik
    string? AHVNumber,
    string? Krankenkasse,
    string? BVGPlan,
    // Parametreler
    decimal? PensumPercent,
    decimal? HolidayRate,
    decimal? OvertimeRate,
    string? WithholdingTaxCode,
    // Adres & iletişim
    string? Address,
    string? Zip,
    string? City,
    string? Phone
);
