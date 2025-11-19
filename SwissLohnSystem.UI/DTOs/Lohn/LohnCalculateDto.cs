using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.Lohn
{
    /// <summary>
    /// Lohn hesaplama isteği (UI → API).
    /// </summary>
    public class LohnCalculateDto
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
