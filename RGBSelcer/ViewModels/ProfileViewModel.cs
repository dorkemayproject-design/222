using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Models;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();
        private readonly CarService _carService = new();

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _registeredAt = string.Empty;
        public string RegisteredAt
        {
            get => _registeredAt;
            set => SetProperty(ref _registeredAt, value);
        }

        private string _oldPassword = string.Empty;
        public string OldPassword
        {
            get => _oldPassword;
            set => SetProperty(ref _oldPassword, value);
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private int _totalCars;
        public int TotalCars
        {
            get => _totalCars;
            set => SetProperty(ref _totalCars, value);
        }

        private int _purchaseCount;
        public int PurchaseCount
        {
            get => _purchaseCount;
            set => SetProperty(ref _purchaseCount, value);
        }

        private string _totalSpent = "0 ₽";
        public string TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        private ObservableCollection<Purchase> _purchases = new();
        public ObservableCollection<Purchase> Purchases
        {
            get => _purchases;
            set => SetProperty(ref _purchases, value);
        }

        public AsyncRelayCommand ChangePasswordCommand { get; }
        public AsyncRelayCommand LoadDataCommand { get; }

        public ProfileViewModel()
        {
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            if (AuthService.CurrentUser != null)
            {
                Login = AuthService.CurrentUser.Login;
                RegisteredAt = AuthService.CurrentUser.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            }
        }

        public async Task LoadDataAsync()
        {
            if (AuthService.CurrentUser == null) return;

            try
            {
                var (carCount, purchaseCount, totalSpent) =
                    await _carService.GetUserStatsAsync(AuthService.CurrentUser.Id);
                TotalCars = carCount;
                PurchaseCount = purchaseCount;
                TotalSpent = $"{totalSpent:N0} ₽";

                var purchases = await _carService.GetUserPurchasesAsync(AuthService.CurrentUser.Id);
                Purchases = new ObservableCollection<Purchase>(purchases);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private async Task ChangePasswordAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var (success, message) = await _authService.ChangePasswordAsync(OldPassword, NewPassword);

            if (success)
            {
                SuccessMessage = message;
                OldPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                ErrorMessage = message;
            }
        }
    }
}
