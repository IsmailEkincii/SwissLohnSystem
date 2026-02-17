namespace SwissLohnSystem.UI.DTOs.Lohn
{
    public sealed class LohnausweisDto
    {
        // Meta
        public int Year { get; set; }
        public bool IsComplete { get; set; }
        public List<int> MissingMonths { get; set; } = new();
        public List<int> NonFinalMonths { get; set; } = new();

        // Employee
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string? EmployeeAddress { get; set; }
        public string? EmployeeZip { get; set; }
        public string? EmployeeCity { get; set; }

        // Company
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }

        // ==========================
        // Lohnausweis core amounts
        // ==========================

        // 8: Bruttolohn (sum)
        public decimal BruttoTotal_8 { get; set; }

        // 2.2: Privatanteil Geschäftsfahrzeug / Privatanteile (sum)
        public decimal PrivateBenefit_2_2 { get; set; }

        // 9: AHV/IV/EO + ALV + NBU (employee)
        public decimal AhvIvEo_9 { get; set; }
        public decimal AlvTotal_9 { get; set; }
        public decimal Nbu_9 { get; set; }
        public decimal SocialTotal_9 => AhvIvEo_9 + AlvTotal_9 + Nbu_9;

        // 10: Berufliche Vorsorge (BVG, employee)
        public decimal Bvg_10 { get; set; }

        // 12: Quellensteuer (sum)
        public decimal Quellensteuer_12 { get; set; }

        // 13: Spesen
        public decimal PauschalSpesen_13 { get; set; }
        public decimal EffektivSpesen_13 { get; set; }

        // 11: Nettolohn (sum of NetSalary snapshot)
        public decimal NetTotal_11 { get; set; }

        // Comments / Remark
        public string? Remark_15 { get; set; }
    }
}
