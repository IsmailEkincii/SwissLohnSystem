using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwissLohnSystem.API.Models
{
    public class Mitarbeiter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Vorname { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nachname { get; set; }

        public string AHVNummer { get; set; } // Sigorta numarası

        public decimal Lohn { get; set; }

        // Firma ilişkisi
        [ForeignKey("Firma")]
        public int FirmaId { get; set; }
        public Firma Firma { get; set; }
    }
}
