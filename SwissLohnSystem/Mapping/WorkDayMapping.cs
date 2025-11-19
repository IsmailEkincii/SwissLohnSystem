using SwissLohnSystem.API.DTOs.WorkDay;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class WorkDayMapping
    {
        public static WorkDayDto ToDto(this WorkDay w) =>
            new WorkDayDto(
                w.Id,
                w.EmployeeId,
                w.Date,
                w.DayType,
                w.HoursWorked,
                w.OvertimeHours
            );

        public static WorkDay ToEntity(this WorkDayCreateDto dto) => new WorkDay
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            DayType = string.IsNullOrWhiteSpace(dto.DayType) ? "Work" : dto.DayType.Trim(),
            HoursWorked = dto.HoursWorked,
            OvertimeHours = dto.OvertimeHours
        };

        public static void Apply(this WorkDay entity, WorkDayUpdateDto dto)
        {
            entity.EmployeeId = dto.EmployeeId;
            entity.Date = dto.Date;
            entity.DayType = string.IsNullOrWhiteSpace(dto.DayType) ? entity.DayType : dto.DayType.Trim();
            entity.HoursWorked = dto.HoursWorked;
            entity.OvertimeHours = dto.OvertimeHours;
        }
    }
}
