using System;
using System.Collections.Generic;
using SwissLohnSystem.API.DTOs.Payroll;

namespace SwissLohnSystem.API.DTOs.Lohn
{
    public class LohnDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }

        public decimal ChildAllowance { get; set; }
        public decimal HolidayAllowance { get; set; }
        public decimal OvertimePay { get; set; }

        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public decimal PrivateBenefitAmount { get; set; }
        public decimal ManualAdjustment { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }
        public DateTime? FinalizedAt { get; set; }

        // Excel fields
        public decimal PauschalExpenses { get; set; }
        public decimal EffectiveExpenses { get; set; }
        public decimal ShortTimeWorkDeduction { get; set; }

        public bool Include13thSalary { get; set; }
        public decimal ThirteenthSalaryAmount { get; set; }

        public int CanteenDays { get; set; }
        public decimal CanteenDailyRate { get; set; }
        public decimal CanteenDeduction { get; set; }

        // Snapshot flags
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }
        public bool ApplyKTG { get; set; }

        public string Gender { get; set; } = "M";
        public string? PermitType { get; set; }
        public string? Canton { get; set; }
        public bool ChurchMember { get; set; }
        public string? WithholdingTaxCode { get; set; }

        public string? Comment { get; set; }
        public string? BvgPlanCodeUsed { get; set; }

        // AN snapshot
        public decimal EmployeeAhvIvEo { get; set; }
        public decimal EmployeeAlv1 { get; set; }
        public decimal EmployeeAlv2 { get; set; }
        public decimal EmployeeNbu { get; set; }
        public decimal EmployeeBvg { get; set; }
        public decimal EmployeeKtg { get; set; }
        public decimal EmployeeQst { get; set; }

        // AG snapshot
        public decimal EmployerAhvIvEo { get; set; }
        public decimal EmployerAlv1 { get; set; }
        public decimal EmployerAlv2 { get; set; }
        public decimal EmployerBu { get; set; }
        public decimal EmployerBvg { get; set; }
        public decimal EmployerKtg { get; set; }
        public decimal EmployerFak { get; set; }

        // ✅ NEW
        public decimal EmployerVk { get; set; }

        public List<PayrollItemDto> Items { get; set; } = new();
    }
}
