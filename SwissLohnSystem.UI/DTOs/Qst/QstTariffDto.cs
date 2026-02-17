using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Qst
{
    public sealed class QstTariffDto
    {
        public int Id { get; set; }

        // Company-scoped (API’de varsa gönderilir; yoksa UI’da opsiyonel kalır)
        public int CompanyId { get; set; }

        [Required]
        [StringLength(10)]
        public string Canton { get; set; } = "ZH";

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = "A";

        [Required]
        [StringLength(10)]
        public string PermitType { get; set; } = "B";

        public bool ChurchMember { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IncomeFrom { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IncomeTo { get; set; }

        /// <summary>
        /// API/DB rate (0..1). Örn: 0.045
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }


        /// <summary>
        /// UI’da yüzde giriş/çıkış için. Örn: 4.5
        /// </summary>
        [Range(0, 100)]
        public decimal DisplayRate { get; set; }

        [StringLength(200)]
        public string? Remark { get; set; }

        // Yardımcı (UI listesi)
        public string DisplayKey => $"{Canton}-{Code}-{PermitType}" + (ChurchMember ? "-K" : "");
    }
}
