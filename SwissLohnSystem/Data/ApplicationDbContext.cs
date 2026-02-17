using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<Lohn> Lohns { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<QstTariff> QstTariffs => Set<QstTariff>();
        public DbSet<BvgPlan> BvgPlans => Set<BvgPlan>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Employee -> Company relation
            // ============================
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Setting>()
                .HasIndex(x => new { x.CompanyId, x.Name })
                .IsUnique();

            modelBuilder.Entity<QstTariff>()
                .HasIndex(x => new { x.CompanyId, x.Canton, x.Code, x.PermitType, x.ChurchMember, x.IncomeFrom, x.IncomeTo });

            modelBuilder.Entity<BvgPlan>()
                .HasIndex(x => new { x.CompanyId, x.PlanCode })
                .IsUnique();
            // ============================
            // Setting config (Company scoped Unique Key)
            // ============================
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.ToTable("Settings");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CompanyId).IsRequired();

                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Description)
                      .HasMaxLength(255);

                entity.Property(x => x.Value)
                      .HasPrecision(18, 6);

                // ✅ company scoped unique
                entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
                entity.HasIndex(x => x.CompanyId);
            });

            // ============================
            // QstTariff config (Company scoped)
            // ============================
            modelBuilder.Entity<QstTariff>(entity =>
            {
                entity.ToTable("QstTariffs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CompanyId).IsRequired(); // ✅ NEW

                entity.Property(x => x.Canton)
                      .IsRequired()
                      .HasMaxLength(2);

                entity.Property(x => x.Code)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.Property(x => x.PermitType)
                      .IsRequired()
                      .HasMaxLength(5);

                entity.Property(x => x.ChurchMember).IsRequired();

                entity.Property(x => x.IncomeFrom).HasColumnType("decimal(18,2)");
                entity.Property(x => x.IncomeTo).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Rate).HasColumnType("decimal(18,4)");

                entity.Property(x => x.Remark).HasMaxLength(200);

                entity.HasIndex(x => new
                {
                    x.CompanyId,
                    x.Canton,
                    x.Code,
                    x.PermitType,
                    x.ChurchMember,
                    x.IncomeFrom,
                    x.IncomeTo
                }).IsUnique();

                entity.HasIndex(x => new { x.CompanyId, x.Canton, x.Code, x.PermitType, x.ChurchMember });
            });
        }
    }
}
