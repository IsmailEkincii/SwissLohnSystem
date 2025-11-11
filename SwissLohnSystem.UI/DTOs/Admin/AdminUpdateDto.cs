using System.ComponentModel.DataAnnotations;
namespace SwissLohnSystem.UI.DTOs.Admin;
public class AdminUpdateDto : AdminCreateDto
{
    [Required] public int Id { get; set; }
}