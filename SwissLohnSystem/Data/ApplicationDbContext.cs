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
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Lohn> Lohns { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<QstTariff> QstTariffs { get; set; } = null!;


        // KALDIRILDI:
        // public DbSet<Firma> Firmen { get; set; }
        // public DbSet<Mitarbeiter> Mitarbeiter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee -> Company relation
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // QstTariff config (opsiyonel, ama tabloyla uyumlu)
            modelBuilder.Entity<QstTariff>(entity =>
            {
                entity.ToTable("QstTariffs");

                entity.Property(e => e.Canton)
                    .HasMaxLength(4)
                    .IsRequired();

                entity.Property(e => e.Code)
                    .HasMaxLength(4)
                    .IsRequired();

                entity.Property(e => e.PermitType)
                    .HasMaxLength(4)
                    .IsRequired();

                entity.Property(e => e.IncomeFrom)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IncomeTo)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18,4)");
            });
        }

    }
}
