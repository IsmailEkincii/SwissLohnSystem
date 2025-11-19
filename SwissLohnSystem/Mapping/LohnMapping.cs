using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class LohnMappings
    {
        public static LohnDto ToDto(this Lohn l)
            => new LohnDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                Month = l.Month,
                Year = l.Year,

                BruttoSalary = l.BruttoSalary,
                TotalDeductions = l.TotalDeductions,
                NetSalary = l.NetSalary,
                ChildAllowance = l.ChildAllowance,
                HolidayAllowance = l.HolidayAllowance,
                OvertimePay = l.OvertimePay,

                MonthlyHours = l.MonthlyHours,
                MonthlyOvertimeHours = l.MonthlyOvertimeHours,

                Bonus = l.Bonus,
                ExtraAllowance = l.ExtraAllowance,
                UnpaidDeduction = l.UnpaidDeduction,
                OtherDeduction = l.OtherDeduction,

                CreatedAt = l.CreatedAt,
                IsFinal = l.IsFinal,

                // Snapshot parametreler
                ApplyAHV = l.ApplyAHV,
                ApplyALV = l.ApplyALV,
                ApplyBVG = l.ApplyBVG,
                ApplyNBU = l.ApplyNBU,
                ApplyBU = l.ApplyBU,
                ApplyFAK = l.ApplyFAK,
                ApplyQST = l.ApplyQST,

                PermitType = l.PermitType,
                Canton = l.Canton,
                ChurchMember = l.ChurchMember,
                WithholdingTaxCode = l.WithholdingTaxCode,

                HolidayRate = l.HolidayRate,
                HolidayEligible = l.HolidayEligible,

                Comment = l.Comment
            };
    }
}
