using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
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
            {
                _viewModel.UserName = AuthService.CurrentUser.Login;
                _viewModel.UpdateBalance();
            }

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
                    _viewModel.UpdateBalance();
                    UpdateOverlayBalance();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия карточки:\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowProfilePanel();
        }

        private async Task ShowProfilePanel()
        {
            if (AuthService.CurrentUser != null)
            {
                ProfileLogin.Text = $"Логин: {AuthService.CurrentUser.Login}";
                ProfileDate.Text = $"Регистрация: {AuthService.CurrentUser.CreatedAt:dd.MM.yyyy HH:mm}";

                try
                {
                    var carService = new CarService();
                    var (_, purchaseCount, totalSpent) =
                        await carService.GetUserStatsAsync(AuthService.CurrentUser.Id);
                    ProfilePurchaseCount.Text = $"Покупок: {purchaseCount}";
                    ProfileTotalSpent.Text = $"Потрачено: {totalSpent:N0} ₽";

                    var purchases = await carService.GetUserPurchasesAsync(AuthService.CurrentUser.Id);
                    ProfilePurchasesGrid.ItemsSource = purchases;
                }
                catch { }
            }

            ProfileOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProfileOverlay.BeginAnimation(OpacityProperty, fadeIn);

            var slideIn = new DoubleAnimation(-500, 0, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProfilePanelTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        }

        private void HideProfilePanel()
        {
            var slideOut = new DoubleAnimation(0, -500, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) => ProfileOverlay.Visibility = Visibility.Collapsed;

            ProfilePanelTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
            ProfileOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CloseProfileOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            HideProfilePanel();
        }

        private void CloseProfilePanel_Click(object sender, RoutedEventArgs e)
        {
            HideProfilePanel();
        }

        private async void ProfileChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ProfilePasswordError.Visibility = Visibility.Collapsed;
            ProfilePasswordSuccess.Visibility = Visibility.Collapsed;

            var authService = new AuthService();
            var (success, message) = await authService.ChangePasswordAsync(
                ProfileOldPassword.Password, ProfileNewPassword.Password);

            if (success)
            {
                ProfilePasswordSuccess.Text = message;
                ProfilePasswordSuccess.Visibility = Visibility.Visible;
                ProfileOldPassword.Clear();
                ProfileNewPassword.Clear();
            }
            else
            {
                ProfilePasswordError.Text = message;
                ProfilePasswordError.Visibility = Visibility.Visible;
            }
        }

        private void TopUpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTopUpPanel();
        }

        private void ShowTopUpPanel()
        {
            UpdateOverlayBalance();
            TopUpOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            TopUpOverlay.BeginAnimation(OpacityProperty, fadeIn);

            var slideIn = new DoubleAnimation(450, 0, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            TopUpPanelTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        }

        private void HideTopUpPanel()
        {
            var slideOut = new DoubleAnimation(0, 450, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) => TopUpOverlay.Visibility = Visibility.Collapsed;

            TopUpPanelTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
            TopUpOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CloseTopUpOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            HideTopUpPanel();
        }

        private void CloseTopUpPanel_Click(object sender, RoutedEventArgs e)
        {
            HideTopUpPanel();
        }

        private void UpdateOverlayBalance()
        {
            if (AuthService.CurrentUser != null)
                OverlayBalanceText.Text = AuthService.CurrentUser.BalanceFormatted;
        }

        private void OverlayCardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Replace(" ", "").Replace("-", "");
            if (text.Length > 16) text = text[..16];

            var formatted = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 4 == 0) formatted += " ";
                formatted += text[i];
            }

            if (tb.Text != formatted)
            {
                tb.Text = formatted;
                tb.CaretIndex = formatted.Length;
            }
        }

        private void OverlayQuickAmount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
                OverlayAmount.Text = tag;
        }

        private async void OverlayTopUp_Click(object sender, RoutedEventArgs e)
        {
            OverlayError.Visibility = Visibility.Collapsed;

            var cardNumber = OverlayCardNumber.Text.Replace(" ", "");
            if (cardNumber.Length < 16)
            {
                ShowOverlayError("Введите полный номер карты (16 цифр).");
                return;
            }

            var expiry = OverlayExpiry.Text.Trim();
            if (expiry.Length < 4)
            {
                ShowOverlayError("Введите срок действия карты (ММ/ГГ).");
                return;
            }

            var cvv = OverlayCvv.Password.Trim();
            if (cvv.Length < 3)
            {
                ShowOverlayError("Введите CVV-код (3 цифры).");
                return;
            }

            if (!decimal.TryParse(OverlayAmount.Text.Replace(" ", ""), out var amount) || amount <= 0)
            {
                ShowOverlayError("Введите корректную сумму пополнения.");
                return;
            }

            if (amount > 100_000_000)
            {
                ShowOverlayError("Максимальная сумма: 100 000 000 ₽.");
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == AuthService.CurrentUser!.Id);
                if (user == null) return;

                user.Balance += amount;
                await context.SaveChangesAsync();
                AuthService.CurrentUser!.Balance = user.Balance;

                _viewModel.UpdateBalance();
                UpdateOverlayBalance();

                OverlayCardNumber.Text = "";
                OverlayExpiry.Text = "";
                OverlayCvv.Clear();
                OverlayAmount.Text = "";

                MessageBox.Show(
                    $"Баланс пополнен!\n\nСумма: {amount:N0} ₽\nНовый баланс: {user.BalanceFormatted}",
                    "SELCER ROYALITY PRM",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                HideTopUpPanel();
            }
            catch (Exception ex)
            {
                ShowOverlayError($"Ошибка: {ex.Message}");
            }
        }

        private void ShowOverlayError(string message)
        {
            OverlayError.Text = message;
            OverlayError.Visibility = Visibility.Visible;
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
