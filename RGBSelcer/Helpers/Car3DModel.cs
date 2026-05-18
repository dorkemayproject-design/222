using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace RGBSelcer.Helpers
{
    public class Car3DModel : Viewport3D
    {
        public static readonly DependencyProperty CarTypeProperty =
            DependencyProperty.Register("CarType", typeof(string), typeof(Car3DModel),
                new PropertyMetadata("sedan", OnCarTypeChanged));

        public static readonly DependencyProperty CarColorProperty =
            DependencyProperty.Register("CarColor", typeof(Color), typeof(Car3DModel),
                new PropertyMetadata(Color.FromRgb(200, 30, 30), OnCarTypeChanged));

        public string CarType
        {
            get => (string)GetValue(CarTypeProperty);
            set => SetValue(CarTypeProperty, value);
        }

        public Color CarColor
        {
            get => (Color)GetValue(CarColorProperty);
            set => SetValue(CarColorProperty, value);
        }

        private AxisAngleRotation3D? _rotation;

        public Car3DModel()
        {
            Loaded += OnLoaded;
        }

        private static void OnCarTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Car3DModel model && model.IsLoaded)
            {
                model.Children.Clear();
                model.BuildScene();
                model.StartRotation();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildScene();
            StartRotation();
        }

        private void BuildScene()
        {
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(5, 3, 6),
                LookDirection = new Vector3D(-5, -3, -6),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45
            };
            Camera = camera;

            var modelGroup = new Model3DGroup();

            switch (CarType?.ToLower())
            {
                case "suv":
                    BuildSUV(modelGroup);
                    break;
                case "sport":
                    BuildSportCar(modelGroup);
                    break;
                case "motorcycle":
                    BuildMotorcycle(modelGroup);
                    break;
                case "truck":
                    BuildTruck(modelGroup);
                    break;
                case "coupe":
                    BuildCoupe(modelGroup);
                    break;
                case "hatchback":
                    BuildHatchback(modelGroup);
                    break;
                default:
                    BuildSedan(modelGroup);
                    break;
            }

            modelGroup.Children.Add(new AmbientLight(Color.FromRgb(100, 100, 100)));
            modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));
            modelGroup.Children.Add(new DirectionalLight(Color.FromRgb(80, 80, 120), new Vector3D(1, 0.5, 0.5)));

            _rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            var rotateTransform = new RotateTransform3D(_rotation);
            modelGroup.Transform = rotateTransform;

            var modelVisual = new ModelVisual3D { Content = modelGroup };
            Children.Add(modelVisual);
        }

        private void BuildSedan(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -1.8, 0.3, -0.7, 3.6, 0.6, 1.4, bodyColor);
            AddBox(group, -0.8, 0.9, -0.65, 2.0, 0.55, 1.3, bodyColor);
            AddBox(group, -0.6, 0.9, -0.55, 1.6, 0.5, 1.1, Color.FromRgb(180, 220, 255));
            AddWheel(group, -1.1, 0.15, -0.8);
            AddWheel(group, -1.1, 0.15, 0.8);
            AddWheel(group, 1.1, 0.15, -0.8);
            AddWheel(group, 1.1, 0.15, 0.8);
            AddBox(group, 1.6, 0.35, -0.5, 0.25, 0.2, 1.0, Color.FromRgb(255, 50, 50));
            AddBox(group, -1.8, 0.35, -0.5, 0.15, 0.2, 1.0, Color.FromRgb(255, 255, 200));
        }

        private void BuildSUV(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -2.0, 0.5, -0.8, 4.0, 0.7, 1.6, bodyColor);
            AddBox(group, -1.2, 1.2, -0.75, 2.8, 0.7, 1.5, bodyColor);
            AddBox(group, -1.0, 1.2, -0.65, 2.4, 0.6, 1.3, Color.FromRgb(180, 220, 255));
            AddWheel(group, -1.3, 0.25, -0.9);
            AddWheel(group, -1.3, 0.25, 0.9);
            AddWheel(group, 1.3, 0.25, -0.9);
            AddWheel(group, 1.3, 0.25, 0.9);
            AddBox(group, 1.8, 0.55, -0.55, 0.25, 0.25, 1.1, Color.FromRgb(255, 50, 50));
            AddBox(group, -2.0, 0.55, -0.55, 0.15, 0.25, 1.1, Color.FromRgb(255, 255, 200));
            AddBox(group, -2.0, 0.1, -0.9, 4.0, 0.15, 1.8, Color.FromRgb(60, 60, 60));
        }

        private void BuildSportCar(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -1.9, 0.2, -0.75, 3.8, 0.45, 1.5, bodyColor);
            AddBox(group, -0.5, 0.65, -0.6, 1.5, 0.4, 1.2, bodyColor);
            AddBox(group, -0.3, 0.65, -0.5, 1.1, 0.35, 1.0, Color.FromRgb(180, 220, 255));
            AddWheel(group, -1.2, 0.12, -0.85);
            AddWheel(group, -1.2, 0.12, 0.85);
            AddWheel(group, 1.2, 0.12, -0.85);
            AddWheel(group, 1.2, 0.12, 0.85);
            AddBox(group, 1.7, 0.25, -0.5, 0.25, 0.15, 1.0, Color.FromRgb(255, 50, 50));
            AddBox(group, -1.9, 0.25, -0.5, 0.15, 0.15, 1.0, Color.FromRgb(255, 255, 200));
            AddBox(group, 0.8, 1.0, -0.3, 1.0, 0.08, 0.6, bodyColor);
        }

        private void BuildMotorcycle(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -0.5, 0.5, -0.15, 1.5, 0.35, 0.3, bodyColor);
            AddBox(group, -0.8, 0.7, -0.1, 0.4, 0.6, 0.2, Color.FromRgb(40, 40, 40));
            AddBox(group, 0.1, 0.85, -0.08, 0.3, 0.15, 0.16, Color.FromRgb(80, 80, 80));
            AddWheel(group, -0.7, 0.2, 0);
            AddWheel(group, 0.8, 0.2, 0);
            AddBox(group, -0.2, 1.1, -0.2, 0.1, 0.5, 0.4, Color.FromRgb(200, 200, 200));
            AddBox(group, 0.6, 0.45, -0.12, 0.5, 0.1, 0.24, Color.FromRgb(200, 200, 200));
        }

        private void BuildTruck(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -2.2, 0.6, -0.9, 4.4, 0.8, 1.8, Color.FromRgb(80, 80, 80));
            AddBox(group, -2.2, 1.4, -0.85, 1.6, 0.8, 1.7, bodyColor);
            AddBox(group, -2.0, 1.4, -0.75, 1.2, 0.7, 1.5, Color.FromRgb(180, 220, 255));
            AddBox(group, -0.4, 1.4, -0.85, 2.6, 1.0, 1.7, Color.FromRgb(100, 100, 100));
            AddWheel(group, -1.5, 0.3, -1.0);
            AddWheel(group, -1.5, 0.3, 1.0);
            AddWheel(group, 1.0, 0.3, -1.0);
            AddWheel(group, 1.0, 0.3, 1.0);
            AddWheel(group, 1.6, 0.3, -1.0);
            AddWheel(group, 1.6, 0.3, 1.0);
        }

        private void BuildCoupe(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -1.7, 0.25, -0.7, 3.4, 0.5, 1.4, bodyColor);
            AddBox(group, -0.4, 0.75, -0.6, 1.6, 0.45, 1.2, bodyColor);
            AddBox(group, -0.2, 0.75, -0.5, 1.2, 0.4, 1.0, Color.FromRgb(180, 220, 255));
            AddWheel(group, -1.1, 0.13, -0.8);
            AddWheel(group, -1.1, 0.13, 0.8);
            AddWheel(group, 1.1, 0.13, -0.8);
            AddWheel(group, 1.1, 0.13, 0.8);
            AddBox(group, 1.5, 0.3, -0.5, 0.2, 0.18, 1.0, Color.FromRgb(255, 50, 50));
            AddBox(group, -1.7, 0.3, -0.5, 0.15, 0.18, 1.0, Color.FromRgb(255, 255, 200));
        }

        private void BuildHatchback(Model3DGroup group)
        {
            var bodyColor = CarColor;
            AddBox(group, -1.5, 0.3, -0.65, 3.0, 0.55, 1.3, bodyColor);
            AddBox(group, -0.6, 0.85, -0.6, 1.8, 0.55, 1.2, bodyColor);
            AddBox(group, -0.4, 0.85, -0.5, 1.4, 0.5, 1.0, Color.FromRgb(180, 220, 255));
            AddWheel(group, -1.0, 0.15, -0.75);
            AddWheel(group, -1.0, 0.15, 0.75);
            AddWheel(group, 1.0, 0.15, -0.75);
            AddWheel(group, 1.0, 0.15, 0.75);
            AddBox(group, 1.3, 0.35, -0.45, 0.2, 0.18, 0.9, Color.FromRgb(255, 50, 50));
        }

        private static void AddBox(Model3DGroup group, double x, double y, double z,
            double width, double height, double depth, Color color)
        {
            double x2 = x + width, y2 = y + height, z2 = z + depth;

            AddQuad(group, new Point3D(x, y, z2), new Point3D(x2, y, z2), new Point3D(x2, y2, z2), new Point3D(x, y2, z2), color);
            AddQuad(group, new Point3D(x2, y, z), new Point3D(x, y, z), new Point3D(x, y2, z), new Point3D(x2, y2, z), color);
            AddQuad(group, new Point3D(x, y2, z), new Point3D(x, y2, z2), new Point3D(x2, y2, z2), new Point3D(x2, y2, z), color);
            AddQuad(group, new Point3D(x, y, z2), new Point3D(x, y, z), new Point3D(x2, y, z), new Point3D(x2, y, z2), color);
            AddQuad(group, new Point3D(x2, y, z), new Point3D(x2, y, z2), new Point3D(x2, y2, z2), new Point3D(x2, y2, z), color);
            AddQuad(group, new Point3D(x, y, z2), new Point3D(x, y, z), new Point3D(x, y2, z), new Point3D(x, y2, z2), color);
        }

        private static void AddWheel(Model3DGroup group, double x, double y, double z)
        {
            var wheelColor = Color.FromRgb(30, 30, 30);
            var rimColor = Color.FromRgb(180, 180, 200);
            AddBox(group, x - 0.18, y - 0.18, z - 0.1, 0.36, 0.36, 0.2, wheelColor);
            AddBox(group, x - 0.1, y - 0.1, z - 0.11, 0.2, 0.2, 0.22, rimColor);
        }

        private static void AddQuad(Model3DGroup group, Point3D p0, Point3D p1, Point3D p2, Point3D p3, Color color)
        {
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            var material = new DiffuseMaterial(new SolidColorBrush(color));
            var geoModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
            group.Children.Add(geoModel);
        }

        private void StartRotation()
        {
            if (_rotation != null)
            {
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = TimeSpan.FromSeconds(12),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                _rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation);
            }
        }
    }
}
