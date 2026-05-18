using System;
using System.Collections.ObjectModel;
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
        private readonly PaletteService _paletteService = new();
        private readonly FileService _fileService = new();

        private ObservableCollection<ColorPalette> _palettes = new();
        public ObservableCollection<ColorPalette> Palettes
        {
            get => _palettes;
            set => SetProperty(ref _palettes, value);
        }

        private ColorPalette? _selectedPalette;
        public ColorPalette? SelectedPalette
        {
            get => _selectedPalette;
            set => SetProperty(ref _selectedPalette, value);
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

        public AsyncRelayCommand LoadPalettesCommand { get; }
        public AsyncRelayCommand CreatePaletteCommand { get; }
        public AsyncRelayCommand DeletePaletteCommand { get; }
        public AsyncRelayCommand ExportJsonCommand { get; }
        public AsyncRelayCommand ExportCsvCommand { get; }
        public AsyncRelayCommand ExportTxtCommand { get; }
        public AsyncRelayCommand ImportJsonCommand { get; }
        public AsyncRelayCommand ImportCsvCommand { get; }

        public MainViewModel()
        {
            LoadPalettesCommand = new AsyncRelayCommand(LoadPalettesAsync);
            CreatePaletteCommand = new AsyncRelayCommand(CreatePaletteAsync);
            DeletePaletteCommand = new AsyncRelayCommand(DeletePaletteAsync);
            ExportJsonCommand = new AsyncRelayCommand(ExportToJsonAsync);
            ExportCsvCommand = new AsyncRelayCommand(ExportToCsvAsync);
            ExportTxtCommand = new AsyncRelayCommand(ExportToTxtAsync);
            ImportJsonCommand = new AsyncRelayCommand(ImportFromJsonAsync);
            ImportCsvCommand = new AsyncRelayCommand(ImportFromCsvAsync);

            if (AuthService.CurrentUser != null)
                UserName = AuthService.CurrentUser.Login;
        }

        public async Task LoadPalettesAsync()
        {
            if (AuthService.CurrentUser == null) return;

            IsLoading = true;
            StatusText = "Загрузка палитр...";

            try
            {
                var palettes = await _paletteService.GetUserPalettesAsync(AuthService.CurrentUser.Id);
                Palettes = new ObservableCollection<ColorPalette>(palettes);
                StatusText = $"Загружено палитр: {palettes.Count}";
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

        private async Task CreatePaletteAsync()
        {
            if (AuthService.CurrentUser == null) return;

            var name = $"Новая палитра {DateTime.Now:HH:mm:ss}";
            try
            {
                await _paletteService.CreatePaletteAsync(AuthService.CurrentUser.Id, name, "");
                await LoadPalettesAsync();
                StatusText = "Палитра создана";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания: {ex.Message}";
            }
        }

        private async Task DeletePaletteAsync()
        {
            if (SelectedPalette == null)
            {
                ErrorMessage = "Выберите палитру для удаления.";
                return;
            }

            var result = MessageBox.Show(
                $"Удалить палитру \"{SelectedPalette.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _paletteService.DeletePaletteAsync(SelectedPalette.Id);
                    await LoadPalettesAsync();
                    StatusText = "Палитра удалена";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Ошибка удаления: {ex.Message}";
                }
            }
        }

        private async Task ExportToJsonAsync()
        {
            if (SelectedPalette == null)
            {
                ErrorMessage = "Выберите палитру для экспорта.";
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                FileName = $"{SelectedPalette.Name}.json",
                Title = "Экспорт палитры в JSON"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportPaletteToJsonAsync(SelectedPalette, dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в JSON...",
                    "Палитра экспортирована в JSON");
            }
        }

        private async Task ExportToCsvAsync()
        {
            if (SelectedPalette == null)
            {
                ErrorMessage = "Выберите палитру для экспорта.";
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"{SelectedPalette.Name}.csv",
                Title = "Экспорт палитры в CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportPaletteToCsvAsync(SelectedPalette, dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в CSV...",
                    "Палитра экспортирована в CSV");
            }
        }

        private async Task ExportToTxtAsync()
        {
            if (SelectedPalette == null)
            {
                ErrorMessage = "Выберите палитру для экспорта.";
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = $"{SelectedPalette.Name}.txt",
                Title = "Экспорт палитры в TXT"
            };

            if (dialog.ShowDialog() == true)
            {
                await ExportWithProgressAsync(
                    () => _fileService.ExportPaletteToTxtAsync(SelectedPalette, dialog.FileName,
                        new Progress<int>(p => ProgressValue = p)),
                    "Экспорт в TXT...",
                    "Палитра экспортирована в TXT");
            }
        }

        private async Task ImportFromJsonAsync()
        {
            if (AuthService.CurrentUser == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                Title = "Импорт палитры из JSON"
            };

            if (dialog.ShowDialog() == true)
            {
                await ImportWithProgressAsync(dialog.FileName, true);
            }
        }

        private async Task ImportFromCsvAsync()
        {
            if (AuthService.CurrentUser == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                Title = "Импорт палитры из CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                await ImportWithProgressAsync(dialog.FileName, false);
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

        private async Task ImportWithProgressAsync(string filePath, bool isJson)
        {
            if (AuthService.CurrentUser == null) return;

            IsProgressVisible = true;
            ProgressValue = 0;
            StatusText = "Импорт палитры...";
            ErrorMessage = string.Empty;

            try
            {
                var progress = new Progress<int>(p => ProgressValue = p);

                FileService.PaletteExportData? data;
                if (isJson)
                    data = await _fileService.ImportPaletteFromJsonAsync(filePath, progress);
                else
                    data = await _fileService.ImportPaletteFromCsvAsync(filePath, progress);

                if (data == null)
                {
                    ErrorMessage = "Не удалось прочитать файл.";
                    return;
                }

                var palette = await _paletteService.CreatePaletteAsync(
                    AuthService.CurrentUser.Id,
                    data.Name,
                    data.Description);

                foreach (var color in data.Colors)
                {
                    await _paletteService.AddColorAsync(
                        palette.Id, color.Name, color.Red, color.Green, color.Blue);
                }

                await LoadPalettesAsync();
                StatusText = $"Импортирована палитра \"{data.Name}\" ({data.Colors.Count} цветов)";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка импорта: {ex.Message}";
                StatusText = "Ошибка импорта";
            }
            finally
            {
                await Task.Delay(500);
                IsProgressVisible = false;
            }
        }
    }
}
