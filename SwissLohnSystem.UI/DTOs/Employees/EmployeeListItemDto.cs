namespace SwissLohnSystem.UI.DTOs.Employees
{
    // ===========================
    //  Liste satırı DTO’su
    // ===========================
    public class EmployeeListItemDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        // Mapping'de FullName olarak kullanıyoruz
        public string FirstName { get; set; } = string.Empty;

        // İstersen ileride kullanmak için:
        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string Phone { get; set; }   
        public string? Position { get; set; }
        public bool Active { get; set; }
    }
}
