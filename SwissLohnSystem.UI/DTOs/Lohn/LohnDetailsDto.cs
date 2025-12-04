using System;
using System.Collections.Generic;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Lohn detay modal/sayfası için genişletilmiş DTO.
    /// </summary>
    public class LohnDetailsDto
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

        // Ek brüt kalemleri
        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        // Aylık çalışma saatleri
        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }

        // UI enrichments
        public string? EmployeeName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        // İleride kalem dökümü için (şimdilik opsiyonel)
        public List<LohnItemDto> Items { get; set; } = new();

        // --- Snapshot parametreler (Lohn tablosundan) ---
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        public string? PermitType { get; set; }
        public string? Canton { get; set; }
        public bool ChurchMember { get; set; }
        public string? WithholdingTaxCode { get; set; }

        public decimal? HolidayRate { get; set; }
        public bool HolidayEligible { get; set; }

        public string? Comment { get; set; }

        // --- Arbeitnehmer-Abzüge Snapshot (AHV/ALV/NBU/BVG/QST) ---
        public decimal EmployeeAhvIvEo { get; set; }
        public decimal EmployeeAlv { get; set; }
        public decimal EmployeeNbu { get; set; }
        public decimal EmployeeBvg { get; set; }
        public decimal EmployeeQst { get; set; }
    }

    public class LohnItemDto
    {
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public string Type { get; set; } = "info";
        public string Basis { get; set; } = "";
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Side { get; set; } = "employee";
    }
}
