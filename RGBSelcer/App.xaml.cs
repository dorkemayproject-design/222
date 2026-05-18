using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Models;

namespace RGBSelcer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new AppDbContext();
            context.Database.EnsureCreated();

            if (!context.Users.Any(u => u.Login == "GURSKIY"))
            {
                context.Users.Add(new User
                {
                    Login = "GURSKIY",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("IGOR"),
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
            }
        }
    }
}
