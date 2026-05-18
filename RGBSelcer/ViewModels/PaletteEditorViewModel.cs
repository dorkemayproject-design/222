using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Models;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class PaletteEditorViewModel : BaseViewModel
    {
        private readonly PaletteService _paletteService = new();

        private int _paletteId;
        public int PaletteId
        {
            get => _paletteId;
            set => SetProperty(ref _paletteId, value);
        }

        private string _paletteName = string.Empty;
        public string PaletteName
        {
            get => _paletteName;
            set => SetProperty(ref _paletteName, value);
        }

        private string _paletteDescription = string.Empty;
        public string PaletteDescription
        {
            get => _paletteDescription;
            set => SetProperty(ref _paletteDescription, value);
        }

        private ObservableCollection<ColorItem> _colors = new();
        public ObservableCollection<ColorItem> Colors
        {
            get => _colors;
            set => SetProperty(ref _colors, value);
        }

        private ColorItem? _selectedColor;
        public ColorItem? SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        public AsyncRelayCommand LoadPaletteCommand { get; }
        public AsyncRelayCommand SavePaletteCommand { get; }
        public AsyncRelayCommand DeleteColorCommand { get; }

        public PaletteEditorViewModel()
        {
            LoadPaletteCommand = new AsyncRelayCommand(LoadPaletteAsync);
            SavePaletteCommand = new AsyncRelayCommand(SavePaletteAsync);
            DeleteColorCommand = new AsyncRelayCommand(DeleteColorAsync);
        }

        public void SetPaletteId(int id)
        {
            PaletteId = id;
        }

        public async Task LoadPaletteAsync()
        {
            if (PaletteId <= 0) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var palette = await _paletteService.GetPaletteByIdAsync(PaletteId);
                if (palette != null)
                {
                    PaletteName = palette.Name;
                    PaletteDescription = palette.Description;
                    Colors = new ObservableCollection<ColorItem>(palette.Colors);
                }
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

        private async Task SavePaletteAsync()
        {
            if (string.IsNullOrWhiteSpace(PaletteName))
            {
                ErrorMessage = "Введите название палитры.";
                return;
            }

            try
            {
                await _paletteService.UpdatePaletteAsync(PaletteId, PaletteName, PaletteDescription);
                SuccessMessage = "Палитра сохранена!";
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения: {ex.Message}";
            }
        }

        public async Task AddColorAsync(string name, byte r, byte g, byte b)
        {
            try
            {
                await _paletteService.AddColorAsync(PaletteId, name, r, g, b);
                await LoadPaletteAsync();
                SuccessMessage = "Цвет добавлен!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка добавления цвета: {ex.Message}";
            }
        }

        public async Task UpdateColorAsync(int colorId, string name, byte r, byte g, byte b)
        {
            try
            {
                await _paletteService.UpdateColorAsync(colorId, name, r, g, b);
                await LoadPaletteAsync();
                SuccessMessage = "Цвет обновлён!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления цвета: {ex.Message}";
            }
        }

        private async Task DeleteColorAsync()
        {
            if (SelectedColor == null)
            {
                ErrorMessage = "Выберите цвет для удаления.";
                return;
            }

            try
            {
                await _paletteService.DeleteColorAsync(SelectedColor.Id);
                await LoadPaletteAsync();
                SuccessMessage = "Цвет удален!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления: {ex.Message}";
            }
        }
    }
}
