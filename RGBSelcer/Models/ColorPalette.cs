using System;
using System.Collections.Generic;

namespace RGBSelcer.Models
{
    public class ColorPalette
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
        public List<ColorItem> Colors { get; set; } = new();
    }
}
