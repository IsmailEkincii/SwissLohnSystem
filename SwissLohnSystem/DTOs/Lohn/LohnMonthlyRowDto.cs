namespace SwissLohnSystem.API.DTOs.Lohn;

public record LohnMonthlyRowDto(
    int EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal BruttoSalary,
    decimal NetSalary,
    decimal TotalDeductions,
    decimal OvertimePay,
    decimal HolidayAllowance
);
