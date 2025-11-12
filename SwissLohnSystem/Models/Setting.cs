using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class Setting
    {
        public int Id { get; set; }

        [Required, MaxLength(100)] // 👈 ekledik
        public string Name { get; set; } = string.Empty;

        public decimal Value { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
