using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Models;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class CarDetailViewModel : BaseViewModel
    {
        private readonly CarService _carService = new();

        private Car? _car;
        public Car? Car
        {
            get => _car;
            set => SetProperty(ref _car, value);
        }

        private bool _isPurchased;
        public bool IsPurchased
        {
            get => _isPurchased;
            set => SetProperty(ref _isPurchased, value);
        }

        public AsyncRelayCommand PurchaseCommand { get; }

        public CarDetailViewModel()
        {
            PurchaseCommand = new AsyncRelayCommand(PurchaseAsync);
        }

        public async Task LoadCarAsync(int carId)
        {
            IsLoading = true;
            try
            {
                Car = await _carService.GetCarByIdAsync(carId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PurchaseAsync()
        {
            if (Car == null || AuthService.CurrentUser == null) return;

            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                await _carService.PurchaseCarAsync(AuthService.CurrentUser.Id, Car.Id);
                IsPurchased = true;
                SuccessMessage = $"Поздравляем! Вы приобрели {Car.Brand} {Car.Model}!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка покупки: {ex.Message}";
            }
        }
    }
}
