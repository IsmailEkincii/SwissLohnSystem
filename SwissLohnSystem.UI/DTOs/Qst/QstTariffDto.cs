namespace SwissLohnSystem.UI.DTOs.Qst
{
    public class QstTariffDto
    {
        public int Id { get; set; }
        public string Canton { get; set; } = "ZH";
        public string Code { get; set; } = "A";
        public string PermitType { get; set; } = "B";
        public bool ChurchMember { get; set; }
        public decimal IncomeFrom { get; set; }
        public decimal IncomeTo { get; set; }
        public decimal Rate { get; set; }
        public string? Remark { get; set; }
    }

}
