using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public AsyncRelayCommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        }

        private async System.Threading.Tasks.Task RegisterAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return;
            }

            var (success, message) = await _authService.RegisterAsync(Login, Password);

            if (success)
                SuccessMessage = message;
            else
                ErrorMessage = message;
        }
    }
}
