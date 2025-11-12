using System;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Tek bir Lohn kaydının tam temsili (detay sayfası için uygun).
    /// API LohnDto ile birebir uyumludur.
    /// </summary>
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
        DateTime CreatedAt,
        bool IsFinal
    );
}
