namespace SwissLohnSystem.UI.DTOs.Lohn
{
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

        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }

        // Display alanları
        public string? EmployeeFullName { get; set; }
        public string? CompanyName { get; set; }
        public string? EmployeeName { get; internal set; }
        public int? CompanyId { get; internal set; }
        public object? Items { get; internal set; }

        // --- Snapshot parametreler (UI'de detay tabında gösterebilirsin) ---
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
    }
}
