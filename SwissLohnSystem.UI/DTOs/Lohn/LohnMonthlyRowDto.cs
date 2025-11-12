namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Firma → Mitarbeiter → Löhne sekmesindeki tablo için satır DTO'su.
    /// API'den gelen LohnDto + UI kolaylık alanları.
    /// </summary>
    public class LohnMonthlyRowDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public int Month { get; set; }            // 1..12
        public int Year { get; set; }             // YYYY
        public string Period => $"{Year}-{Month.ToString().PadLeft(2, '0')}";

        public decimal BruttoSalary { get; set; }
        public decimal NetSalary { get; set; }

        public bool IsFinal { get; set; }

        // UI kolaylık alanları
        public string EmployeeName { get; set; } = ""; // UI birleştirecek: FirstName + LastName
    }
}
