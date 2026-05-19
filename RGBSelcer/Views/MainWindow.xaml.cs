using System;
using System.Windows;
using System.Windows.Input;
using RGBSelcer.Helpers;
using RGBSelcer.Models;
using RGBSelcer.Services;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;

            if (AuthService.CurrentUser != null)
                _viewModel.UserName = AuthService.CurrentUser.Login;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadCarsAsync();
            AudioService.StartBackgroundMusic();
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            AudioService.StopBackgroundMusic();
        }

        private void CarCard_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element && element.DataContext is Car car)
                {
                    var detailWindow = new CarDetailWindow(car.Id);
                    detailWindow.Owner = this;
                    detailWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия карточки:\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow();
            profileWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthService.Logout();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}
