using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Services;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class CarDetailWindow : Window
    {
        private readonly CarDetailViewModel _viewModel;
        private readonly int _carId;

        public CarDetailWindow(int carId)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"XAML init error:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Debug", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            _viewModel = (CarDetailViewModel)DataContext;
            _carId = carId;
            Loaded += CarDetailWindow_Loaded;
        }

        private async void CarDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadCarAsync(_carId);
        }

        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Car == null || AuthService.CurrentUser == null) return;

            var car = _viewModel.Car;
            var user = AuthService.CurrentUser;

            if (user.Balance < car.Price)
            {
                var deficit = car.Price - user.Balance;
                var result = MessageBox.Show(
                    $"Недостаточно средств на балансе!\n\n" +
                    $"Цена: {car.Price:N0} ₽\n" +
                    $"Ваш баланс: {user.Balance:N0} ₽\n" +
                    $"Не хватает: {deficit:N0} ₽\n\n" +
                    $"Пополнить баланс?",
                    "SELCER ROYALITY PRM — Недостаточно средств",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var topUpWindow = new TopUpWindow();
                    topUpWindow.Owner = this;
                    topUpWindow.ShowDialog();
                }
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Подтвердите покупку:\n\n{car.Brand} {car.Model}\n" +
                $"Цена: {car.Price:N0} ₽\n" +
                $"Ваш баланс: {user.Balance:N0} ₽\n" +
                $"Баланс после покупки: {(user.Balance - car.Price):N0} ₽\n\n" +
                $"Оформление займёт 3 минуты.\nПродолжить?",
                "SELCER ROYALITY PRM — Покупка",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.Yes)
            {
                var timerWindow = new PurchaseTimerWindow($"{car.Brand} {car.Model}", car.Price);
                timerWindow.Owner = this;
                timerWindow.ShowDialog();

                if (timerWindow.PurchaseCompleted)
                {
                    try
                    {
                        using var context = new AppDbContext();
                        var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                        if (dbUser == null) return;

                        dbUser.Balance -= car.Price;
                        await context.SaveChangesAsync();
                        AuthService.CurrentUser!.Balance = dbUser.Balance;

                        await _viewModel.PurchaseCommand.ExecuteAsync(null);

                        if (_viewModel.IsPurchased)
                        {
                            MessageBox.Show(
                                $"Поздравляем с покупкой!\n\n{car.Brand} {car.Model}\n" +
                                $"Списано: {car.Price:N0} ₽\n" +
                                $"Остаток на балансе: {dbUser.Balance:N0} ₽",
                                "SELCER ROYALITY PRM",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при оформлении: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
