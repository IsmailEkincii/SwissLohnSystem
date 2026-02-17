using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class QstTariff
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }

        [MaxLength(2)]
        public string Canton { get; set; } = "ZH";   // ZH, AG, VD ...

        [MaxLength(10)]
        public string Code { get; set; } = "";       // A0, B0, C0, H ...

        [MaxLength(5)]
        public string PermitType { get; set; } = ""; // B, C, L, G, F, N

        public bool ChurchMember { get; set; }       // Kirchensteuer: true/false

        // Aylık gelir aralığı (brüt)
        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeFrom { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeTo { get; set; }

        // Vergi oranı (0.045 = %4.5)
        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }

        [MaxLength(200)]
        public string? Remark { get; set; }     // "ledig, 0 Kinder..." vb.
    }
}
