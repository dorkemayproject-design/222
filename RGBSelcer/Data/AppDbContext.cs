using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Models;

namespace RGBSelcer.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Purchase> Purchases { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SelcerRoyalityPRM",
                "selcer.db");

            var dir = Path.GetDirectoryName(dbPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.Login).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LicenseCategory).IsRequired().HasMaxLength(5);
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Purchases)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Car)
                      .WithMany()
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
