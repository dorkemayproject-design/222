using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RGBSelcer.Services;

namespace RGBSelcer.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();
        private readonly PaletteService _paletteService = new();

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _createdAt = string.Empty;
        public string CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private int _paletteCount;
        public int PaletteCount
        {
            get => _paletteCount;
            set => SetProperty(ref _paletteCount, value);
        }

        private int _colorCount;
        public int ColorCount
        {
            get => _colorCount;
            set => SetProperty(ref _colorCount, value);
        }

        private string _oldPassword = string.Empty;
        public string OldPassword
        {
            get => _oldPassword;
            set => SetProperty(ref _oldPassword, value);
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public AsyncRelayCommand LoadProfileCommand { get; }
        public AsyncRelayCommand ChangePasswordCommand { get; }

        public ProfileViewModel()
        {
            LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
        }

        public async Task LoadProfileAsync()
        {
            if (AuthService.CurrentUser == null) return;

            Login = AuthService.CurrentUser.Login;
            CreatedAt = AuthService.CurrentUser.CreatedAt.ToString("dd.MM.yyyy HH:mm");

            try
            {
                var (paletteCount, colorCount) = await _paletteService.GetUserStatsAsync(AuthService.CurrentUser.Id);
                PaletteCount = paletteCount;
                ColorCount = colorCount;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки статистики: {ex.Message}";
            }
        }

        private async Task ChangePasswordAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var (success, message) = await _authService.ChangePasswordAsync(OldPassword, NewPassword);

            if (success)
            {
                SuccessMessage = message;
                OldPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                ErrorMessage = message;
            }
        }
    }
}
