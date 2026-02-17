namespace SwissLohnSystem.API.DTOs.Payroll
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
