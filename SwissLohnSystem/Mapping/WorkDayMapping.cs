using SwissLohnSystem.API.DTOs.WorkDay;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings;

public static class WorkDayMapping
{
    public static WorkDayDto ToDto(this WorkDay w) =>
        new(w.Id, w.EmployeeId, w.Date, w.HoursWorked, w.OvertimeHours);

    public static WorkDay ToEntity(this WorkDayCreateDto dto) => new()
    {
        EmployeeId = dto.EmployeeId,
        Date = dto.Date,
        HoursWorked = dto.HoursWorked,
        OvertimeHours = dto.OvertimeHours
    };

    public static void Apply(this WorkDay entity, WorkDayUpdateDto dto)
    {
        entity.EmployeeId = dto.EmployeeId;
        entity.Date = dto.Date;
        entity.HoursWorked = dto.HoursWorked;
        entity.OvertimeHours = dto.OvertimeHours;
    }
}
