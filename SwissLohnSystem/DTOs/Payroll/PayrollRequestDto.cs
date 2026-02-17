namespace SwissLohnSystem.API.DTOs.Payroll
{
    public class PayrollRequestDto
    {
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }

        public DateOnly Period { get; set; }

        // Service gross hesabını buraya yazar (base gross)
        public decimal GrossMonthly { get; set; }

        // mevcut ekler
        public decimal Bonus { get; set; }
        public decimal ExtraAllowance { get; set; }
        public decimal UnpaidDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        // ✅ MA1: Privatanteile (brüte EK) ve Manuell (±)
        // - PrivateBenefitAmount: brüte eklenir (benefit)
        // - ManualAdjustment: brüte eklenir (negatif de olabilir)
        public decimal PrivateBenefitAmount { get; set; }
        public decimal ManualAdjustment { get; set; }

        // ✅ Excel mantığı
        public decimal ChildAllowance { get; set; }          // Kinderzulage
        public decimal PauschalExpenses { get; set; }        // Pauschalspesen (QST start bazında var)
        public decimal EffectiveExpenses { get; set; }       // Effektivspesen (QST bazında yok)
        public decimal ShortTimeWorkDeduction { get; set; }  // Kurzarbeit Abzug (QST start bazından düşer)

        public bool Include13thSalary { get; set; }
        public decimal ThirteenthSalaryAmount { get; set; }  // 13. maaş payı

        public int CanteenDays { get; set; }
        public decimal CanteenDailyRate { get; set; }

        // BVG fix
        public decimal BvgFixEmployee { get; set; }
        public decimal BvgFixEmployer { get; set; }

        // Flags
        public bool ApplyAHV { get; set; }
        public bool ApplyALV { get; set; }
        public bool ApplyBVG { get; set; }
        public bool ApplyNBU { get; set; }
        public bool ApplyBU { get; set; }
        public bool ApplyFAK { get; set; }
        public bool ApplyQST { get; set; }
        public bool ApplyKTG { get; set; }

        public int WeeklyHours { get; set; }

        // QST inputs
        public string? Canton { get; set; }
        public string? PermitType { get; set; }
        public bool ChurchMember { get; set; }
        public string? WithholdingTaxCode { get; set; }

        // KTG inputs
        public string? Gender { get; set; }

        // BVG plan (fallback)
        public BvgPlanDto? BvgPlan { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? PensumPercent { get; set; }

        // Monatslohn prorata inputs
        public decimal? WorkedDays { get; set; }
        public decimal? SickDays { get; set; }
        public decimal? UnpaidDays { get; set; }

        public decimal GetCanteenDeduction() =>
            CanteenDays > 0 && CanteenDailyRate > 0m ? CanteenDays * CanteenDailyRate : 0m;
    }
}
