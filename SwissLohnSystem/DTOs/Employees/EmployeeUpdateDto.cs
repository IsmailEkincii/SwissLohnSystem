using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Employees;

public class EmployeeUpdateDto : EmployeeCreateDto
{
    [Required] public int Id { get; set; }
}
