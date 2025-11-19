namespace SwissLohnSystem.API.DTOs.Lohn
{
    public class CompanyMonthlyLohnDto
    {
        public int Id { get; set; }                // 🔥 En önemli ek: Lohn satırının Id'si
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BruttoSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public bool IsFinal { get; set; }
    }
}
