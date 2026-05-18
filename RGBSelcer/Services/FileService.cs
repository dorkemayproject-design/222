using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RGBSelcer.Models;

namespace RGBSelcer.Services
{
    public class FileService
    {
        public class PaletteExportData
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ExportedAt { get; set; } = string.Empty;
            public List<ColorExportData> Colors { get; set; } = new();
        }

        public class ColorExportData
        {
            public string Name { get; set; } = string.Empty;
            public byte Red { get; set; }
            public byte Green { get; set; }
            public byte Blue { get; set; }
            public string HexCode { get; set; } = string.Empty;
        }

        public async Task ExportPaletteToJsonAsync(ColorPalette palette, string filePath, IProgress<int>? progress = null)
        {
            var exportData = new PaletteExportData
            {
                Name = palette.Name,
                Description = palette.Description,
                ExportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            progress?.Report(20);

            foreach (var color in palette.Colors)
            {
                exportData.Colors.Add(new ColorExportData
                {
                    Name = color.Name,
                    Red = color.Red,
                    Green = color.Green,
                    Blue = color.Blue,
                    HexCode = color.HexCode
                });
            }

            progress?.Report(50);

            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            progress?.Report(100);
        }

        public async Task ExportPaletteToCsvAsync(ColorPalette palette, string filePath, IProgress<int>? progress = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Red,Green,Blue,HexCode");

            progress?.Report(20);

            var total = palette.Colors.Count;
            for (int i = 0; i < total; i++)
            {
                var color = palette.Colors[i];
                sb.AppendLine($"\"{color.Name}\",{color.Red},{color.Green},{color.Blue},\"{color.HexCode}\"");

                var percent = 20 + (int)((i + 1.0) / total * 60);
                progress?.Report(percent);
            }

            await File.WriteAllTextAsync(filePath, sb.ToString());

            progress?.Report(100);
        }

        public async Task ExportPaletteToTxtAsync(ColorPalette palette, string filePath, IProgress<int>? progress = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Палитра: {palette.Name}");
            sb.AppendLine($"Описание: {palette.Description}");
            sb.AppendLine($"Дата экспорта: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine();

            progress?.Report(20);

            var total = palette.Colors.Count;
            for (int i = 0; i < total; i++)
            {
                var color = palette.Colors[i];
                sb.AppendLine($"  {color.Name}");
                sb.AppendLine($"    RGB: ({color.Red}, {color.Green}, {color.Blue})");
                sb.AppendLine($"    HEX: {color.HexCode}");
                sb.AppendLine();

                var percent = 20 + (int)((i + 1.0) / total * 60);
                progress?.Report(percent);
            }

            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Всего цветов: {palette.Colors.Count}");

            await File.WriteAllTextAsync(filePath, sb.ToString());

            progress?.Report(100);
        }

        public async Task<PaletteExportData?> ImportPaletteFromJsonAsync(string filePath, IProgress<int>? progress = null)
        {
            progress?.Report(10);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден.", filePath);

            var json = await File.ReadAllTextAsync(filePath);
            progress?.Report(50);

            var data = JsonConvert.DeserializeObject<PaletteExportData>(json);
            progress?.Report(100);

            return data;
        }

        public async Task<PaletteExportData?> ImportPaletteFromCsvAsync(string filePath, IProgress<int>? progress = null)
        {
            progress?.Report(10);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден.", filePath);

            var lines = await File.ReadAllLinesAsync(filePath);
            progress?.Report(30);

            if (lines.Length < 2)
                throw new InvalidDataException("CSV файл пуст или содержит только заголовок.");

            var data = new PaletteExportData
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Description = "Импортировано из CSV",
                ExportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var total = lines.Length - 1;
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = ParseCsvLine(lines[i]);
                if (parts.Length >= 5)
                {
                    data.Colors.Add(new ColorExportData
                    {
                        Name = parts[0].Trim('"'),
                        Red = byte.Parse(parts[1]),
                        Green = byte.Parse(parts[2]),
                        Blue = byte.Parse(parts[3]),
                        HexCode = parts[4].Trim('"')
                    });
                }

                var percent = 30 + (int)((i * 1.0) / total * 60);
                progress?.Report(percent);
            }

            progress?.Report(100);
            return data;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await Task.Run(() =>
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            });
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
