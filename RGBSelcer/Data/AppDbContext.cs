using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Models;

namespace RGBSelcer.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ColorPalette> Palettes { get; set; } = null!;
        public DbSet<ColorItem> Colors { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RGBSelcer",
                "rgbselcer.db");

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
            });

            modelBuilder.Entity<ColorPalette>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Palettes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ColorItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.HasOne(e => e.Palette)
                      .WithMany(p => p.Colors)
                      .HasForeignKey(e => e.PaletteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
