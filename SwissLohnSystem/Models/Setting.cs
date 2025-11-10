using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Value { get; set; }
        public string Description { get; set; }
    }
}
