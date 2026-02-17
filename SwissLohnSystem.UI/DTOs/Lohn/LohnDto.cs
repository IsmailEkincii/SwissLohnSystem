using System;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    public class LohnDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }

        public bool IsFinal { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Allowances / extras (excel snapshot)
        public decimal ChildAllowance { get; set; }
        public decimal HolidayAllowance { get; set; }
        public decimal OvertimePay { get; set; }

        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        // Flags
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

        // Employee configuration snapshot bits
        public decimal? HolidayRate { get; set; }
        public bool HolidayEligible { get; set; }

        public string? Comment { get; set; }

        // ✅ Snapshot totals from API payroll calc / LohnService
        public decimal EmployeeAhvIvEo { get; set; }
        public decimal EmployeeAlv { get; set; }    // ✅ HATA BURADAYDI
        public decimal EmployeeNbu { get; set; }
        public decimal EmployeeBvg { get; set; }
        public decimal EmployeeQst { get; set; }

        public decimal EmployerAhvIvEo { get; set; }
        public decimal EmployerAlv { get; set; }    // ✅ HATA BURADAYDI
        public decimal EmployerBu { get; set; }
        public decimal EmployerBvg { get; set; }
        public decimal EmployerFak { get; set; }
    }
}
