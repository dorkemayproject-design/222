using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RGBSelcer.Views
{
    public partial class PurchaseTimerWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private int _remainingSeconds = 180;
        private readonly int _totalSeconds = 180;

        public bool PurchaseCompleted { get; private set; }
        public string CarName { get; set; } = string.Empty;
        public decimal CarPrice { get; set; }

        public PurchaseTimerWindow(string carName, decimal carPrice)
        {
            InitializeComponent();
            CarName = carName;
            CarPrice = carPrice;

            CarNameText.Text = carName;
            CarPriceText.Text = $"{carPrice:N0} ₽";

            DrawClockTicks();
            DrawProgressArc();
            UpdateTimerDisplay();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            Loaded += (_, _) => _timer.Start();
            Closing += (_, e) =>
            {
                if (!PurchaseCompleted && _remainingSeconds > 0)
                {
                    var result = MessageBox.Show(
                        "Отменить оформление покупки?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                        e.Cancel = true;
                }
                _timer.Stop();
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                _timer.Stop();
                PurchaseCompleted = true;
                TimerText.Text = "00:00";
                TimerLabel.Text = "Готово!";
                StatusText.Text = "Покупка оформлена успешно!";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C));

                MessageBox.Show(
                    $"Покупка оформлена!\n\n{CarName}\nСумма: {CarPrice:N0} ₽\n\nСредства списаны со счёта.",
                    "SELCER ROYALITY PRM — Покупка завершена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
                return;
            }

            UpdateTimerDisplay();
            DrawProgressArc();
        }

        private void UpdateTimerDisplay()
        {
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            TimerText.Text = $"{minutes:D2}:{seconds:D2}";
            TimerProgress.Value = _totalSeconds - _remainingSeconds;

            if (_remainingSeconds <= 30)
            {
                StatusText.Text = "Завершение оформления...";
                TimerText.Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C));
            }
            else if (_remainingSeconds <= 60)
            {
                StatusText.Text = "Проверка данных...";
            }
            else if (_remainingSeconds <= 120)
            {
                StatusText.Text = "Обработка платежа...";
            }
        }

        private void DrawClockTicks()
        {
            double cx = 110, cy = 110, r = 100;

            for (int i = 0; i < 60; i++)
            {
                double angle = (i * 6 - 90) * Math.PI / 180.0;
                double innerR = i % 5 == 0 ? r - 12 : r - 6;
                double thickness = i % 5 == 0 ? 2.0 : 1.0;

                var line = new Line
                {
                    X1 = cx + innerR * Math.Cos(angle),
                    Y1 = cy + innerR * Math.Sin(angle),
                    X2 = cx + r * Math.Cos(angle),
                    Y2 = cy + r * Math.Sin(angle),
                    Stroke = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                    StrokeThickness = thickness
                };
                TickCanvas.Children.Add(line);
            }
        }

        private void DrawProgressArc()
        {
            TimerCanvas.Children.Clear();

            double progress = (_totalSeconds - _remainingSeconds) / (double)_totalSeconds;
            if (progress <= 0) return;

            double cx = 110, cy = 110, r = 102;
            double startAngle = -90;
            double sweepAngle = progress * 360;

            double startRad = startAngle * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180.0;

            double x1 = cx + r * Math.Cos(startRad);
            double y1 = cy + r * Math.Sin(startRad);
            double x2 = cx + r * Math.Cos(endRad);
            double y2 = cy + r * Math.Sin(endRad);

            var path = new Path
            {
                StrokeThickness = 8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0x6C, 0x2B, 0xD9), 0));
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0x9B, 0x30, 0xFF), 1));
            path.Stroke = gradient;

            var pathFigure = new PathFigure { StartPoint = new Point(x1, y1) };
            var arcSegment = new ArcSegment
            {
                Point = new Point(x2, y2),
                Size = new Size(r, r),
                IsLargeArc = sweepAngle > 180,
                SweepDirection = SweepDirection.Clockwise
            };
            pathFigure.Segments.Add(arcSegment);

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            path.Data = pathGeometry;

            TimerCanvas.Children.Add(path);
        }
    }
}
