namespace SwissLohnSystem.UI.DTOs.Payroll
{
    public sealed class PayrollResponseDto
    {
        public string Period { get; set; } = string.Empty;   // YYYY-MM
        public MoneyBreakdownDto Employee { get; set; } = new();
        public MoneyBreakdownDto Employer { get; set; } = new();
        public decimal NetToPay { get; set; }
        public decimal EmployerTotalCost { get; set; }
        public List<PayrollItemDto> Items { get; set; } = new();
    }

    public sealed class MoneyBreakdownDto
    {
        public decimal AHV_IV_EO { get; set; }
        public decimal ALV { get; set; }
        public decimal UVG_NBU { get; set; }
        public decimal BVG { get; set; }

        // ✅ NEW (Excel’e göre KTG yüzdelik)
        public decimal KTG { get; set; }

        public decimal WithholdingTax { get; set; }
        public decimal Other { get; set; }

        public decimal Total => AHV_IV_EO + ALV + UVG_NBU + BVG + KTG + WithholdingTax + Other;
    }

    public sealed class PayrollItemDto
    {
        public string Code { get; set; } = string.Empty;      // Örn: AHV, ALV, NBU, BU, BVG, QST, FAK, KTG
        public string Title { get; set; } = string.Empty;     // DE açıklama
        public string Type { get; set; } = "deduction";       // deduction | contribution | earning | info
        public decimal Amount { get; set; }                   // CHF tutarı
        public string Basis { get; set; } = string.Empty;     // örn. "Brutto", "Koord. Lohn"
        public decimal Rate { get; set; }                     // örn. 0.053
        public string Side { get; set; } = "employee";        // employee | employer
    }
}
