namespace SwissLohnSystem.UI.DTOs.Employees
{
    public class EmployeeEditDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        // Stammdaten
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? AHVNumber { get; set; }

        // Adresse
        public string? Street { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }

        // 🧮 Arbeitszeit & Payroll
        public decimal WeeklyHours { get; set; }
        public decimal PensumPercent { get; set; }
        public decimal HolidayRate { get; set; }
        public decimal OvertimeRate { get; set; }

        // İzin / Kanton / Kilise
        public string? PermitType { get; set; }
        public string? Canton { get; set; }
        public bool ChurchMember { get; set; }

        // Sosyal sigortalar
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }

        // Quellensteuer
        public bool ApplyQST { get; set; }
        public string? WithholdingTaxCode { get; set; }

        // Ek haklar
        public bool HolidayEligible { get; set; }
        public bool ThirteenthEligible { get; set; }
        public bool ThirteenthProrated { get; set; }
    }
}