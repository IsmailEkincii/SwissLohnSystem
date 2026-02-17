using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Setting
{
    public sealed class SettingDto
    {
        public int Id { get; set; }       // <-- EKLİ
        public int CompanyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Value { get; set; } = "";
        public string? Description { get; set; }
    }
}
