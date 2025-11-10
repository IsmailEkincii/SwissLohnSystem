using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Lohn> Lohns { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<Firma> Firmen { get; set; }
        public DbSet<Mitarbeiter> Mitarbeiter { get; set; }
    }
}
