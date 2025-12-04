namespace SwissLohnSystem.UI.DTOs.Payroll
{
    public class PayrollRequestDto
    {
        public int EmployeeId { get; set; }

        /// <summary>
        /// Dönem için YYYY-MM-01 gibi DateOnly (sadece yıl+ay önemli)
        /// </summary>
        public DateOnly Period { get; set; }

        /// <summary>
        /// Hesaplanmış aylık Brutto (controller içinde set edilecek)
        /// </summary>
        public decimal GrossMonthly { get; set; }

        // --- Sozialversicherungen flags (modalden gelecek) ---
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }

        // --- Steuer / Rahmenbedingungen ---
        public int WeeklyHours { get; set; }          // opsiyonel: calculator kullanıyorsa
        public string Canton { get; set; } = "ZH";
        public string PermitType { get; set; } = "B";
        public bool ChurchMember { get; set; }

        public string? WithholdingTaxCode { get; set; }

        // --- Ek kalemler (modalde serbest girilebilir) ---
        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public decimal? WorkedDays { get; set; }
        public decimal? SickDays { get; set; }
        public decimal? UnpaidDays { get; set; }

    }
}
