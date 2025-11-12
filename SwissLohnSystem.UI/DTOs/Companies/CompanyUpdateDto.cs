using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Companies
{
    public class CompanyUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name ist erforderlich.")]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Kanton ist erforderlich.")]
        [MaxLength(10)]
        public string Canton { get; set; } = null!;

        [MaxLength(300)]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail eingeben.")]
        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }
    }
}
