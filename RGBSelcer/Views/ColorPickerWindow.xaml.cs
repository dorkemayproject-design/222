using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerViewModel ViewModel { get; }

        public ColorPickerWindow()
        {
            InitializeComponent();
            ViewModel = new ColorPickerViewModel();
            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdatePreviewColor();
        }

        public ColorPickerWindow(byte r, byte g, byte b, string name) : this()
        {
            ViewModel.SetColor(r, g, b, name);
            UpdatePreviewColor();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColorPickerViewModel.Red) ||
                e.PropertyName == nameof(ColorPickerViewModel.Green) ||
                e.PropertyName == nameof(ColorPickerViewModel.Blue))
            {
                UpdatePreviewColor();
            }
        }

        private void UpdatePreviewColor()
        {
            PreviewBrush.Color = Color.FromRgb(ViewModel.Red, ViewModel.Green, ViewModel.Blue);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ViewModel.ColorName))
            {
                MessageBox.Show("Введите название цвета.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ViewModel.IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
