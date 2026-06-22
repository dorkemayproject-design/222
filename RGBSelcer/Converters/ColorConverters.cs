using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RGBSelcer.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrEmpty(s))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PriceFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price)
                return $"{price:N0} ₽";
            return "0 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName)
            {
                return colorName.ToLower() switch
                {
                    "красный" or "red" => new SolidColorBrush(Color.FromRgb(200, 30, 30)),
                    "синий" or "blue" => new SolidColorBrush(Color.FromRgb(30, 60, 200)),
                    "чёрный" or "black" => new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    "белый" or "white" => new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    "серебристый" or "silver" => new SolidColorBrush(Color.FromRgb(192, 192, 200)),
                    "серый" or "gray" => new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                    "зелёный" or "green" => new SolidColorBrush(Color.FromRgb(30, 150, 30)),
                    "жёлтый" or "yellow" => new SolidColorBrush(Color.FromRgb(230, 200, 30)),
                    "оранжевый" or "orange" => new SolidColorBrush(Color.FromRgb(230, 130, 30)),
                    "коричневый" or "brown" => new SolidColorBrush(Color.FromRgb(139, 90, 43)),
                    _ => new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BodyTypeToCarTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string bodyType)
            {
                return bodyType.ToLower() switch
                {
                    "седан" => "sedan",
                    "внедорожник" or "кроссовер" => "suv",
                    "спорткар" or "суперкар" or "гиперкар" or "родстер" => "sport",
                    "мотоцикл" => "motorcycle",
                    "пикап" or "грузовик" => "truck",
                    "купе" => "coupe",
                    "хэтчбек" or "компакт" => "hatchback",
                    _ => "sedan",
                };
            }
            return "sedan";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName)
            {
                return colorName.ToLower() switch
                {
                    "красный" or "red" => Color.FromRgb(200, 30, 30),
                    "синий" or "blue" => Color.FromRgb(30, 60, 200),
                    "чёрный" or "black" => Color.FromRgb(30, 30, 30),
                    "белый" or "white" => Color.FromRgb(240, 240, 240),
                    "серебристый" or "silver" => Color.FromRgb(192, 192, 200),
                    "серый" or "gray" => Color.FromRgb(128, 128, 128),
                    "зелёный" or "green" => Color.FromRgb(30, 150, 30),
                    "жёлтый" or "yellow" => Color.FromRgb(230, 200, 30),
                    "оранжевый" or "orange" => Color.FromRgb(230, 130, 30),
                    "коричневый" or "brown" => Color.FromRgb(139, 90, 43),
                    _ => Color.FromRgb(100, 100, 100),
                };
            }
            return Color.FromRgb(100, 100, 100);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
