using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.WorkDay
{
    public record WorkDayCreateDto(
        int EmployeeId,
        DateTime Date,
        string DayType,
        decimal HoursWorked,
        decimal OvertimeHours
    );
}
