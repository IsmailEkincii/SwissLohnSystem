using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwissLohnSystem.API.Models
{
    [Index(nameof(EmployeeId), nameof(Month), nameof(Year), IsUnique = true)]
    public class Lohn
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        [Range(1, 12)]
        public int Month { get; set; }

        [Range(2000, 2100)]
        public int Year { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal ChildAllowance { get; set; }
        public decimal HolidayAllowance { get; set; }
        public decimal OvertimePay { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Entwurf (false) / Final (true). Final kayıt değiştirilemez/silinemez.</summary>
        public bool IsFinal { get; set; } = false;
    }
}
