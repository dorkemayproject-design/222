using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class LoginViewModel : BaseViewModel
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

        public AsyncRelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;

            var (success, message) = await _authService.LoginAsync(Login, Password);

            IsLoading = false;

            if (success)
                SuccessMessage = message;
            else
                ErrorMessage = message;
        }
    }
}
