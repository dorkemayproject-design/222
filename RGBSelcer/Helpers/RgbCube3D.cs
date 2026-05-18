using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace RGBSelcer.Helpers
{
    public class RgbCube3D : Viewport3D
    {
        public RgbCube3D()
        {
            Loaded += OnLoaded;
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
                Position = new Point3D(2.5, 2.0, 3.5),
                LookDirection = new Vector3D(-2.5, -2.0, -3.5),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45
            };
            Camera = camera;

            var modelGroup = new Model3DGroup();

            AddCubeFace(modelGroup,
                new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(1, 1, 1), new Point3D(0, 1, 1),
                Color.FromRgb(255, 0, 0));

            AddCubeFace(modelGroup,
                new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 1, 0), new Point3D(1, 0, 0),
                Color.FromRgb(0, 255, 0));

            AddCubeFace(modelGroup,
                new Point3D(0, 1, 0), new Point3D(0, 1, 1), new Point3D(1, 1, 1), new Point3D(1, 1, 0),
                Color.FromRgb(0, 0, 255));

            AddCubeFace(modelGroup,
                new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 0, 1), new Point3D(0, 0, 1),
                Color.FromRgb(255, 255, 0));

            AddCubeFace(modelGroup,
                new Point3D(1, 0, 0), new Point3D(1, 1, 0), new Point3D(1, 1, 1), new Point3D(1, 0, 1),
                Color.FromRgb(255, 0, 255));

            AddCubeFace(modelGroup,
                new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 1), new Point3D(0, 1, 0),
                Color.FromRgb(0, 255, 255));

            modelGroup.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
            modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));
            modelGroup.Children.Add(new DirectionalLight(Color.FromRgb(100, 100, 100), new Vector3D(1, 0.5, 0.5)));

            var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            var rotateTransform = new RotateTransform3D(rotation);
            rotation.SetValue(FrameworkElement.NameProperty, "CubeRotation");

            var centerTranslate = new TranslateTransform3D(-0.5, -0.5, -0.5);
            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(centerTranslate);
            transformGroup.Children.Add(rotateTransform);

            modelGroup.Transform = transformGroup;

            var modelVisual = new ModelVisual3D { Content = modelGroup };
            Children.Add(modelVisual);

            Tag = rotation;
        }

        private static void AddCubeFace(Model3DGroup group, Point3D p0, Point3D p1, Point3D p2, Point3D p3, Color color)
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
            var geoModel = new GeometryModel3D(mesh, material)
            {
                BackMaterial = material
            };
            group.Children.Add(geoModel);
        }

        private void StartRotation()
        {
            if (Tag is AxisAngleRotation3D rotation)
            {
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = TimeSpan.FromSeconds(10),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation);
            }
        }
    }
}
