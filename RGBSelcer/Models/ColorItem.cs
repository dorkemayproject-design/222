namespace RGBSelcer.Models
{
    public class ColorItem
    {
        public int Id { get; set; }
        public int PaletteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public string HexCode { get; set; } = "#000000";
        public int SortOrder { get; set; }

        public ColorPalette? Palette { get; set; }
    }
}
