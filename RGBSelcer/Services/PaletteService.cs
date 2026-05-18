using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Models;

namespace RGBSelcer.Services
{
    public class PaletteService
    {
        public async Task<List<ColorPalette>> GetUserPalettesAsync(int userId)
        {
            using var context = new AppDbContext();
            return await context.Palettes
                .Include(p => p.Colors)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ColorPalette?> GetPaletteByIdAsync(int paletteId)
        {
            using var context = new AppDbContext();
            return await context.Palettes
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == paletteId);
        }

        public async Task<ColorPalette> CreatePaletteAsync(int userId, string name, string description)
        {
            using var context = new AppDbContext();
            var palette = new ColorPalette
            {
                UserId = userId,
                Name = name,
                Description = description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Palettes.Add(palette);
            await context.SaveChangesAsync();
            return palette;
        }

        public async Task UpdatePaletteAsync(int paletteId, string name, string description)
        {
            using var context = new AppDbContext();
            var palette = await context.Palettes.FindAsync(paletteId);
            if (palette != null)
            {
                palette.Name = name;
                palette.Description = description;
                palette.UpdatedAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeletePaletteAsync(int paletteId)
        {
            using var context = new AppDbContext();
            var palette = await context.Palettes
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == paletteId);

            if (palette != null)
            {
                context.Palettes.Remove(palette);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddColorAsync(int paletteId, string name, byte r, byte g, byte b)
        {
            using var context = new AppDbContext();
            var maxOrder = await context.Colors
                .Where(c => c.PaletteId == paletteId)
                .MaxAsync(c => (int?)c.SortOrder) ?? 0;

            var color = new ColorItem
            {
                PaletteId = paletteId,
                Name = name,
                Red = r,
                Green = g,
                Blue = b,
                HexCode = $"#{r:X2}{g:X2}{b:X2}",
                SortOrder = maxOrder + 1
            };

            context.Colors.Add(color);

            var palette = await context.Palettes.FindAsync(paletteId);
            if (palette != null)
                palette.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
        }

        public async Task UpdateColorAsync(int colorId, string name, byte r, byte g, byte b)
        {
            using var context = new AppDbContext();
            var color = await context.Colors.FindAsync(colorId);
            if (color != null)
            {
                color.Name = name;
                color.Red = r;
                color.Green = g;
                color.Blue = b;
                color.HexCode = $"#{r:X2}{g:X2}{b:X2}";

                var palette = await context.Palettes.FindAsync(color.PaletteId);
                if (palette != null)
                    palette.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteColorAsync(int colorId)
        {
            using var context = new AppDbContext();
            var color = await context.Colors.FindAsync(colorId);
            if (color != null)
            {
                var palette = await context.Palettes.FindAsync(color.PaletteId);
                if (palette != null)
                    palette.UpdatedAt = DateTime.Now;

                context.Colors.Remove(color);
                await context.SaveChangesAsync();
            }
        }

        public async Task<(int PaletteCount, int ColorCount)> GetUserStatsAsync(int userId)
        {
            using var context = new AppDbContext();
            var paletteCount = await context.Palettes.CountAsync(p => p.UserId == userId);
            var colorCount = await context.Colors
                .Include(c => c.Palette)
                .CountAsync(c => c.Palette != null && c.Palette.UserId == userId);
            return (paletteCount, colorCount);
        }
    }
}
