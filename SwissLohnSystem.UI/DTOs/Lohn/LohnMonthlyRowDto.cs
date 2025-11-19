namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Firma → Mitarbeiter → Löhne sekmesindeki tablo için satır DTO'su.
    /// API'den gelen LohnDto + UI kolaylık alanları.
    /// </summary>
    public class LohnMonthlyRowDto
    {
        public int Id { get; set; }               // Lohn Id
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalDeductions { get; set; }

        public decimal OvertimePay { get; set; }
        public decimal HolidayAllowance { get; set; }

        public bool IsFinal { get; set; }
    }
}
