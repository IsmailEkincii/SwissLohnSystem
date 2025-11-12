namespace SwissLohnSystem.UI.DTOs.Payroll
{
    public sealed class PayrollResponseDto
    {
        public string Period { get; set; } = string.Empty;
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
        public decimal WithholdingTax { get; set; }
        public decimal Other { get; set; }
        public decimal Total => AHV_IV_EO + ALV + UVG_NBU + BVG + WithholdingTax + Other;
    }

    public sealed class PayrollItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "deduction";
        public decimal Amount { get; set; }
        public string Basis { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string Side { get; set; } = "employee";
    }
}
