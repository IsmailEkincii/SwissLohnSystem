using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.WorkDay
{
    public record WorkDayCreateDto(
         int EmployeeId,
        DateTime Date,
        string DayType,
        decimal HoursWorked,
        decimal OvertimeHours
    );
}
