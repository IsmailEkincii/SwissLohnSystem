using System;
using System.Collections.Generic;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    public class LohnDetailsDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }

        public bool IsFinal { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Company / Employee header fields
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }   // ✅ HATA BURADAYDI
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyCanton { get; set; }

        public string? EmployeeName { get; set; }
        public string? EmployeeAddress { get; set; }
        public string? EmployeeZip { get; set; }
        public string? EmployeeCity { get; set; }
        public string? EmployeeAhvNumber { get; set; }
        public string? EmployeePhone { get; set; }

        // Allowances / extras
        public decimal ChildAllowance { get; set; }
        public decimal HolidayAllowance { get; set; }
        public decimal OvertimePay { get; set; }

        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        // Flags (snapshot)
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        // QST meta
        public string? PermitType { get; set; }
        public string? Canton { get; set; }
        public bool ChurchMember { get; set; }
        public string? WithholdingTaxCode { get; set; }

        public decimal? HolidayRate { get; set; }
        public bool HolidayEligible { get; set; }

        public string? Comment { get; set; }

        // ✅ Employee deductions (AN)
        public decimal EmployeeAhvIvEo { get; set; }
        public decimal EmployeeAlv { get; set; }       // ✅ HATA BURADAYDI
        public decimal EmployeeNbu { get; set; }
        public decimal EmployeeBvg { get; set; }
        public decimal EmployeeQst { get; set; }

        // ✅ Employer contributions (AG)
        public decimal EmployerAhvIvEo { get; set; }
        public decimal EmployerAlv { get; set; }       // ✅ HATA BURADAYDI
        public decimal EmployerBu { get; set; }
        public decimal EmployerBvg { get; set; }
        public decimal EmployerFak { get; set; }

        // ✅ UI slip rows
        public List<LohnSlipItemDto> Items { get; set; } = new(); // ✅ Tip net: LohnSlipItemDto
    }
}
