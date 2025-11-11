using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.Setting;

public class SettingUpdateDto : SettingCreateDto
{
    [Required] public int Id { get; set; }
}
