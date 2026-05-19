using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Services;

namespace RGBSelcer.Views
{
    public partial class TopUpWindow : Window
    {
        public bool TopUpSuccess { get; private set; }

        public TopUpWindow()
        {
            InitializeComponent();
            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay()
        {
            if (AuthService.CurrentUser != null)
                BalanceText.Text = AuthService.CurrentUser.BalanceFormatted;
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
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

        private void QuickAmount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                AmountBox.Text = tag;
            }
        }

        private async void TopUp_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var cardNumber = CardNumberBox.Text.Replace(" ", "");
            if (cardNumber.Length < 16)
            {
                ShowError("Введите полный номер карты (16 цифр).");
                return;
            }

            var expiry = ExpiryBox.Text.Trim();
            if (expiry.Length < 4)
            {
                ShowError("Введите срок действия карты (ММ/ГГ).");
                return;
            }

            var cvv = CvvBox.Password.Trim();
            if (cvv.Length < 3)
            {
                ShowError("Введите CVV-код (3 цифры).");
                return;
            }

            if (!decimal.TryParse(AmountBox.Text.Replace(" ", ""), out var amount) || amount <= 0)
            {
                ShowError("Введите корректную сумму пополнения.");
                return;
            }

            if (amount > 100_000_000)
            {
                ShowError("Максимальная сумма пополнения: 100 000 000 ₽.");
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
                TopUpSuccess = true;

                MessageBox.Show(
                    $"Баланс успешно пополнен!\n\nСумма: {amount:N0} ₽\nНовый баланс: {user.BalanceFormatted}",
                    "SELCER ROYALITY PRM",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
