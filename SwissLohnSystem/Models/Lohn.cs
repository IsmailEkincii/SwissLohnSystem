using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class Lohn
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BruttoSalary { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal NetSalary { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ChildAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal HolidayAllowance { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OvertimePay { get; set; }

        // 🔥 yeni alanlar
        [Column(TypeName = "decimal(18,4)")]
        public decimal MonthlyHours { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MonthlyOvertimeHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }
    }
}
