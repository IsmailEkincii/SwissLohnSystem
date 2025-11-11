using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Setting;

public class SettingUpdateDto : SettingCreateDto
{
    [Required] public int Id { get; set; }
}
