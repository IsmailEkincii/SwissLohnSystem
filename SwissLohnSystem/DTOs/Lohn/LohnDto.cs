namespace SwissLohnSystem.API.DTOs.Lohn;

public record LohnDto(
    int Id,
    int EmployeeId,
    int Month,
    int Year,
    decimal BruttoSalary,
    decimal TotalDeductions,
    decimal NetSalary,
    decimal ChildAllowance,
    decimal HolidayAllowance,
    decimal OvertimePay,
    DateTime CreatedAt
);
