using System.Windows;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class PaletteEditorWindow : Window
    {
        private readonly PaletteEditorViewModel _viewModel;

        public PaletteEditorWindow(int paletteId)
        {
            InitializeComponent();

            _viewModel = new PaletteEditorViewModel();
            _viewModel.SetPaletteId(paletteId);
            DataContext = _viewModel;

            Loaded += PaletteEditorWindow_Loaded;
        }

        private async void PaletteEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadPaletteAsync();
        }

        private async void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new ColorPickerWindow();
            if (picker.ShowDialog() == true)
            {
                var vm = picker.ViewModel;
                await _viewModel.AddColorAsync(vm.ColorName, vm.Red, vm.Green, vm.Blue);
            }
        }

        private async void EditColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedColor == null)
            {
                MessageBox.Show("Выберите цвет для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var color = _viewModel.SelectedColor;
            var picker = new ColorPickerWindow(color.Red, color.Green, color.Blue, color.Name);
            if (picker.ShowDialog() == true)
            {
                var vm = picker.ViewModel;
                await _viewModel.UpdateColorAsync(color.Id, vm.ColorName, vm.Red, vm.Green, vm.Blue);
            }
        }
    }
}
