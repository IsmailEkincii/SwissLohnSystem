using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class LohnMappings
    {
        public static LohnDto ToDto(this Lohn l)
            => new LohnDto(
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
                l.MonthlyHours,          // 🔥 aylık toplam saat
                l.MonthlyOvertimeHours,  // 🔥 aylık toplam mesai
                l.CreatedAt,
                l.IsFinal
            );
    }
}
