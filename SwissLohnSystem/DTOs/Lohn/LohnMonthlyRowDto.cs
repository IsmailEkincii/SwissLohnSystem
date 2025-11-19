namespace SwissLohnSystem.API.DTOs.Lohn
{
    public class LohnMonthlyRowDto
    {
        public LohnMonthlyRowDto(
            int employeeId,
            string employeeName,
            int month,
            int year,
            decimal bruttoSalary,
            decimal netSalary,
            decimal totalDeductions,
            decimal overtimePay,
            decimal holidayAllowance)
        {
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            Month = month;
            Year = year;
            BruttoSalary = bruttoSalary;
            NetSalary = netSalary;
            TotalDeductions = totalDeductions;
            OvertimePay = overtimePay;
            HolidayAllowance = holidayAllowance;
        }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BruttoSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalDeductions { get; set; }

        public decimal OvertimePay { get; set; }
        public decimal HolidayAllowance { get; set; }
    }
}
