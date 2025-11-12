namespace SwissLohnSystem.UI.DTOs.Employees
{
    public class EmployeeListItemDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string FullName { get; set; } = ""; // UI tarafında FirstName + LastName ile doldur
        public string? Email { get; set; }
        public string? Position { get; set; }
        public bool Active { get; set; }
    }
}
