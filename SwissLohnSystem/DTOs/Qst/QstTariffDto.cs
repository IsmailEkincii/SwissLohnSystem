using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Qst
{
    public class QstTariffDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }  
        public string Canton { get; set; } = "ZH";
        public string Code { get; set; } = null!;
        public string PermitType { get; set; } = "B";
        public bool ChurchMember { get; set; }
        public decimal IncomeFrom { get; set; }
        public decimal IncomeTo { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }

        public string? Remark { get; set; }
    }

}
