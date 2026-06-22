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
        public class CarExportData
        {
            public string Brand { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public int Year { get; set; }
            public decimal Price { get; set; }
            public string LicenseCategory { get; set; } = string.Empty;
            public int MaxSpeedKmh { get; set; }
            public int HorsePower { get; set; }
            public string EngineType { get; set; } = string.Empty;
            public string ExportedAt { get; set; } = string.Empty;
        }

        public async Task ExportCarsToJsonAsync(List<Car> cars, string filePath, IProgress<int>? progress = null)
        {
            progress?.Report(20);

            var exportData = cars.Select(c => new CarExportData
            {
                Brand = c.Brand,
                Model = c.Model,
                Year = c.Year,
                Price = c.Price,
                LicenseCategory = c.LicenseCategory,
                MaxSpeedKmh = c.MaxSpeedKmh,
                HorsePower = c.HorsePower,
                EngineType = c.EngineType,
                ExportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            progress?.Report(50);

            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            progress?.Report(100);
        }

        public async Task ExportCarsToCsvAsync(List<Car> cars, string filePath, IProgress<int>? progress = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Brand,Model,Year,Price,Category,MaxSpeed,HorsePower,Engine");

            progress?.Report(20);

            var total = cars.Count;
            for (int i = 0; i < total; i++)
            {
                var c = cars[i];
                sb.AppendLine($"\"{c.Brand}\",\"{c.Model}\",{c.Year},{c.Price},\"{c.LicenseCategory}\",{c.MaxSpeedKmh},{c.HorsePower},\"{c.EngineType}\"");

                var percent = 20 + (int)((i + 1.0) / total * 60);
                progress?.Report(percent);
            }

            await File.WriteAllTextAsync(filePath, sb.ToString());
            progress?.Report(100);
        }

        public async Task ExportCarsToTxtAsync(List<Car> cars, string filePath, IProgress<int>? progress = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════╗");
            sb.AppendLine("║       SELCER ROYALITY PRM — Каталог         ║");
            sb.AppendLine("╚══════════════════════════════════════════════╝");
            sb.AppendLine($"  Дата экспорта: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('─', 50));
            sb.AppendLine();

            progress?.Report(20);

            var total = cars.Count;
            for (int i = 0; i < total; i++)
            {
                var c = cars[i];
                sb.AppendLine($"  ★ {c.Brand} {c.Model} ({c.Year})");
                sb.AppendLine($"    Цена: {c.Price:N0} ₽");
                sb.AppendLine($"    Категория прав: {c.LicenseCategory}");
                sb.AppendLine($"    Макс. скорость: {c.MaxSpeedKmh} км/ч");
                sb.AppendLine($"    Мощность: {c.HorsePower} л.с.");
                sb.AppendLine($"    Двигатель: {c.EngineType} ({c.EngineVolume} л)");
                sb.AppendLine($"    Разгон 0-100: {c.Acceleration0to100} сек");
                sb.AppendLine();

                var percent = 20 + (int)((i + 1.0) / total * 60);
                progress?.Report(percent);
            }

            sb.AppendLine(new string('─', 50));
            sb.AppendLine($"  Всего автомобилей: {cars.Count}");

            await File.WriteAllTextAsync(filePath, sb.ToString());
            progress?.Report(100);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await Task.Run(() =>
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            });
        }
    }
}
