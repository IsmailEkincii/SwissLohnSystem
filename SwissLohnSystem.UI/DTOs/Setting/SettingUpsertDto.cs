using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Setting
{
    public class SettingUpsertDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        // Value string olarak kalır: yüzde, decimal, bool vs. hepsi burada taşınır.
        [Required]
        public string Value { get; set; } = string.Empty;
    }
}
