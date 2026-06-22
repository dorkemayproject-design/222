using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Models;

namespace RGBSelcer.Services
{
    public class CarService
    {
        public async Task<List<Car>> GetAllCarsAsync()
        {
            using var context = new AppDbContext();
            return await context.Cars.OrderBy(c => c.Brand).ThenBy(c => c.Model).ToListAsync();
        }

        public async Task<List<Car>> GetCarsByCategoryAsync(string category)
        {
            using var context = new AppDbContext();
            return await context.Cars
                .Where(c => c.LicenseCategory == category)
                .OrderBy(c => c.Brand)
                .ToListAsync();
        }

        public async Task<Car?> GetCarByIdAsync(int carId)
        {
            using var context = new AppDbContext();
            return await context.Cars.FindAsync(carId);
        }

        public async Task<Purchase> PurchaseCarAsync(int userId, int carId)
        {
            using var context = new AppDbContext();
            var car = await context.Cars.FindAsync(carId);
            if (car == null)
                throw new InvalidOperationException("Автомобиль не найден.");

            var purchase = new Purchase
            {
                UserId = userId,
                CarId = carId,
                PurchaseDate = DateTime.Now,
                PaidAmount = car.Price,
                Status = "Оформлен"
            };

            context.Purchases.Add(purchase);
            await context.SaveChangesAsync();
            return purchase;
        }

        public async Task<List<Purchase>> GetUserPurchasesAsync(int userId)
        {
            using var context = new AppDbContext();
            return await context.Purchases
                .Include(p => p.Car)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<(int CarCount, int PurchaseCount, decimal TotalSpent)> GetUserStatsAsync(int userId)
        {
            using var context = new AppDbContext();
            var purchases = await context.Purchases.Where(p => p.UserId == userId).ToListAsync();
            var carCount = await context.Cars.CountAsync();
            return (carCount, purchases.Count, purchases.Sum(p => p.PaidAmount));
        }
    }
}
