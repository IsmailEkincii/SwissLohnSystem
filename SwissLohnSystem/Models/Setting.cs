using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class Setting
    {
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = null!;

        // ✅ string olmalı (UI text input + 0.053 / 5,3 destek)
        [MaxLength(200)]
        public string? Value { get; set; }

        [MaxLength(400)]
        public string? Description { get; set; }

        // ✅ sende yoktu, Seeder/Mapping bunu kullanıyor
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
