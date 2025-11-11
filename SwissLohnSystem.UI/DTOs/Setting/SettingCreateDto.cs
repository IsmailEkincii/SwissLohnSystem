using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Setting;

public class SettingCreateDto
{
    [Required] public string Name { get; set; } = null!;
    [Range(0, 1_000_000)] public decimal Value { get; set; }  // yüzde ya da tutar olabilir
    public string? Description { get; set; }
}
