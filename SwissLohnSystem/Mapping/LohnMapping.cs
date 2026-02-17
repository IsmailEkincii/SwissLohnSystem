using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class LohnMapping
    {
        public static LohnDto ToDto(this Lohn l) => new LohnDto
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

            PrivateBenefitAmount = l.PrivateBenefitAmount,
            ManualAdjustment = l.ManualAdjustment,

            CreatedAt = l.CreatedAt,
            IsFinal = l.IsFinal,
            FinalizedAt = l.FinalizedAt,

            PauschalExpenses = l.PauschalExpenses,
            EffectiveExpenses = l.EffectiveExpenses,
            ShortTimeWorkDeduction = l.ShortTimeWorkDeduction,
            Include13thSalary = l.Include13thSalary,
            ThirteenthSalaryAmount = l.ThirteenthSalaryAmount,
            CanteenDays = l.CanteenDays,
            CanteenDailyRate = l.CanteenDailyRate,
            CanteenDeduction = l.CanteenDeduction,

            ApplyAHV = l.ApplyAHV,
            ApplyALV = l.ApplyALV,
            ApplyBVG = l.ApplyBVG,
            ApplyNBU = l.ApplyNBU,
            ApplyBU = l.ApplyBU,
            ApplyFAK = l.ApplyFAK,
            ApplyQST = l.ApplyQST,
            ApplyKTG = l.ApplyKTG,

            Gender = l.Gender,
            PermitType = l.PermitType,
            Canton = l.Canton,
            ChurchMember = l.ChurchMember,
            WithholdingTaxCode = l.WithholdingTaxCode,

            Comment = l.Comment,
            BvgPlanCodeUsed = l.BvgPlanCodeUsed,

            EmployeeAhvIvEo = l.EmployeeAhvIvEo,
            EmployeeAlv1 = l.EmployeeAlv1,
            EmployeeAlv2 = l.EmployeeAlv2,
            EmployeeNbu = l.EmployeeNbu,
            EmployeeBvg = l.EmployeeBvg,
            EmployeeKtg = l.EmployeeKtg,
            EmployeeQst = l.EmployeeQst,

            EmployerAhvIvEo = l.EmployerAhvIvEo,
            EmployerAlv1 = l.EmployerAlv1,
            EmployerAlv2 = l.EmployerAlv2,
            EmployerBu = l.EmployerBu,
            EmployerBvg = l.EmployerBvg,
            EmployerKtg = l.EmployerKtg,
            EmployerFak = l.EmployerFak,
            EmployerVk = l.EmployerVk
        };

        // ✅ PDF için: Entity -> DetailsDto
        public static LohnDetailsDto ToDetailsDto(this Lohn l) => new LohnDetailsDto
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

            PrivateBenefitAmount = l.PrivateBenefitAmount,
            ManualAdjustment = l.ManualAdjustment,

            CreatedAt = l.CreatedAt,
            IsFinal = l.IsFinal,
            FinalizedAt = l.FinalizedAt,

            // UI enrichments (bu endpointte boş kalabilir)
            EmployeeName = null,
            CompanyId = null,
            CompanyName = null,

            PauschalExpenses = l.PauschalExpenses,
            EffectiveExpenses = l.EffectiveExpenses,
            ShortTimeWorkDeduction = l.ShortTimeWorkDeduction,

            Include13thSalary = l.Include13thSalary,
            ThirteenthSalaryAmount = l.ThirteenthSalaryAmount,

            CanteenDays = l.CanteenDays,
            CanteenDailyRate = l.CanteenDailyRate,
            CanteenDeduction = l.CanteenDeduction,

            ApplyAHV = l.ApplyAHV,
            ApplyALV = l.ApplyALV,
            ApplyBVG = l.ApplyBVG,
            ApplyNBU = l.ApplyNBU,
            ApplyBU = l.ApplyBU,
            ApplyFAK = l.ApplyFAK,
            ApplyQST = l.ApplyQST,
            ApplyKTG = l.ApplyKTG,

            Gender = l.Gender,
            PermitType = l.PermitType,
            Canton = l.Canton,
            ChurchMember = l.ChurchMember,
            WithholdingTaxCode = l.WithholdingTaxCode,

            Comment = l.Comment,
            BvgPlanCodeUsed = l.BvgPlanCodeUsed,

            EmployeeAhvIvEo = l.EmployeeAhvIvEo,
            EmployeeAlv1 = l.EmployeeAlv1,
            EmployeeAlv2 = l.EmployeeAlv2,
            EmployeeNbu = l.EmployeeNbu,
            EmployeeBvg = l.EmployeeBvg,
            EmployeeKtg = l.EmployeeKtg,
            EmployeeQst = l.EmployeeQst,

            EmployerAhvIvEo = l.EmployerAhvIvEo,
            EmployerAlv1 = l.EmployerAlv1,
            EmployerAlv2 = l.EmployerAlv2,
            EmployerBu = l.EmployerBu,
            EmployerBvg = l.EmployerBvg,
            EmployerKtg = l.EmployerKtg,
            EmployerFak = l.EmployerFak,
            EmployerVk = l.EmployerVk,

            // Items: entity'de saklamıyorsan burada boş kalır.
            // PDF'in items'a ihtiyacı varsa, Lohn tablosunda Items yoksa
            // PDF'de snapshot alanlarıyla tabloyu kurmak gerekir.
            Items = new List<PayrollItemDto>()
        };
    }
}
