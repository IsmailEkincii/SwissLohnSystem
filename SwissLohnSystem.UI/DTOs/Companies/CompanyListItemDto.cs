namespace SwissLohnSystem.UI.DTOs.Companies
{
    /// <summary>
    /// Firmen listesi için hafif DTO (UI).
    /// </summary>
    public class CompanyListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Canton { get; set; } = "";
        public string? Email { get; set; }
        public int? EmployeeCount { get; set; } // opsiyonel: liste sayfasında göstermek için
    }
}
