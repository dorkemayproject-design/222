using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RGBSelcer.Models;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly CarService _carService = new();
        private readonly FileService _fileService = new();
        private List<Car> _allCars = new();

        private ObservableCollection<Car> _cars = new();
        public ObservableCollection<Car> Cars
        {
            get => _cars;
            set => SetProperty(ref _cars, value);
        }

        private Car? _selectedCar;
        public Car? SelectedCar
        {
            get => _selectedCar;
            set => SetProperty(ref _selectedCar, value);
        }

        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        private string _selectedCategory = "Все";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    ApplyFilters();
            }
        }

        private string _selectedBodyType = "Все";
        public string SelectedBodyType
        {
            get => _selectedBodyType;
            set
            {
                if (SetProperty(ref _selectedBodyType, value))
                    ApplyFilters();
            }
        }

        private string _selectedBrand = "Все";
        public string SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (SetProperty(ref _selectedBrand, value))
                    ApplyFilters();
            }
        }

        private string _balanceText = "0 ₽";
        public string BalanceText
        {
            get => _balanceText;
            set => SetProperty(ref _balanceText, value);
        }

        private ObservableCollection<string> _brands = new() { "Все" };
        public ObservableCollection<string> Brands
        {
            get => _brands;
            set => SetProperty(ref _brands, value);
        }

        public void UpdateBalance()
        {
            if (AuthService.CurrentUser != null)
                BalanceText = AuthService.CurrentUser.BalanceFormatted;
        }

        public AsyncRelayCommand LoadCarsCommand { get; }
        public AsyncRelayCommand ExportJsonCommand { get; }
        public AsyncRelayCommand ExportCsvCommand { get; }
        public AsyncRelayCommand ExportTxtCommand { get; }
        public AsyncRelayCommand DeleteFileCommand { get; }

        public MainViewModel()
        {
            LoadCarsCommand = new AsyncRelayCommand(LoadCarsAsync);
            ExportJsonCommand = new AsyncRelayCommand(ExportToJsonAsync);
            ExportCsvCommand = new AsyncRelayCommand(ExportToCsvAsync);
            ExportTxtCommand = new AsyncRelayCommand(ExportToTxtAsync);
            DeleteFileCommand = new AsyncRelayCommand(DeleteFileAsync);

            if (AuthService.CurrentUser != null)
                UserName = AuthService.CurrentUser.Login;
        }

        public async Task LoadCarsAsync()
        {
            IsLoading = true;
            StatusText = "Загрузка каталога...";

            try
            {
                _allCars = await _carService.GetAllCarsAsync();
                var brandList = new List<string> { "Все" };
                brandList.AddRange(_allCars.Select(c => c.Brand).Distinct().OrderBy(b => b));
                Brands = new ObservableCollection<string>(brandList);

                ApplyFilters();
                StatusText = $"Загружено автомобилей: {_allCars.Count}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
                StatusText = "Ошибка загрузки";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allCars.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    c.Brand.ToLower().Contains(search) ||
                    c.Model.ToLower().Contains(search) ||
                    c.Description.ToLower().Contains(search) ||
                    c.Country.ToLower().Contains(search));
            }

            if (SelectedCategory != "Все")
                filtered = filtered.Where(c => c.LicenseCategory == SelectedCategory);

            if (SelectedBodyType != "Все")
                filtered = filtered.Where(c => c.BodyType == SelectedBodyType);

            if (SelectedBrand != "Все")
                filtered = filtered.Where(c => c.Brand == SelectedBrand);

            Cars = new ObservableCollection<Car>(filtered);
        }

        private async Task ExportToJsonAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                FileName = "catalog.json",
                Title = "Экспорт каталога в JSON"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportCarsToJsonAsync(Cars.ToList(), dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в JSON...",
                    "Каталог экспортирован в JSON");
            }
        }

        private async Task ExportToCsvAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = "catalog.csv",
                Title = "Экспорт каталога в CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportCarsToCsvAsync(Cars.ToList(), dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в CSV...",
                    "Каталог экспортирован в CSV");
            }
        }

        private async Task ExportToTxtAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = "catalog.txt",
                Title = "Экспорт каталога в TXT"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportCarsToTxtAsync(Cars.ToList(), dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в TXT...",
                    "Каталог экспортирован в TXT");
            }
        }

        private async Task DeleteFileAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Все файлы (*.*)|*.*|JSON (*.json)|*.json|CSV (*.csv)|*.csv|TXT (*.txt)|*.txt",
                Title = "Выберите файл для удаления"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = MessageBox.Show(
                    $"Удалить файл \"{System.IO.Path.GetFileName(dialog.FileName)}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _fileService.DeleteFileAsync(dialog.FileName);
                        StatusText = $"Файл удалён: {System.IO.Path.GetFileName(dialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Ошибка удаления файла: {ex.Message}";
                    }
                }
            }
        }

        private async Task ExportWithProgressAsync(Func<Task> exportAction, string startMessage, string endMessage)
        {
            IsProgressVisible = true;
            ProgressValue = 0;
            StatusText = startMessage;
            ErrorMessage = string.Empty;

            try
            {
                await exportAction();
                StatusText = endMessage;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка экспорта: {ex.Message}";
                StatusText = "Ошибка экспорта";
            }
            finally
            {
                await Task.Delay(500);
                IsProgressVisible = false;
            }
        }
    }
}
