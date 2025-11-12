using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Lohn hesaplama isteği (UI → API).
    /// </summary>
    public class LohnCalculateDto
    {
        [Required] public int EmployeeId { get; set; }

        [Range(1, 12, ErrorMessage = "Monat 1..12 aralığında olmalı.")]
        public int Month { get; set; }

        [Range(2000, 2100, ErrorMessage = "Jahr 2000..2100 aralığında olmalı.")]
        public int Year { get; set; }

        // Gelecekte: Kanton, PermitType, Church, Children override vs. buraya eklenebilir.
    }
}
