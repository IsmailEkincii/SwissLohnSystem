using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Position { get; set; }

        public DateTime BirthDate { get; set; }
        public string MaritalStatus { get; set; } // Single / Married
        public int ChildCount { get; set; }

        [Required]
        public string SalaryType { get; set; } // Monthly / Hourly
        public decimal HourlyRate { get; set; }
        public int MonthlyHours { get; set; }
        public decimal BruttoSalary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int WorkedHours { get; set; }
        public int OvertimeHours { get; set; }
        public bool Active { get; set; } = true;
    }
}
