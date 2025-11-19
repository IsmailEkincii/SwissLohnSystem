using System;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Tek bir Lohn kaydının tam temsili (detay ve liste için).
    /// API LohnDto ile birebir uyumlu olmalı.
    /// </summary>
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

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }
    }
}
