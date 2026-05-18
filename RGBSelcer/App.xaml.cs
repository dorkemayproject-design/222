using System.Windows;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;

namespace RGBSelcer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new AppDbContext();
            context.Database.EnsureCreated();
        }
    }
}
