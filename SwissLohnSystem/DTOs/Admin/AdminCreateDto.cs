using System.ComponentModel.DataAnnotations;
namespace SwissLohnSystem.API.DTOs.Admin;
public class AdminCreateDto
{
    [Required] public string Username { get; set; } = null!;
    [Required] public string PasswordHash { get; set; } = null!;
}