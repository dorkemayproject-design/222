# RGB SELCER — Менеджер цветовых палитр

Десктопное WPF-приложение на C# для создания, управления и экспорта цветовых палитр с системой авторизации пользователей.

## Возможности

- **Система пользователей**: регистрация, авторизация, личный кабинет
- **Управление палитрами**: создание, редактирование, удаление цветовых палитр
- **RGB-палитра**: интерактивный выбор цвета через RGB-слайдеры
- **Работа с файлами**: экспорт/импорт палитр в форматах JSON, CSV, TXT
- **База данных**: SQLite для хранения пользователей и палитр
- **ProgressBar**: отображение прогресса при файловых операциях
- **Асинхронные операции**: все операции с БД и файлами выполняются асинхронно
- **Разграничение данных**: каждый пользователь работает только со своими палитрами
- **Хеширование паролей**: BCrypt для безопасного хранения паролей

## Технологии

- **Язык**: C# (.NET 8)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Архитектура**: MVVM (Model-View-ViewModel)
- **ORM**: Entity Framework Core
- **БД**: SQLite
- **Библиотеки**:
  - `CommunityToolkit.Mvvm` — реализация MVVM
  - `BCrypt.Net-Next` — хеширование паролей
  - `Newtonsoft.Json` — работа с JSON
  - `Microsoft.EntityFrameworkCore.Sqlite` — доступ к БД

## Структура проекта

```
RGBSelcer/
├── Models/            # Модели данных (User, ColorPalette, ColorItem)
├── Data/              # Контекст БД (AppDbContext)
├── Services/          # Бизнес-логика (AuthService, PaletteService, FileService)
├── ViewModels/        # ViewModel'ы для MVVM
├── Views/             # XAML-окна приложения
├── Converters/        # Конвертеры для привязки данных
└── Helpers/           # Вспомогательные классы
```

## Окна приложения

1. **LoginWindow** — Вход в систему
2. **RegisterWindow** — Регистрация нового пользователя
3. **MainWindow** — Главная панель с палитрами, экспортом/импортом
4. **PaletteEditorWindow** — Редактор палитры с таблицей цветов
5. **ColorPickerWindow** — Выбор цвета через RGB-слайдеры
6. **ProfileWindow** — Личный кабинет пользователя

## База данных

| Таблица | Назначение |
|---------|-----------|
| Users | Пользователи (Id, Login, PasswordHash, CreatedAt) |
| Palettes | Цветовые палитры (Id, UserId, Name, Description, CreatedAt, UpdatedAt) |
| Colors | Цвета в палитрах (Id, PaletteId, Name, Red, Green, Blue, HexCode, SortOrder) |

## Запуск

### Требования
- Windows 10/11
- .NET 8 SDK

### Сборка и запуск
```bash
dotnet restore
dotnet build
dotnet run --project RGBSelcer
```

## Автор

Учебная практика — РО 10.6
