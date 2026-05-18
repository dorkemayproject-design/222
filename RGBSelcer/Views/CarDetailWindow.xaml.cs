using System.Windows;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class CarDetailWindow : Window
    {
        private readonly CarDetailViewModel _viewModel;
        private readonly int _carId;

        public CarDetailWindow(int carId)
        {
            InitializeComponent();
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
            if (_viewModel.Car == null) return;

            var result = MessageBox.Show(
                $"Подтвердите покупку:\n\n{_viewModel.Car.Brand} {_viewModel.Car.Model}\n" +
                $"Цена: {_viewModel.Car.PriceFormatted}\n\nОформить?",
                "SELCER ROYALITY PRM — Покупка",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _viewModel.PurchaseCommand.ExecuteAsync(null);

                if (_viewModel.IsPurchased)
                {
                    MessageBox.Show(_viewModel.SuccessMessage, "Поздравляем!",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                {
                    MessageBox.Show(_viewModel.ErrorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
