using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RGBSelcer.Data;
using RGBSelcer.Models;

namespace RGBSelcer.Services
{
    public class AuthService
    {
        private static User? _currentUser;

        public static User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                return (false, "Логин не может быть пустым.");

            if (login.Length < 3)
                return (false, "Логин должен содержать минимум 3 символа.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Пароль не может быть пустым.");

            if (password.Length < 4)
                return (false, "Пароль должен содержать минимум 4 символа.");

            try
            {
                using var context = new AppDbContext();
                var exists = await context.Users.AnyAsync(u => u.Login == login);
                if (exists)
                    return (false, "Пользователь с таким логином уже существует.");

                var user = new User
                {
                    Login = login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.Now
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                return (true, "Регистрация прошла успешно!");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> LoginAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return (false, "Введите логин и пароль.");

            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Login == login);

                if (user == null)
                    return (false, "Пользователь не найден.");

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return (false, "Неверный пароль.");

                CurrentUser = user;
                return (true, "Вход выполнен успешно!");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при входе: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            if (CurrentUser == null)
                return (false, "Пользователь не авторизован.");

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                return (false, "Заполните оба поля.");

            if (newPassword.Length < 4)
                return (false, "Новый пароль должен содержать минимум 4 символа.");

            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FindAsync(CurrentUser.Id);
                if (user == null)
                    return (false, "Пользователь не найден.");

                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                    return (false, "Неверный текущий пароль.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await context.SaveChangesAsync();

                CurrentUser = user;
                return (true, "Пароль успешно изменен!");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}");
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
