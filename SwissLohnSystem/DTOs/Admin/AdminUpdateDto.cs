using System.ComponentModel.DataAnnotations;
namespace SwissLohnSystem.API.DTOs.Admin;
public class AdminUpdateDto : AdminCreateDto
{
    [Required] public int Id { get; set; }
}