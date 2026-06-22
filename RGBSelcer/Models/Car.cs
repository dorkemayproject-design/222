using System;

namespace RGBSelcer.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string LicenseCategory { get; set; } = "B";
        public int MaxSpeedKmh { get; set; }
        public int HorsePower { get; set; }
        public string EngineType { get; set; } = string.Empty;
        public double EngineVolume { get; set; }
        public string Transmission { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public double FuelConsumption { get; set; }
        public int Seats { get; set; }
        public string BodyType { get; set; } = string.Empty;
        public string DriveType { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Acceleration0to100 { get; set; }
        public int Weight { get; set; }
        public string Country { get; set; } = string.Empty;

        public string DisplayName => $"{Brand} {Model} ({Year})";
        public string PriceFormatted => $"{Price:N0} ₽";
        public string CategoryDisplay => LicenseCategory == "A" ? "Категория A (мотоцикл)" : "Категория B (легковой)";
    }
}
