using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.Models
{
    public class Company
    {
        [Key] public int Id { get; set; }

        [Required] public string Name { get; set; } = null!;
        [Required] public string Canton { get; set; } = null!;

        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? TaxNumber { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
