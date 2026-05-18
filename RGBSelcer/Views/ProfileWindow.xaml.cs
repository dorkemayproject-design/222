using System.Windows;
using RGBSelcer.ViewModels;

namespace RGBSelcer.Views
{
    public partial class ProfileWindow : Window
    {
        private readonly ProfileViewModel _viewModel;

        public ProfileWindow()
        {
            InitializeComponent();
            _viewModel = (ProfileViewModel)DataContext;
            Loaded += ProfileWindow_Loaded;
        }

        private async void ProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
