using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.WorkDay
{
    public record WorkDayUpdateDto(
       int Id,
       int EmployeeId,
       DateTime Date,
       string DayType,
       decimal HoursWorked,
       decimal OvertimeHours
   );
}
