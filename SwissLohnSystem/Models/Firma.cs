using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class Firma

    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Adresse { get; set; }

        [MaxLength(50)]
        public string SteuerNummer { get; set; }

        // Firma'ya bağlı çalışanlar
        public ICollection<Mitarbeiter> Mitarbeiter { get; set; }
    }
}
