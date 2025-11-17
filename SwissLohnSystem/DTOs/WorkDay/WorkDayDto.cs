using System;

namespace SwissLohnSystem.API.DTOs.WorkDay
{
    public record WorkDayDto(
        int Id,
        int EmployeeId,
        DateTime Date,
        decimal HoursWorked,
        decimal OvertimeHours
    );
}
