using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.WorkDay;

public class WorkDayCreateDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Range(0, 24, ErrorMessage = "Arbeitsstunden müssen zwischen 0 und 24 liegen.")]
    public decimal HoursWorked { get; set; }

    [Range(0, 24, ErrorMessage = "Überstunden müssen zwischen 0 und 24 liegen.")]
    public decimal OvertimeHours { get; set; }
}
