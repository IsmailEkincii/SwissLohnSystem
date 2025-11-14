using System;

namespace SwissLohnSystem.UI.DTOs.Employees
{
    public sealed record EmployeeDto
    {
        public int Id { get; init; }
        public int CompanyId { get; init; }

        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string? Email { get; init; }
        public string? Position { get; init; }

        public DateTime? BirthDate { get; init; }
        public string? MaritalStatus { get; init; }
        public int ChildCount { get; init; }

        public string SalaryType { get; init; } = "Monthly";
        public decimal HourlyRate { get; init; }
        public int MonthlyHours { get; init; }
        public decimal BruttoSalary { get; init; }

        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool Active { get; init; }

        // Sigorta & kimlik
        public string? AHVNumber { get; init; }
        public string? Krankenkasse { get; init; }
        public string? BVGPlan { get; init; }

        // Parametreler
        public decimal? PensumPercent { get; init; }
        public decimal? HolidayRate { get; init; }
        public decimal? OvertimeRate { get; init; }
        public string? WithholdingTaxCode { get; init; }

        // Bordro varsayılanları
        public int WeeklyHours { get; init; }
        public bool ApplyAHV { get; init; }
        public bool ApplyALV { get; init; }
        public bool ApplyNBU { get; init; }
        public bool ApplyBU { get; init; }
        public bool ApplyBVG { get; init; }
        public bool ApplyFAK { get; init; }
        public bool ApplyQST { get; init; }

        public bool HolidayEligible { get; init; }
        public bool ThirteenthEligible { get; init; }
        public bool ThirteenthProrated { get; init; }

        public string PermitType { get; init; } = "B";
        public bool ChurchMember { get; init; }
        public string Canton { get; init; } = "ZH";

        // Adres & iletişim
        public string? Address { get; init; }
        public string? Zip { get; init; }
        public string? City { get; init; }
        public string? Phone { get; init; }
    }
}
