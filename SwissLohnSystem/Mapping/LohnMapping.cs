using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings;

public static class LohnMapping
{
    public static LohnDto ToDto(this Lohn l) =>
        new(
            l.Id,
            l.EmployeeId,
            l.Month,
            l.Year,
            l.BruttoSalary,
            l.TotalDeductions,
            l.NetSalary,
            l.ChildAllowance,
            l.HolidayAllowance,
            l.OvertimePay,
            l.CreatedAt
        );
}
