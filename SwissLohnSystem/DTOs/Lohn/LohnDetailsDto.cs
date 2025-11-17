namespace SwissLohnSystem.UI.DTOs.Lohn
{
    public class LohnDetailsDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal ChildAllowance { get; set; }
        public decimal HolidayAllowance { get; set; }
        public decimal OvertimePay { get; set; }

        // 🔥 Saat alanları
        public decimal MonthlyHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsFinal { get; set; }

        // Employee / Company display alanların varsa buraya ekli
        public string? EmployeeFullName { get; set; }
        public string? CompanyName { get; set; }
        public string? EmployeeName { get; internal set; }
        public int? CompanyId { get; internal set; }
        public object Items { get; internal set; }
    }
}
