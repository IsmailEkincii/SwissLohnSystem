using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Employees
{
    public class EmployeeUpdateDto
    {
        [Required] public int Id { get; set; }
        [Required] public int CompanyId { get; set; }

        [Required, StringLength(150)]
        public string FirstName { get; set; } = null!;

        [Required, StringLength(150)]
        public string LastName { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        public string? Position { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? MaritalStatus { get; set; }
        public int ChildCount { get; set; }

        // ✅ NEW
        [Required, MaxLength(1)]
        public string Gender { get; set; } = "M";

        [Required]
        public string SalaryType { get; set; } = "Monthly";

        public decimal HourlyRate { get; set; }
        public int MonthlyHours { get; set; }
        public decimal BruttoSalary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }

        public int WeeklyHours { get; set; }
        public decimal? PensumPercent { get; set; }
        public decimal? HolidayRate { get; set; }
        public decimal? OvertimeRate { get; set; }

        public bool HolidayEligible { get; set; }
        public bool ThirteenthEligible { get; set; }
        public bool ThirteenthProrated { get; set; }

        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        // ✅ NEW
        public bool ApplyKTG { get; set; }

        public string PermitType { get; set; } = "B";
        public bool ChurchMember { get; set; }
        public string Canton { get; set; } = "ZH";
        public string? WithholdingTaxCode { get; set; }

        public string? AHVNumber { get; set; }
        public string? Krankenkasse { get; set; }
        public string? BVGPlan { get; set; }

        public string? Address { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
    }
}
