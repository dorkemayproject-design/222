using CommunityToolkit.Mvvm.ComponentModel;

namespace RGBSelcer.ViewModels
{
    public class ColorPickerViewModel : BaseViewModel
    {
        private string _colorName = string.Empty;
        public string ColorName
        {
            get => _colorName;
            set => SetProperty(ref _colorName, value);
        }

        private byte _red;
        public byte Red
        {
            get => _red;
            set
            {
                if (SetProperty(ref _red, value))
                    UpdateHexCode();
            }
        }

        private byte _green;
        public byte Green
        {
            get => _green;
            set
            {
                if (SetProperty(ref _green, value))
                    UpdateHexCode();
            }
        }

        private byte _blue;
        public byte Blue
        {
            get => _blue;
            set
            {
                if (SetProperty(ref _blue, value))
                    UpdateHexCode();
            }
        }

        private double _redSlider;
        public double RedSlider
        {
            get => _redSlider;
            set
            {
                if (SetProperty(ref _redSlider, value))
                    Red = (byte)value;
            }
        }

        private double _greenSlider;
        public double GreenSlider
        {
            get => _greenSlider;
            set
            {
                if (SetProperty(ref _greenSlider, value))
                    Green = (byte)value;
            }
        }

        private double _blueSlider;
        public double BlueSlider
        {
            get => _blueSlider;
            set
            {
                if (SetProperty(ref _blueSlider, value))
                    Blue = (byte)value;
            }
        }

        private string _hexCode = "#000000";
        public string HexCode
        {
            get => _hexCode;
            set => SetProperty(ref _hexCode, value);
        }

        public bool IsConfirmed { get; set; }

        public ColorPickerViewModel()
        {
            Red = 128;
            Green = 128;
            Blue = 128;
            RedSlider = 128;
            GreenSlider = 128;
            BlueSlider = 128;
        }

        public void SetColor(byte r, byte g, byte b, string name)
        {
            ColorName = name;
            _red = r;
            _green = g;
            _blue = b;
            _redSlider = r;
            _greenSlider = g;
            _blueSlider = b;
            OnPropertyChanged(nameof(Red));
            OnPropertyChanged(nameof(Green));
            OnPropertyChanged(nameof(Blue));
            OnPropertyChanged(nameof(RedSlider));
            OnPropertyChanged(nameof(GreenSlider));
            OnPropertyChanged(nameof(BlueSlider));
            UpdateHexCode();
        }

        private void UpdateHexCode()
        {
            HexCode = $"#{Red:X2}{Green:X2}{Blue:X2}";
        }
    }
}
