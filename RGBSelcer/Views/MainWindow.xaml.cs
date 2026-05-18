using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                _viewModel.UserName = AuthService.CurrentUser.Login;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadPalettesAsync();
        }

        private void OpenPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedPalette();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedPalette();
        }

        private void OpenSelectedPalette()
        {
            if (_viewModel.SelectedPalette == null)
            {
                MessageBox.Show("Выберите палитру для открытия.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editor = new PaletteEditorWindow(_viewModel.SelectedPalette.Id);
            editor.ShowDialog();

            _ = _viewModel.LoadPalettesAsync();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow();
            profileWindow.ShowDialog();
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
