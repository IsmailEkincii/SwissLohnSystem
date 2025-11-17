using System;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Tek bir Lohn kaydının tam temsili (detay ve liste için).
    /// API LohnDto ile birebir uyumlu olmalı.
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
        decimal MonthlyHours,          // 🔥
        decimal MonthlyOvertimeHours,  // 🔥
        DateTime CreatedAt,
        bool IsFinal
    );
}
