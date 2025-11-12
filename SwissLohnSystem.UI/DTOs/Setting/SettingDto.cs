using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Setting
{
    public sealed class SettingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Description { get; set; }
    }
}
