namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class QstTariffLookupDto
    {
        public string Canton { get; set; } = "";
        public string Code { get; set; } = "";          // A0, C0, H...
        public string Description { get; set; } = "";   // UI'da gösterilecek
        public decimal Rate { get; set; }               // Son versiyonun oranı (isteğe bağlı)
    }
}
