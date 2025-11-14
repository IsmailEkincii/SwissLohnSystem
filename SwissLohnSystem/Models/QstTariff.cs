namespace SwissLohnSystem.API.Models
{
    public class QstTariff
    {
        public int Id { get; set; }

        public string Canton { get; set; } = "ZH";      // ZH, SG, SZ...
        public string Code { get; set; } = "A";         // Tarifcode: A, B, C, D, H...
        public string PermitType { get; set; } = "B";   // B, L, F...
        public bool ChurchMember { get; set; }          // Kirchensteuer: true/false

        public decimal IncomeFrom { get; set; }         // 0
        public decimal IncomeTo { get; set; }           // 6000
        public decimal Rate { get; set; }               // 0.015 = %1.5

        public string? Remark { get; set; }             // opsiyonel açıklama
    }
}
