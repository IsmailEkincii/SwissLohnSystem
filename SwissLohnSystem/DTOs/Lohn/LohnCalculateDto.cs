using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Lohn;

public class LohnCalculateDto
{
    [Required] public int EmployeeId { get; set; }
    [Range(1, 12)] public int Month { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
}
