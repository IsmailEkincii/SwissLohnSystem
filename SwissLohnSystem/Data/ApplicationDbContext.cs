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

            // QstTariff config
            modelBuilder.Entity<QstTariff>(entity =>
            {
                entity.ToTable("QstTariffs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Canton)
                      .IsRequired()
                      .HasMaxLength(2);

                entity.Property(x => x.Code)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.Property(x => x.PermitType)
                      .IsRequired()
                      .HasMaxLength(5);

                entity.Property(x => x.ChurchMember)
                      .IsRequired();

                entity.Property(x => x.IncomeFrom)
                      .HasColumnType("decimal(18,2)");

                entity.Property(x => x.IncomeTo)
                      .HasColumnType("decimal(18,2)");

                entity.Property(x => x.Rate)
                      .HasColumnType("decimal(18,4)");

                entity.Property(x => x.Remark)
                      .HasMaxLength(200);

                // Sorgu için mantıklı index’ler
                entity.HasIndex(x => new { x.Canton, x.Code, x.PermitType, x.ChurchMember });
                entity.HasIndex(x => new { x.Canton, x.Code, x.PermitType, x.IncomeFrom, x.IncomeTo });
            });
        }


    }
}
