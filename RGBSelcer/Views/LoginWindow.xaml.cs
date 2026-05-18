using System.Windows;
using System.Windows.Input;
using RGBSelcer.Services;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (LoginViewModel)DataContext;
            await vm.LoginCommand.ExecuteAsync(null);

            if (AuthService.CurrentUser != null)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
