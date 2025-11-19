namespace SwissLohnSystem.API.DTOs.Employees
{
    public class EmployeeListItemDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Position { get; set; }
        public bool Active { get; set; }
    }
}
