using System.Windows;
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
    }
}
