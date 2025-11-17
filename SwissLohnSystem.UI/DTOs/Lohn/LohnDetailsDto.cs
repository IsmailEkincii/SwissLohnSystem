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

        // 🔥 Aylık çalışma saatleri
        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }

        // UI enrichments
        public string? EmployeeName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        // İleride kalem dökümü için (şimdilik boş)
        public List<LohnItemDto> Items { get; set; } = new();
    }

    public class LohnItemDto
    {
        public string Code { get; set; } = "";         // AHV, ALV, NBU, BU, BVG, QST, FAK, etc.
        public string Title { get; set; } = "";        // DE başlık
        public string Type { get; set; } = "info";     // deduction | contribution | info
        public string Basis { get; set; } = "";        // Örn: Brutto, Koord. Lohn
        public decimal Rate { get; set; }              // 0.053 vb.
        public decimal Amount { get; set; }            // +/- tutar
        public string Side { get; set; } = "employee"; // employee | employer
    }
}
