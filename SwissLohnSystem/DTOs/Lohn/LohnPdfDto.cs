namespace SwissLohnSystem.API.DTOs.Lohn
{
    public sealed class LohnPdfDto
    {
        // Lohn
        public int Id { get; init; }
        public int Month { get; init; }
        public int Year { get; init; }

        public decimal BruttoSalary { get; init; }
        public decimal NetSalary { get; init; }

        public decimal EmployeeAhvIvEo { get; init; }
        public decimal EmployeeAlvTotal { get; init; }   // ✅ toplam (PDF basit kalsın diye)
        public decimal EmployeeAlv1 { get; init; }       // opsiyonel
        public decimal EmployeeAlv2 { get; init; }       // opsiyonel

        public decimal EmployeeNbu { get; init; }
        public decimal EmployeeBvg { get; init; }
        public decimal EmployeeKtg { get; init; }
        public decimal EmployeeQst { get; init; }

        public decimal EmployerAhvIvEo { get; init; }
        public decimal EmployerAlvTotal { get; init; }   // ✅ toplam
        public decimal EmployerAlv1 { get; init; }       // opsiyonel
        public decimal EmployerAlv2 { get; init; }       // opsiyonel

        public decimal EmployerBu { get; init; }
        public decimal EmployerBvg { get; init; }
        public decimal EmployerKtg { get; init; }
        public decimal EmployerFak { get; init; }

        public string? WithholdingTaxCode { get; init; }
        public string? Canton { get; init; }

        // Employee
        public string EmployeeName { get; init; } = "";
        public string? EmployeeAddress { get; init; }
        public string? EmployeeZip { get; init; }
        public string? EmployeeCity { get; init; }
        public string? EmployeePhone { get; init; }

        // Company
        public string CompanyName { get; init; } = "";
        public string? CompanyAddress { get; init; }
        public string? CompanyPhone { get; init; }
        public string? CompanyEmail { get; init; }
    }
}
