using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace RGBSelcer.Helpers
{
    internal sealed class MeshBuilder
    {
        private readonly List<Point3D> _positions = new();
        private readonly List<Vector3D> _normals = new();
        private readonly List<int> _indices = new();

        public MeshGeometry3D ToMesh()
        {
            var mesh = new MeshGeometry3D();
            foreach (var p in _positions) mesh.Positions.Add(p);
            foreach (var n in _normals) mesh.Normals.Add(n);
            foreach (var i in _indices) mesh.TriangleIndices.Add(i);
            mesh.Freeze();
            return mesh;
        }

        public void AddBox(Point3D center, double xLen, double yLen, double zLen)
        {
            double hx = xLen / 2, hy = yLen / 2, hz = zLen / 2;
            double cx = center.X, cy = center.Y, cz = center.Z;
            var corners = new Point3D[]
            {
                new(cx - hx, cy - hy, cz - hz), new(cx + hx, cy - hy, cz - hz),
                new(cx + hx, cy + hy, cz - hz), new(cx - hx, cy + hy, cz - hz),
                new(cx - hx, cy - hy, cz + hz), new(cx + hx, cy - hy, cz + hz),
                new(cx + hx, cy + hy, cz + hz), new(cx - hx, cy + hy, cz + hz)
            };
            int[][] faces = {
                new[]{0,3,2,1}, new[]{4,5,6,7}, new[]{0,1,5,4},
                new[]{2,3,7,6}, new[]{0,4,7,3}, new[]{1,2,6,5}
            };
            Vector3D[] normals = {
                new(0,0,-1), new(0,0,1), new(0,-1,0),
                new(0,1,0), new(-1,0,0), new(1,0,0)
            };
            for (int f = 0; f < 6; f++)
            {
                int start = _positions.Count;
                for (int v = 0; v < 4; v++)
                {
                    _positions.Add(corners[faces[f][v]]);
                    _normals.Add(normals[f]);
                }
                _indices.Add(start); _indices.Add(start + 1); _indices.Add(start + 2);
                _indices.Add(start); _indices.Add(start + 2); _indices.Add(start + 3);
            }
        }

        public void AddCylinder(Point3D p1, Point3D p2, double radius, int segments)
        {
            var axis = p2 - p1;
            double length = axis.Length;
            if (length < 1e-9) return;
            axis.Normalize();
            var perp = Math.Abs(axis.Y) < 0.9
                ? Vector3D.CrossProduct(axis, new Vector3D(0, 1, 0))
                : Vector3D.CrossProduct(axis, new Vector3D(1, 0, 0));
            perp.Normalize();
            var perp2 = Vector3D.CrossProduct(axis, perp);
            perp2.Normalize();
            var bottomRing = new Point3D[segments];
            var topRing = new Point3D[segments];
            var ringNormals = new Vector3D[segments];
            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double cos = Math.Cos(angle), sin = Math.Sin(angle);
                var normal = perp * cos + perp2 * sin;
                ringNormals[i] = normal;
                var offset = normal * radius;
                bottomRing[i] = p1 + offset;
                topRing[i] = p2 + offset;
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int start = _positions.Count;
                _positions.Add(bottomRing[i]); _normals.Add(ringNormals[i]);
                _positions.Add(topRing[i]); _normals.Add(ringNormals[i]);
                _positions.Add(topRing[next]); _normals.Add(ringNormals[next]);
                _positions.Add(bottomRing[next]); _normals.Add(ringNormals[next]);
                _indices.Add(start); _indices.Add(start + 1); _indices.Add(start + 2);
                _indices.Add(start); _indices.Add(start + 2); _indices.Add(start + 3);
            }
            var downNormal = -axis;
            var upNormal = axis;
            int bc = _positions.Count;
            _positions.Add(p1); _normals.Add(downNormal);
            for (int i = 0; i < segments; i++) { _positions.Add(bottomRing[i]); _normals.Add(downNormal); }
            for (int i = 0; i < segments; i++) { int next = (i + 1) % segments; _indices.Add(bc); _indices.Add(bc + 1 + next); _indices.Add(bc + 1 + i); }
            int tc = _positions.Count;
            _positions.Add(p2); _normals.Add(upNormal);
            for (int i = 0; i < segments; i++) { _positions.Add(topRing[i]); _normals.Add(upNormal); }
            for (int i = 0; i < segments; i++) { int next = (i + 1) % segments; _indices.Add(tc); _indices.Add(tc + 1 + i); _indices.Add(tc + 1 + next); }
        }

        public void AddSphere(Point3D center, double radius, int stacks, int slices)
        {
            int startIdx = _positions.Count;
            for (int stack = 0; stack <= stacks; stack++)
            {
                double phi = Math.PI * stack / stacks;
                double sinPhi = Math.Sin(phi), cosPhi = Math.Cos(phi);
                for (int slice = 0; slice <= slices; slice++)
                {
                    double theta = 2 * Math.PI * slice / slices;
                    double sinTheta = Math.Sin(theta), cosTheta = Math.Cos(theta);
                    var normal = new Vector3D(sinPhi * cosTheta, cosPhi, sinPhi * sinTheta);
                    _positions.Add(new Point3D(center.X + radius * normal.X, center.Y + radius * normal.Y, center.Z + radius * normal.Z));
                    _normals.Add(normal);
                }
            }
            int cols = slices + 1;
            for (int stack = 0; stack < stacks; stack++)
                for (int slice = 0; slice < slices; slice++)
                {
                    int a = startIdx + stack * cols + slice, b = a + cols, c = b + 1, d = a + 1;
                    _indices.Add(a); _indices.Add(b); _indices.Add(c);
                    _indices.Add(a); _indices.Add(c); _indices.Add(d);
                }
        }

        public void AddEllipsoid(Point3D center, double rx, double ry, double rz, int stacks = 10, int slices = 12)
        {
            int startIdx = _positions.Count;
            for (int stack = 0; stack <= stacks; stack++)
            {
                double phi = Math.PI * stack / stacks;
                double sinPhi = Math.Sin(phi), cosPhi = Math.Cos(phi);
                for (int slice = 0; slice <= slices; slice++)
                {
                    double theta = 2 * Math.PI * slice / slices;
                    double sinTheta = Math.Sin(theta), cosTheta = Math.Cos(theta);
                    double x = rx * sinPhi * cosTheta, y = ry * cosPhi, z = rz * sinPhi * sinTheta;
                    _positions.Add(new Point3D(center.X + x, center.Y + y, center.Z + z));
                    var n = new Vector3D(x / (rx * rx), y / (ry * ry), z / (rz * rz));
                    n.Normalize();
                    _normals.Add(n);
                }
            }
            int cols = slices + 1;
            for (int stack = 0; stack < stacks; stack++)
                for (int slice = 0; slice < slices; slice++)
                {
                    int a = startIdx + stack * cols + slice, b = a + cols, c = b + 1, d = a + 1;
                    _indices.Add(a); _indices.Add(b); _indices.Add(c);
                    _indices.Add(a); _indices.Add(c); _indices.Add(d);
                }
        }
    }


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

        private double _angle;
        private bool _isRendering;
        private RotateTransform3D? _rotateTransform;

        public Car3DModel()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static void OnCarTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Car3DModel model && model.IsLoaded)
            {
                model.Children.Clear();
                model.BuildScene();
                model.StartRendering();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildScene();
            StartRendering();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopRendering();
        }

        private void StartRendering()
        {
            if (!_isRendering)
            {
                _isRendering = true;
                CompositionTarget.Rendering += OnRendering;
            }
        }

        private void StopRendering()
        {
            if (_isRendering)
            {
                _isRendering = false;
                CompositionTarget.Rendering -= OnRendering;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_rotateTransform?.Rotation is AxisAngleRotation3D rotation)
            {
                _angle += 1.2;
                if (_angle >= 360) _angle -= 360;
                rotation.Angle = _angle;
            }
        }

        private void BuildScene()
        {
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(5, 3.5, 6),
                LookDirection = new Vector3D(-5, -3.5, -6),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 42
            };
            Camera = camera;

            var modelGroup = new Model3DGroup();

            switch (CarType?.ToLower())
            {
                case "suv": BuildSUV(modelGroup); break;
                case "sport": BuildSportCar(modelGroup); break;
                case "motorcycle": BuildMotorcycle(modelGroup); break;
                case "truck": BuildTruck(modelGroup); break;
                case "coupe": BuildCoupe(modelGroup); break;
                case "hatchback": BuildHatchback(modelGroup); break;
                default: BuildSedan(modelGroup); break;
            }

            modelGroup.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 90)));
            modelGroup.Children.Add(new DirectionalLight(Color.FromRgb(255, 255, 250), new Vector3D(-1, -1, -0.5)));
            modelGroup.Children.Add(new DirectionalLight(Color.FromRgb(100, 100, 140), new Vector3D(1, -0.3, 0.5)));
            modelGroup.Children.Add(new DirectionalLight(Color.FromRgb(60, 60, 80), new Vector3D(0, 1, 0)));

            var axisRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            _rotateTransform = new RotateTransform3D(axisRotation);
            modelGroup.Transform = _rotateTransform;

            var floor = new MeshBuilder();
            floor.AddBox(new Point3D(0, -0.02, 0), 12, 0.04, 12);
            var floorMat = CreateMaterial(Color.FromRgb(40, 35, 50), 0.1);
            modelGroup.Children.Add(new GeometryModel3D(floor.ToMesh(), floorMat));

            Children.Add(new ModelVisual3D { Content = modelGroup });
        }

        private static Material CreateMaterial(Color color, double specularPower = 0.6)
        {
            var group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
            if (specularPower > 0)
            {
                group.Children.Add(new SpecularMaterial(
                    new SolidColorBrush(Color.FromArgb((byte)(specularPower * 255), 255, 255, 255)),
                    specularPower * 100));
            }
            return group;
        }

        private static Material CreateGlassMaterial()
        {
            var group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(140, 160, 200, 255))));
            group.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), 120));
            return group;
        }

        private static Material CreateChromeMaterial()
        {
            var group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 210))));
            group.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)), 150));
            return group;
        }

        private void AddWheel(Model3DGroup group, double x, double y, double z, double radius = 0.3, double width = 0.18)
        {
            var tire = new MeshBuilder();
            tire.AddCylinder(new Point3D(x, y, z - width), new Point3D(x, y, z + width), radius, 24);
            var tireMat = CreateMaterial(Color.FromRgb(25, 25, 25), 0.15);
            group.Children.Add(new GeometryModel3D(tire.ToMesh(), tireMat) { BackMaterial = tireMat });

            var rim = new MeshBuilder();
            rim.AddCylinder(new Point3D(x, y, z - width * 0.6), new Point3D(x, y, z + width * 0.6), radius * 0.65, 16);
            var rimMat = CreateChromeMaterial();
            group.Children.Add(new GeometryModel3D(rim.ToMesh(), rimMat) { BackMaterial = rimMat });

            var hub = new MeshBuilder();
            hub.AddSphere(new Point3D(x, y, z + width * 0.55), radius * 0.15, 8, 8);
            hub.AddSphere(new Point3D(x, y, z - width * 0.55), radius * 0.15, 8, 8);
            group.Children.Add(new GeometryModel3D(hub.ToMesh(), rimMat) { BackMaterial = rimMat });
        }

        private void AddHeadlight(Model3DGroup group, double x, double y, double z, double size = 0.12)
        {
            var light = new MeshBuilder();
            light.AddSphere(new Point3D(x, y, z), size, 8, 8);
            var lightMat = new MaterialGroup();
            lightMat.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 220))));
            lightMat.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(180, 255, 255, 200))));
            group.Children.Add(new GeometryModel3D(light.ToMesh(), lightMat));
        }

        private void AddTaillight(Model3DGroup group, double x, double y, double z, double size = 0.1)
        {
            var light = new MeshBuilder();
            light.AddSphere(new Point3D(x, y, z), size, 8, 8);
            var lightMat = new MaterialGroup();
            lightMat.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 20, 20))));
            lightMat.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(150, 255, 0, 0))));
            group.Children.Add(new GeometryModel3D(light.ToMesh(), lightMat));
        }

        private void BuildSedan(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.7);
            var glassMat = CreateGlassMaterial();

            var body = new MeshBuilder();
            body.AddBox(new Point3D(0, 0.55, 0), 3.8, 0.55, 1.6);
            group.Children.Add(new GeometryModel3D(body.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var cabin = new MeshBuilder();
            cabin.AddBox(new Point3D(0.1, 1.1, 0), 2.0, 0.55, 1.45);
            group.Children.Add(new GeometryModel3D(cabin.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var roof = new MeshBuilder();
            roof.AddBox(new Point3D(0.1, 1.42, 0), 1.6, 0.1, 1.3);
            group.Children.Add(new GeometryModel3D(roof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var frontGlass = new MeshBuilder();
            frontGlass.AddBox(new Point3D(-0.72, 1.08, 0), 0.08, 0.48, 1.3);
            group.Children.Add(new GeometryModel3D(frontGlass.ToMesh(), glassMat));
            var rearGlass = new MeshBuilder();
            rearGlass.AddBox(new Point3D(0.92, 1.08, 0), 0.08, 0.48, 1.3);
            group.Children.Add(new GeometryModel3D(rearGlass.ToMesh(), glassMat));
            var sideGlassL = new MeshBuilder();
            sideGlassL.AddBox(new Point3D(0.1, 1.08, -0.72), 1.5, 0.42, 0.06);
            group.Children.Add(new GeometryModel3D(sideGlassL.ToMesh(), glassMat));
            var sideGlassR = new MeshBuilder();
            sideGlassR.AddBox(new Point3D(0.1, 1.08, 0.72), 1.5, 0.42, 0.06);
            group.Children.Add(new GeometryModel3D(sideGlassR.ToMesh(), glassMat));

            var hood = new MeshBuilder();
            hood.AddBox(new Point3D(-1.2, 0.85, 0), 1.3, 0.06, 1.5);
            group.Children.Add(new GeometryModel3D(hood.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var trunk = new MeshBuilder();
            trunk.AddBox(new Point3D(1.4, 0.82, 0), 1.0, 0.06, 1.5);
            group.Children.Add(new GeometryModel3D(trunk.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var bumperF = new MeshBuilder();
            bumperF.AddBox(new Point3D(-1.95, 0.35, 0), 0.15, 0.35, 1.65);
            group.Children.Add(new GeometryModel3D(bumperF.ToMesh(), CreateMaterial(Color.FromRgb(30, 30, 30), 0.3)));
            var bumperR = new MeshBuilder();
            bumperR.AddBox(new Point3D(1.95, 0.35, 0), 0.15, 0.35, 1.65);
            group.Children.Add(new GeometryModel3D(bumperR.ToMesh(), CreateMaterial(Color.FromRgb(30, 30, 30), 0.3)));

            var grille = new MeshBuilder();
            grille.AddBox(new Point3D(-1.92, 0.5, 0), 0.05, 0.25, 0.9);
            group.Children.Add(new GeometryModel3D(grille.ToMesh(), CreateChromeMaterial()));

            AddWheel(group, -1.15, 0.3, -0.85);
            AddWheel(group, -1.15, 0.3, 0.85);
            AddWheel(group, 1.15, 0.3, -0.85);
            AddWheel(group, 1.15, 0.3, 0.85);
            AddHeadlight(group, -1.9, 0.6, -0.5);
            AddHeadlight(group, -1.9, 0.6, 0.5);
            AddTaillight(group, 1.9, 0.6, -0.55);
            AddTaillight(group, 1.9, 0.6, 0.55);
        }

        private void BuildSUV(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.7);
            var glassMat = CreateGlassMaterial();
            var blackMat = CreateMaterial(Color.FromRgb(30, 30, 30), 0.3);

            var body = new MeshBuilder();
            body.AddBox(new Point3D(0, 0.75, 0), 4.2, 0.7, 1.8);
            group.Children.Add(new GeometryModel3D(body.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var underguard = new MeshBuilder();
            underguard.AddBox(new Point3D(0, 0.3, 0), 4.3, 0.2, 1.85);
            group.Children.Add(new GeometryModel3D(underguard.ToMesh(), blackMat));

            var cabin = new MeshBuilder();
            cabin.AddBox(new Point3D(0, 1.4, 0), 2.6, 0.7, 1.7);
            group.Children.Add(new GeometryModel3D(cabin.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var roof = new MeshBuilder();
            roof.AddBox(new Point3D(0, 1.8, 0), 2.2, 0.1, 1.55);
            group.Children.Add(new GeometryModel3D(roof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var roofrack = new MeshBuilder();
            roofrack.AddCylinder(new Point3D(-0.8, 1.88, -0.65), new Point3D(-0.8, 1.88, 0.65), 0.03, 8);
            roofrack.AddCylinder(new Point3D(0.5, 1.88, -0.65), new Point3D(0.5, 1.88, 0.65), 0.03, 8);
            group.Children.Add(new GeometryModel3D(roofrack.ToMesh(), CreateChromeMaterial()));

            var frontGlass = new MeshBuilder();
            frontGlass.AddBox(new Point3D(-1.08, 1.38, 0), 0.08, 0.55, 1.5);
            group.Children.Add(new GeometryModel3D(frontGlass.ToMesh(), glassMat));
            var rearGlass = new MeshBuilder();
            rearGlass.AddBox(new Point3D(1.08, 1.38, 0), 0.08, 0.55, 1.5);
            group.Children.Add(new GeometryModel3D(rearGlass.ToMesh(), glassMat));
            var sideL = new MeshBuilder();
            sideL.AddBox(new Point3D(0, 1.38, -0.84), 2.0, 0.45, 0.06);
            group.Children.Add(new GeometryModel3D(sideL.ToMesh(), glassMat));
            var sideR = new MeshBuilder();
            sideR.AddBox(new Point3D(0, 1.38, 0.84), 2.0, 0.45, 0.06);
            group.Children.Add(new GeometryModel3D(sideR.ToMesh(), glassMat));

            var hood = new MeshBuilder();
            hood.AddBox(new Point3D(-1.5, 1.12, 0), 1.1, 0.06, 1.7);
            group.Children.Add(new GeometryModel3D(hood.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var bumperF = new MeshBuilder();
            bumperF.AddBox(new Point3D(-2.15, 0.5, 0), 0.15, 0.5, 1.85);
            group.Children.Add(new GeometryModel3D(bumperF.ToMesh(), blackMat));
            var bumperR = new MeshBuilder();
            bumperR.AddBox(new Point3D(2.15, 0.5, 0), 0.15, 0.5, 1.85);
            group.Children.Add(new GeometryModel3D(bumperR.ToMesh(), blackMat));

            AddWheel(group, -1.35, 0.38, -0.95, 0.38, 0.22);
            AddWheel(group, -1.35, 0.38, 0.95, 0.38, 0.22);
            AddWheel(group, 1.35, 0.38, -0.95, 0.38, 0.22);
            AddWheel(group, 1.35, 0.38, 0.95, 0.38, 0.22);
            AddHeadlight(group, -2.08, 0.75, -0.6, 0.14);
            AddHeadlight(group, -2.08, 0.75, 0.6, 0.14);
            AddTaillight(group, 2.08, 0.75, -0.6, 0.12);
            AddTaillight(group, 2.08, 0.75, 0.6, 0.12);
        }

        private void BuildSportCar(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.85);
            var glassMat = CreateGlassMaterial();
            var carbonMat = CreateMaterial(Color.FromRgb(20, 20, 20), 0.4);

            var body = new MeshBuilder();
            body.AddBox(new Point3D(0, 0.4, 0), 4.0, 0.4, 1.65);
            group.Children.Add(new GeometryModel3D(body.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var bodyTop = new MeshBuilder();
            bodyTop.AddBox(new Point3D(-0.3, 0.68, 0), 3.2, 0.18, 1.6);
            group.Children.Add(new GeometryModel3D(bodyTop.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var cabin = new MeshBuilder();
            cabin.AddBox(new Point3D(0.3, 0.95, 0), 1.4, 0.4, 1.4);
            group.Children.Add(new GeometryModel3D(cabin.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var roof = new MeshBuilder();
            roof.AddBox(new Point3D(0.3, 1.18, 0), 1.1, 0.08, 1.25);
            group.Children.Add(new GeometryModel3D(roof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var frontGlass = new MeshBuilder();
            frontGlass.AddBox(new Point3D(-0.28, 0.93, 0), 0.08, 0.35, 1.25);
            group.Children.Add(new GeometryModel3D(frontGlass.ToMesh(), glassMat));
            var rearGlass = new MeshBuilder();
            rearGlass.AddBox(new Point3D(0.88, 0.93, 0), 0.08, 0.3, 1.2);
            group.Children.Add(new GeometryModel3D(rearGlass.ToMesh(), glassMat));
            var sideL = new MeshBuilder();
            sideL.AddBox(new Point3D(0.3, 0.93, -0.69), 1.0, 0.3, 0.06);
            group.Children.Add(new GeometryModel3D(sideL.ToMesh(), glassMat));
            var sideR = new MeshBuilder();
            sideR.AddBox(new Point3D(0.3, 0.93, 0.69), 1.0, 0.3, 0.06);
            group.Children.Add(new GeometryModel3D(sideR.ToMesh(), glassMat));

            var hood = new MeshBuilder();
            hood.AddBox(new Point3D(-1.2, 0.75, 0), 1.5, 0.05, 1.55);
            group.Children.Add(new GeometryModel3D(hood.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var hoodVent = new MeshBuilder();
            hoodVent.AddBox(new Point3D(-1.0, 0.79, 0), 0.4, 0.04, 0.5);
            group.Children.Add(new GeometryModel3D(hoodVent.ToMesh(), carbonMat));

            var splitter = new MeshBuilder();
            splitter.AddBox(new Point3D(-2.05, 0.2, 0), 0.15, 0.08, 1.75);
            group.Children.Add(new GeometryModel3D(splitter.ToMesh(), carbonMat));
            var diffuser = new MeshBuilder();
            diffuser.AddBox(new Point3D(2.0, 0.22, 0), 0.12, 0.2, 1.5);
            group.Children.Add(new GeometryModel3D(diffuser.ToMesh(), carbonMat));

            var spoiler = new MeshBuilder();
            spoiler.AddBox(new Point3D(1.7, 0.95, 0), 0.4, 0.05, 1.5);
            spoiler.AddCylinder(new Point3D(1.7, 0.78, -0.5), new Point3D(1.7, 0.93, -0.5), 0.03, 8);
            spoiler.AddCylinder(new Point3D(1.7, 0.78, 0.5), new Point3D(1.7, 0.93, 0.5), 0.03, 8);
            group.Children.Add(new GeometryModel3D(spoiler.ToMesh(), carbonMat));

            var sideSkirt = new MeshBuilder();
            sideSkirt.AddBox(new Point3D(0, 0.18, -0.84), 3.4, 0.08, 0.06);
            sideSkirt.AddBox(new Point3D(0, 0.18, 0.84), 3.4, 0.08, 0.06);
            group.Children.Add(new GeometryModel3D(sideSkirt.ToMesh(), carbonMat));

            var exhaust = new MeshBuilder();
            exhaust.AddCylinder(new Point3D(2.0, 0.3, -0.35), new Point3D(2.1, 0.3, -0.35), 0.06, 12);
            exhaust.AddCylinder(new Point3D(2.0, 0.3, 0.35), new Point3D(2.1, 0.3, 0.35), 0.06, 12);
            group.Children.Add(new GeometryModel3D(exhaust.ToMesh(), CreateChromeMaterial()));

            AddWheel(group, -1.3, 0.28, -0.88, 0.28, 0.2);
            AddWheel(group, -1.3, 0.28, 0.88, 0.28, 0.2);
            AddWheel(group, 1.3, 0.28, -0.88, 0.28, 0.2);
            AddWheel(group, 1.3, 0.28, 0.88, 0.28, 0.2);
            AddHeadlight(group, -1.98, 0.55, -0.55, 0.1);
            AddHeadlight(group, -1.98, 0.55, 0.55, 0.1);
            AddTaillight(group, 1.98, 0.5, -0.55, 0.1);
            AddTaillight(group, 1.98, 0.5, 0.55, 0.1);
        }

        private void BuildMotorcycle(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.8);
            var chromeMat = CreateChromeMaterial();
            var blackMat = CreateMaterial(Color.FromRgb(25, 25, 25), 0.2);

            var frontWheel = new MeshBuilder();
            frontWheel.AddCylinder(new Point3D(-1.0, 0.35, -0.08), new Point3D(-1.0, 0.35, 0.08), 0.35, 24);
            group.Children.Add(new GeometryModel3D(frontWheel.ToMesh(), blackMat) { BackMaterial = blackMat });
            var frontRim = new MeshBuilder();
            frontRim.AddCylinder(new Point3D(-1.0, 0.35, -0.06), new Point3D(-1.0, 0.35, 0.06), 0.22, 16);
            group.Children.Add(new GeometryModel3D(frontRim.ToMesh(), chromeMat));

            var rearWheel = new MeshBuilder();
            rearWheel.AddCylinder(new Point3D(0.8, 0.35, -0.1), new Point3D(0.8, 0.35, 0.1), 0.35, 24);
            group.Children.Add(new GeometryModel3D(rearWheel.ToMesh(), blackMat) { BackMaterial = blackMat });
            var rearRim = new MeshBuilder();
            rearRim.AddCylinder(new Point3D(0.8, 0.35, -0.08), new Point3D(0.8, 0.35, 0.08), 0.22, 16);
            group.Children.Add(new GeometryModel3D(rearRim.ToMesh(), chromeMat));

            var frame = new MeshBuilder();
            frame.AddCylinder(new Point3D(-0.5, 0.7, 0), new Point3D(0.6, 0.55, 0), 0.06, 8);
            frame.AddCylinder(new Point3D(-0.5, 0.7, 0), new Point3D(-0.3, 0.4, 0), 0.05, 8);
            frame.AddCylinder(new Point3D(0.6, 0.55, 0), new Point3D(0.8, 0.35, 0), 0.05, 8);
            group.Children.Add(new GeometryModel3D(frame.ToMesh(), blackMat));

            var tank = new MeshBuilder();
            tank.AddEllipsoid(new Point3D(-0.1, 0.85, 0), 0.45, 0.2, 0.25);
            group.Children.Add(new GeometryModel3D(tank.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var frontFairing = new MeshBuilder();
            frontFairing.AddBox(new Point3D(-0.55, 0.85, 0), 0.3, 0.35, 0.35);
            group.Children.Add(new GeometryModel3D(frontFairing.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var tail = new MeshBuilder();
            tail.AddBox(new Point3D(0.5, 0.65, 0), 0.5, 0.12, 0.22);
            group.Children.Add(new GeometryModel3D(tail.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var seat = new MeshBuilder();
            seat.AddBox(new Point3D(0.15, 0.92, 0), 0.6, 0.08, 0.22);
            group.Children.Add(new GeometryModel3D(seat.ToMesh(), blackMat));

            var forks = new MeshBuilder();
            forks.AddCylinder(new Point3D(-0.75, 1.0, -0.05), new Point3D(-1.0, 0.35, -0.05), 0.025, 8);
            forks.AddCylinder(new Point3D(-0.75, 1.0, 0.05), new Point3D(-1.0, 0.35, 0.05), 0.025, 8);
            group.Children.Add(new GeometryModel3D(forks.ToMesh(), chromeMat));
            var handlebars = new MeshBuilder();
            handlebars.AddCylinder(new Point3D(-0.72, 1.05, -0.25), new Point3D(-0.72, 1.05, 0.25), 0.02, 8);
            group.Children.Add(new GeometryModel3D(handlebars.ToMesh(), blackMat));

            var windscreen = new MeshBuilder();
            windscreen.AddBox(new Point3D(-0.65, 1.12, 0), 0.04, 0.2, 0.2);
            group.Children.Add(new GeometryModel3D(windscreen.ToMesh(), CreateGlassMaterial()));

            var exhaustPipe = new MeshBuilder();
            exhaustPipe.AddCylinder(new Point3D(0.2, 0.35, 0.15), new Point3D(0.85, 0.3, 0.18), 0.04, 10);
            group.Children.Add(new GeometryModel3D(exhaustPipe.ToMesh(), chromeMat));

            AddHeadlight(group, -0.72, 0.95, 0, 0.08);
            AddTaillight(group, 0.78, 0.6, 0, 0.06);
        }

        private void BuildTruck(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.6);
            var blackMat = CreateMaterial(Color.FromRgb(30, 30, 30), 0.3);
            var glassMat = CreateGlassMaterial();

            var chassis = new MeshBuilder();
            chassis.AddBox(new Point3D(0, 0.5, 0), 4.8, 0.3, 1.9);
            group.Children.Add(new GeometryModel3D(chassis.ToMesh(), blackMat));

            var cab = new MeshBuilder();
            cab.AddBox(new Point3D(-1.5, 1.2, 0), 1.5, 1.1, 1.8);
            group.Children.Add(new GeometryModel3D(cab.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var cabRoof = new MeshBuilder();
            cabRoof.AddBox(new Point3D(-1.5, 1.82, 0), 1.3, 0.08, 1.7);
            group.Children.Add(new GeometryModel3D(cabRoof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var cabGlassF = new MeshBuilder();
            cabGlassF.AddBox(new Point3D(-2.27, 1.25, 0), 0.06, 0.65, 1.5);
            group.Children.Add(new GeometryModel3D(cabGlassF.ToMesh(), glassMat));
            var cabGlassL = new MeshBuilder();
            cabGlassL.AddBox(new Point3D(-1.5, 1.25, -0.91), 1.1, 0.55, 0.06);
            group.Children.Add(new GeometryModel3D(cabGlassL.ToMesh(), glassMat));
            var cabGlassR = new MeshBuilder();
            cabGlassR.AddBox(new Point3D(-1.5, 1.25, 0.91), 1.1, 0.55, 0.06);
            group.Children.Add(new GeometryModel3D(cabGlassR.ToMesh(), glassMat));

            var bed = new MeshBuilder();
            bed.AddBox(new Point3D(0.8, 0.9, 0), 2.8, 0.5, 1.85);
            bed.AddBox(new Point3D(0.8, 1.2, -0.9), 2.8, 0.15, 0.08);
            bed.AddBox(new Point3D(0.8, 1.2, 0.9), 2.8, 0.15, 0.08);
            bed.AddBox(new Point3D(2.18, 1.2, 0), 0.08, 0.15, 1.85);
            group.Children.Add(new GeometryModel3D(bed.ToMesh(), CreateMaterial(Color.FromRgb(80, 80, 80), 0.3)));

            AddWheel(group, -1.6, 0.35, -1.0, 0.35, 0.2);
            AddWheel(group, -1.6, 0.35, 1.0, 0.35, 0.2);
            AddWheel(group, 1.0, 0.35, -1.0, 0.35, 0.2);
            AddWheel(group, 1.0, 0.35, 1.0, 0.35, 0.2);
            AddWheel(group, 1.7, 0.35, -1.0, 0.35, 0.2);
            AddWheel(group, 1.7, 0.35, 1.0, 0.35, 0.2);
            AddHeadlight(group, -2.25, 0.9, -0.6, 0.12);
            AddHeadlight(group, -2.25, 0.9, 0.6, 0.12);
            AddTaillight(group, 2.2, 0.8, -0.7, 0.1);
            AddTaillight(group, 2.2, 0.8, 0.7, 0.1);
        }

        private void BuildCoupe(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.8);
            var glassMat = CreateGlassMaterial();
            var carbonMat = CreateMaterial(Color.FromRgb(25, 25, 25), 0.4);

            var body = new MeshBuilder();
            body.AddBox(new Point3D(0, 0.48, 0), 3.6, 0.48, 1.55);
            group.Children.Add(new GeometryModel3D(body.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var bodyUpper = new MeshBuilder();
            bodyUpper.AddBox(new Point3D(-0.1, 0.75, 0), 3.0, 0.12, 1.5);
            group.Children.Add(new GeometryModel3D(bodyUpper.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var cabin = new MeshBuilder();
            cabin.AddBox(new Point3D(0.2, 1.0, 0), 1.6, 0.42, 1.4);
            group.Children.Add(new GeometryModel3D(cabin.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var roof = new MeshBuilder();
            roof.AddBox(new Point3D(0.2, 1.24, 0), 1.3, 0.06, 1.25);
            group.Children.Add(new GeometryModel3D(roof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var frontGlass = new MeshBuilder();
            frontGlass.AddBox(new Point3D(-0.48, 0.98, 0), 0.07, 0.38, 1.25);
            group.Children.Add(new GeometryModel3D(frontGlass.ToMesh(), glassMat));
            var rearGlass = new MeshBuilder();
            rearGlass.AddBox(new Point3D(0.88, 0.95, 0), 0.07, 0.3, 1.2);
            group.Children.Add(new GeometryModel3D(rearGlass.ToMesh(), glassMat));
            var sideL = new MeshBuilder();
            sideL.AddBox(new Point3D(0.2, 0.98, -0.69), 1.2, 0.32, 0.06);
            group.Children.Add(new GeometryModel3D(sideL.ToMesh(), glassMat));
            var sideR = new MeshBuilder();
            sideR.AddBox(new Point3D(0.2, 0.98, 0.69), 1.2, 0.32, 0.06);
            group.Children.Add(new GeometryModel3D(sideR.ToMesh(), glassMat));

            var hood = new MeshBuilder();
            hood.AddBox(new Point3D(-1.1, 0.79, 0), 1.3, 0.05, 1.45);
            group.Children.Add(new GeometryModel3D(hood.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var trunk = new MeshBuilder();
            trunk.AddBox(new Point3D(1.2, 0.72, 0), 1.0, 0.05, 1.4);
            group.Children.Add(new GeometryModel3D(trunk.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var bumperF = new MeshBuilder();
            bumperF.AddBox(new Point3D(-1.85, 0.3, 0), 0.12, 0.3, 1.6);
            group.Children.Add(new GeometryModel3D(bumperF.ToMesh(), carbonMat));
            var bumperR = new MeshBuilder();
            bumperR.AddBox(new Point3D(1.85, 0.3, 0), 0.12, 0.3, 1.6);
            group.Children.Add(new GeometryModel3D(bumperR.ToMesh(), carbonMat));

            var exhaustMesh = new MeshBuilder();
            exhaustMesh.AddCylinder(new Point3D(1.82, 0.28, -0.4), new Point3D(1.92, 0.28, -0.4), 0.05, 10);
            exhaustMesh.AddCylinder(new Point3D(1.82, 0.28, 0.4), new Point3D(1.92, 0.28, 0.4), 0.05, 10);
            group.Children.Add(new GeometryModel3D(exhaustMesh.ToMesh(), CreateChromeMaterial()));

            AddWheel(group, -1.15, 0.28, -0.82, 0.28, 0.18);
            AddWheel(group, -1.15, 0.28, 0.82, 0.28, 0.18);
            AddWheel(group, 1.15, 0.28, -0.82, 0.28, 0.18);
            AddWheel(group, 1.15, 0.28, 0.82, 0.28, 0.18);
            AddHeadlight(group, -1.82, 0.55, -0.5);
            AddHeadlight(group, -1.82, 0.55, 0.5);
            AddTaillight(group, 1.82, 0.52, -0.5);
            AddTaillight(group, 1.82, 0.52, 0.5);
        }

        private void BuildHatchback(Model3DGroup group)
        {
            var bodyMat = CreateMaterial(CarColor, 0.65);
            var glassMat = CreateGlassMaterial();
            var blackMat = CreateMaterial(Color.FromRgb(30, 30, 30), 0.3);

            var body = new MeshBuilder();
            body.AddBox(new Point3D(0, 0.52, 0), 3.2, 0.52, 1.5);
            group.Children.Add(new GeometryModel3D(body.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var cabin = new MeshBuilder();
            cabin.AddBox(new Point3D(0.1, 1.05, 0), 1.8, 0.55, 1.4);
            group.Children.Add(new GeometryModel3D(cabin.ToMesh(), bodyMat) { BackMaterial = bodyMat });
            var roof = new MeshBuilder();
            roof.AddBox(new Point3D(0.1, 1.35, 0), 1.5, 0.08, 1.25);
            group.Children.Add(new GeometryModel3D(roof.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var frontGlass = new MeshBuilder();
            frontGlass.AddBox(new Point3D(-0.65, 1.02, 0), 0.07, 0.45, 1.2);
            group.Children.Add(new GeometryModel3D(frontGlass.ToMesh(), glassMat));
            var rearGlass = new MeshBuilder();
            rearGlass.AddBox(new Point3D(0.82, 1.0, 0), 0.07, 0.45, 1.2);
            group.Children.Add(new GeometryModel3D(rearGlass.ToMesh(), glassMat));
            var sideL = new MeshBuilder();
            sideL.AddBox(new Point3D(0.1, 1.02, -0.69), 1.4, 0.38, 0.06);
            group.Children.Add(new GeometryModel3D(sideL.ToMesh(), glassMat));
            var sideR = new MeshBuilder();
            sideR.AddBox(new Point3D(0.1, 1.02, 0.69), 1.4, 0.38, 0.06);
            group.Children.Add(new GeometryModel3D(sideR.ToMesh(), glassMat));

            var hood = new MeshBuilder();
            hood.AddBox(new Point3D(-1.0, 0.8, 0), 1.1, 0.05, 1.4);
            group.Children.Add(new GeometryModel3D(hood.ToMesh(), bodyMat) { BackMaterial = bodyMat });

            var bumperF = new MeshBuilder();
            bumperF.AddBox(new Point3D(-1.65, 0.32, 0), 0.12, 0.32, 1.55);
            group.Children.Add(new GeometryModel3D(bumperF.ToMesh(), blackMat));
            var bumperR = new MeshBuilder();
            bumperR.AddBox(new Point3D(1.65, 0.32, 0), 0.12, 0.32, 1.55);
            group.Children.Add(new GeometryModel3D(bumperR.ToMesh(), blackMat));

            AddWheel(group, -1.0, 0.28, -0.8, 0.28, 0.17);
            AddWheel(group, -1.0, 0.28, 0.8, 0.28, 0.17);
            AddWheel(group, 1.0, 0.28, -0.8, 0.28, 0.17);
            AddWheel(group, 1.0, 0.28, 0.8, 0.28, 0.17);
            AddHeadlight(group, -1.62, 0.55, -0.5);
            AddHeadlight(group, -1.62, 0.55, 0.5);
            AddTaillight(group, 1.62, 0.55, -0.5);
            AddTaillight(group, 1.62, 0.55, 0.5);
        }
    }
}
