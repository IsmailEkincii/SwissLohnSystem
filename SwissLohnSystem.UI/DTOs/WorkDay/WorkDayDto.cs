namespace SwissLohnSystem.UI.DTOs.WorkDay
{
public record WorkDayDto(
       int Id,
       int EmployeeId,
       DateTime Date,
       string DayType,
       decimal HoursWorked,
       decimal OvertimeHours
   );
}
