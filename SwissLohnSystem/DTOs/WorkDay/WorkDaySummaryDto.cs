namespace SwissLohnSystem.API.DTOs.WorkDay
{
    public class WorkDaySummaryDto
    {
        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal TotalHours { get; set; }          // Normal çalışma saati
        public decimal TotalOvertimeHours { get; set; }  // Mesai saati
    }
}
