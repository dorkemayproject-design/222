using System.Windows;
using System.Windows.Controls;

namespace RGBSelcer.Helpers
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        private static bool _updating;

        public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);
        public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);

        public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                else
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_updating) return;
            if (sender is PasswordBox passwordBox)
            {
                _updating = true;
                SetBoundPassword(passwordBox, passwordBox.Password);
                _updating = false;
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (_updating) return;
            if (dp is PasswordBox passwordBox)
            {
                _updating = true;
                passwordBox.Password = (string)e.NewValue;
                _updating = false;
            }
        }
    }
}
