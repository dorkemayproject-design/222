using System.Windows;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (RegisterViewModel)DataContext;
            await vm.RegisterCommand.ExecuteAsync(null);

            if (!string.IsNullOrEmpty(vm.SuccessMessage))
            {
                MessageBox.Show(vm.SuccessMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
