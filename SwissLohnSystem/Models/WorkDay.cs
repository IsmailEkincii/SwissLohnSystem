using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class WorkDay
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        /// <summary>
        /// Çalışma / devamsızlık tarihi
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gün türü:
        /// "Work", "Vacation", "Sick", "Unpaid", "PublicHoliday", "OtherPaid" vb.
        /// Default: "Work"
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string DayType { get; set; } = "Work";

        /// <summary>
        /// Normal çalışma saatleri (sadece Work günlerinde > 0 olur)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal HoursWorked { get; set; }

        /// <summary>
        /// Überstunden saatleri
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal OvertimeHours { get; set; }
    }
}
