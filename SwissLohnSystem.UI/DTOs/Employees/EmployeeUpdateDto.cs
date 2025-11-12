using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Employees
{
    public class EmployeeUpdateDto : EmployeeCreateDto
    {
        [Required] public int Id { get; set; }
    }
}
