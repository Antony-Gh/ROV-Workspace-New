using System;
using System.Linq;
using System.Windows;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Runtime.CompilerServices;
using System.Text;

namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for ROV3DModel.xaml
    /// </summary>
    public partial class ROV3DModel : UserControl, INotifyPropertyChanged, IDisposable
    {
        public static readonly DependencyProperty DirValueProperty = DependencyProperty.Register("MoveDir", typeof(int), typeof(ROV3DModel),
            new PropertyMetadata(0, OnDirValuePropertyChanged));
        public int MoveDir
        {
            get { return (int)GetValue(DirValueProperty); }
            set { SetValue(DirValueProperty, value); }
        }
        private static void OnDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROV3DModel f = d as ROV3DModel;
            f.OnDirValueChanged(e);
        }
        public virtual void OnDirValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MoveDir != -1)
                MoveTimer.Start();
            else
                MoveTimer.Stop();
        }
        public static readonly DependencyProperty MarkValueProperty = DependencyProperty.Register("AddMark", typeof(bool), typeof(ROV3DModel),
            new PropertyMetadata(false, OnMarkValuePropertyChanged));
        public bool AddMark
        {
            get { return (bool)GetValue(MarkValueProperty); }
            set { SetValue(MarkValueProperty, value); }
        }
        private static void OnMarkValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROV3DModel f = d as ROV3DModel;
            f.OnMarkValueChanged(e);
        }
        public virtual void OnMarkValueChanged(DependencyPropertyChangedEventArgs e)
        {
            Addmark(new Point3D(X_Position, Y_Position, 0));
        }
        private readonly System.Timers.Timer MoveTimer;
        Point3D center;
        readonly Color[] partColors = [Colors.Blue, Colors.WhiteSmoke, Colors.Red];
        int i = 1;
        Model3D model;
        public ICommand Add { get; }
        public ICommand Delete { get; }
        public ICommand Run { get; }
        private float _width;
        public float Pool_width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Pool_width));
                }
            }
        }
        private float _lengtht;
        public float Pool_length
        {
            get => _lengtht;
            set
            {
                if (_lengtht != value)
                {
                    _lengtht = value;
                    OnPropertyChanged(nameof(Pool_length));
                }
            }
        }
        private float _height;
        public float Pool_height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Pool_height));
                }
            }
        }
        private float _grid;
        public float GridSize
        {
            get => _grid;
            set
            {
                if (_grid != value)
                {
                    _grid = value;
                    OnPropertyChanged(nameof(GridSize));
                }
            }
        }
        private double _yaw;
        public double Yaw
        {
            get => _yaw;
            set
            {
                if (_yaw != value)
                {
                    _yaw = value;
                    OnPropertyChanged(nameof(Yaw));
                }
            }
        }
        private double x_axis;
        public double X_Axis
        {
            get { return x_axis; }
            set
            {
                if (x_axis != value)
                {
                    x_axis = value;
                    OnPropertyChanged(nameof(X_Axis));
                }
            }
        }
        private double y_axis;
        public double Y_Axis
        {
            get { return y_axis; }
            set
            {
                if (y_axis != value)
                {
                    y_axis = value;
                    OnPropertyChanged(nameof(Y_Axis));
                }
            }
        }
        private double z_axis;
        public double Z_Axis
        {
            get { return z_axis; }
            set
            {
                if (z_axis != value)
                {
                    z_axis = value;
                    OnPropertyChanged(nameof(Z_Axis));
                }
            }
        }
        private double x_position;
        public double X_Position
        {
            get { return x_position; }
            set
            {
                if (x_position != value)
                {
                    x_position = value;
                    OnPropertyChanged(nameof(X_Position));
                }
            }
        }
        private double y_position;
        public double Y_Position
        {
            get { return y_position; }
            set
            {
                if (y_position != value)
                {
                    y_position = value;
                    OnPropertyChanged(nameof(Y_Position));
                }
            }
        }
        private double z_position;
        public double Z_Position
        {
            get { return z_position; }
            set
            {
                if (z_position != value)
                {
                    z_position = value;
                    OnPropertyChanged(nameof(Z_Position));
                }
            }
        }
        private double _speed;
        public double Speed
        {
            get { return _speed; }
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged(nameof(Speed));
                }
            }
        }
        private Point3D _position;
        public Point3D Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }
        private double _rovdiameter;
        public double ROV_Diameter
        {
            get { return _rovdiameter; }
            set
            {
                if (_rovdiameter != value)
                {
                    _rovdiameter = value;
                    OnPropertyChanged(nameof(ROV_Diameter));
                }
            }
        }
        private StringBuilder _sampletext = new();
        public string SampleText
        {
            get => _sampletext.ToString();
            set
            {
                _sampletext = new StringBuilder(value);
                OnPropertyChanged(nameof(SampleText));
            }
        }
        private int LineCount = 0;
        private Point3D PrevSample = new(0, 0 ,0);
        private double X_bound;
        private double Y_bound;
        private double Z_bound;
        private readonly Queue<Point3D> TrackPoints = new();
        private readonly HashSet<string> Points = [];

        private readonly List<LinesVisual3D> lineList = [];
        private bool AutoMoving = false;
        private double stepDurationSeconds = 2.0;
        Point3D PrevPoint;
        ModelVisual3D FindTubeAtPosition(Point3D position, double tolerance = 0.01)
        {
            foreach (var child in ROV3DView.Children.OfType<ModelVisual3D>())
            {
                if (child.Transform is Transform3DGroup group)
                {
                    foreach (var t in group.Children)
                    {
                        if (t is TranslateTransform3D translate)
                        {
                            if (Math.Abs(translate.OffsetX - position.X) < tolerance &&
                                Math.Abs(translate.OffsetY - position.Y) < tolerance &&
                                Math.Abs(translate.OffsetZ - position.Z) < tolerance)
                            {
                                return child;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public ROV3DModel()
        {
            InitializeComponent();
            Yaw = 0;
            PrevPoint = new(0, 0, 0);
            X_Axis = 0;
            Y_Axis = 0;
            Z_Axis = 0;
            Pool_width = 10;
            Pool_length = 20;
            Pool_height = 6;
            GridSize = 0.5f;
            ROV_Diameter = 0.31;
            X_bound = (Pool_width - ROV_Diameter) *1000;
            Y_bound = (Pool_length - ROV_Diameter) * 1000;
            Z_bound = (Pool_height - 0.3) * 1000;
            Add = new RelayCommand(_ => AddPoint(new Point3D(X_Axis, Y_Axis, Z_Axis)));
            Delete = new RelayCommand(_ => DeletePoint());
            Run = new RelayCommand(_ => StartAutoPath(20));
            MoveTimer = new System.Timers.Timer(100);
            MoveTimer.Elapsed += ManualMove_Tick;
            MoveTimer.AutoReset = true;
            AddPool();
            AddGrid();
            AddROV("ROV2025.obj");
            GetCenter();
            AlgaeType1(new Point3D(6000, 2000, 18), new Vector3D(0, 0, 0), 0);
            AlgaeType2(new Point3D(6500, 2000, 18), new Vector3D(0, 0, 0), 0);
            AlgaeType3(new Point3D(7000, 2000, 18), new Vector3D(0, 0, 0), 0);
            task2();
            task3();
        }
        private void AddPool()
        {
            float width = Pool_width * 1000;
            float length = Pool_length * 1000;
            float height = Pool_height * 1000;
            var builder = new MeshBuilder();
            builder.AddQuad(
                new Point3D(width, 0, 0),
                new Point3D(width, length, 0),
                new Point3D(0, length, 0),
                new Point3D(0, 0, 0)
            );
            builder.AddQuad(
                new Point3D(0, length, 0),
                new Point3D(0, length, height),
                new Point3D(0, 0, height),
                new Point3D(0, 0, 0)
            );
            builder.AddQuad(
                new Point3D(width, length, 0),
                new Point3D(width, length, height),
                new Point3D(0, length, height),
                new Point3D(0, length, 0)
            );
            var model = new GeometryModel3D
            {
                Geometry = builder.ToMesh(),
                BackMaterial = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.MediumBlue)),
                Material = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.MediumBlue))
            };
            ROV3DView.Children.Add(new ModelVisual3D { Content = model });
            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(CreateBox(new Point3D(-1000, 0, 5700), 1001, 21000, 300, Colors.LightGray));
            modelGroup.Children.Add(CreateBox(new Point3D(-1000, 19999, 5700), 11000, 1000, 300, Colors.LightGray));
            ModelVisual3D visual = new() { Content = modelGroup };
            ROV3DView.Children.Add(visual);
        }
        private GeometryModel3D CreateBox(Point3D origin, double width, double height, double depth, Color color)
        {
            MeshGeometry3D mesh = new();
            Point3D p0 = origin;
            Point3D p1 = new(origin.X + width, origin.Y, origin.Z);
            Point3D p2 = new(origin.X + width, origin.Y, origin.Z + depth);
            Point3D p3 = new(origin.X, origin.Y, origin.Z + depth);
            Point3D p4 = new(origin.X, origin.Y + height, origin.Z);
            Point3D p5 = new(origin.X + width, origin.Y + height, origin.Z);
            Point3D p6 = new(origin.X + width, origin.Y + height, origin.Z + depth);
            Point3D p7 = new(origin.X, origin.Y + height, origin.Z + depth);
            Point3D[] pts = [p0, p1, p2, p3, p4, p5, p6, p7];
            void AddFace(int a, int b, int c, int d)
            {
                mesh.Positions.Add(pts[a]);
                mesh.Positions.Add(pts[b]);
                mesh.Positions.Add(pts[c]);
                mesh.Positions.Add(pts[d]);
                int i = mesh.Positions.Count - 4;
                mesh.TriangleIndices.Add(i);
                mesh.TriangleIndices.Add(i + 1);
                mesh.TriangleIndices.Add(i + 2);
                mesh.TriangleIndices.Add(i);
                mesh.TriangleIndices.Add(i + 2);
                mesh.TriangleIndices.Add(i + 3);
            }
            AddFace(0, 1, 2, 3);
            AddFace(0, 4, 5, 1);
            AddFace(3, 2, 6, 7);
            AddFace(0, 3, 7, 4);
            AddFace(1, 5, 6, 2);
            var geometrymodel3d = new GeometryModel3D
            {
                Geometry = mesh,
                Material = new DiffuseMaterial(new SolidColorBrush(color)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
            };
            return geometrymodel3d;
        }
        private void AddGrid()
        {
            var ground = new GridLinesVisual3D
            {
                Width = Pool_length * 1000,
                Length = Pool_width * 1000,
                MinorDistance = GridSize*1000,
                Thickness = 15,
                Center = new Point3D(Pool_width * 500, Pool_length * 500, 3),
                Normal = new Vector3D(0, 0, 1),
                MajorDistance = 0,
                Fill = Brushes.White
            };
            ROV3DView.Children.Add(ground);

            var leftside = new GridLinesVisual3D
            {
                Width = Pool_length * 1000,
                Length = Pool_height * 1000 - 300,
                MinorDistance = GridSize * 1000,
                Thickness = 15,
                Center = new Point3D(1, Pool_length * 500, Pool_height * 500 - 150),
                Normal = new Vector3D(1, 0, 0),
                MajorDistance = 0,
                Fill = Brushes.White
            };
            ROV3DView.Children.Add(leftside);

            var forwardside = new GridLinesVisual3D
            {
                Width = Pool_height * 1000 - 300,
                Length = Pool_width * 1000,
                MinorDistance = GridSize * 1000,
                Thickness = 15,
                Center = new Point3D(Pool_width * 500, Pool_length * 1000 - 1, Pool_height * 500 - 150),
                Normal = new Vector3D(0, 1, 0),
                MajorDistance = 0,
                Fill = Brushes.White
            };
            ROV3DView.Children.Add(forwardside);
        }
        private void AddROV(string modelPath)
        {
            var importer = new ModelImporter
            {
                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue))
            };
            model = importer.Load(modelPath);
            var bounds = model.Bounds;
            center = new Point3D(
                (bounds.X + bounds.SizeX) / 2,
                (bounds.Y + bounds.SizeY) / 2,
                (bounds.Z + bounds.SizeZ) / 2);
            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new TranslateTransform3D(-center.X, -center.Y, -center.Z));
            transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -90)));
            transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 1, 0), -90)));
            transformGroup.Children.Add(new TranslateTransform3D(center.Z, center.Y, center.X));
            model.Transform = transformGroup;
            transformGroup.Children.Add(new TranslateTransform3D(95, -12, -53));
            model.Transform = transformGroup;
            ROV3DView.Children.Add(new ModelVisual3D { Content = model });
            var modelGroup = model as Model3DGroup;
            foreach (var child in modelGroup.Children)
            {
                if (child is GeometryModel3D geometry)
                {
                    geometry.Material = new DiffuseMaterial(new SolidColorBrush(partColors[i % 2]));
                    i++;
                }
            }
            /*Matrix3D matrix = model.Transform.Value;
            double roll = Math.Atan2(matrix.M31, matrix.M33);
            double pitch = Math.Asin(-matrix.M32);
            double yaw = Math.Atan2(matrix.M12, matrix.M22);
            yaw = yaw * 180 / Math.PI;
            pitch = pitch * 180 / Math.PI;
            roll = roll * 180 / Math.PI;*/
        }
        private void AddPoint(Point3D point)
        {
            if (TrackPoints.Count == 0)
            {
                TrackPoints.Enqueue(point);
            }
            else
            {
                string key = GetPointsKey(PrevPoint, point);
                if (Points.Contains(key))
                    return;
                TrackPoints.Enqueue(point);
                var line = new LinesVisual3D
                {
                    Color = Colors.Red,
                    Thickness = 5,
                    Points = [PrevPoint, point]
                };
                Points.Add(key);
                lineList.Add(line);
                ROV3DView.Children.Add(line);
            }
            PrevPoint = point;
        }
        private void DeletePoint()
        {
            if (TrackPoints.Count != 0)
            {
                TrackPoints.Dequeue();
                if (TrackPoints.Count != 0)
                    PrevPoint = TrackPoints.Last();
                else
                    PrevPoint = new(0, 0, 0);
                if (lineList.Count == 0)
                    return;
                var lastLine = lineList[lineList.Count - 1];
                ROV3DView.Children.Remove(lastLine);
                lineList.RemoveAt(lineList.Count - 1);
                var points = lastLine.Points.ToArray();
                string key = GetPointsKey(points[0], points[1]);
                Points.Remove(key);
            }
        }
        private string GetPointsKey(Point3D a, Point3D b)
        {
            var ordered = new[] { a, b }.OrderBy(p => p.X)
                                        .ThenBy(p => p.Y)
                                        .ThenBy(p => p.Z)
                                        .ToArray();
            return $"{ordered[0]}|{ordered[1]}";
        }
        private void Addmark(Point3D point)
        {
            if (point == PrevSample) return;

            var tube = new TubeVisual3D
            {
                Path =
                [
                     new Point3D(0, 0, 0),
                     new Point3D(0, 0, 1000),
                ],
                Diameter = 30,
                Fill = new SolidColorBrush(Colors.Red),
                IsPathClosed = true,
                ThetaDiv = 60
            };
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 700));
            mesh.Positions.Add(new Point3D(-300, -300, 850));
            mesh.Positions.Add(new Point3D(0, 0, 1000));
            mesh.TriangleIndices = [0, 1, 2];
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));
            var backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = backMaterial
            };
            var visual = new ModelVisual3D { Content = model };
            visual.Children.Add(tube);
            visual.Transform = new TranslateTransform3D(point.X, point.Y, 0);
            //ROV3DView.Children.Add(visual);

            var timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
            var newMessage = timestamp + $"Sample identified at {Position.X}, {Position.Y}";
            if (LineCount > 0)
                _sampletext.AppendLine();
            _sampletext.Append(newMessage);
            LineCount++;
            SampleText = SampleText.ToString();
            PrevSample = point;
        }
        private void AlgaeType1(Point3D point, Vector3D vector, double angle)
        {
            var visual = new ModelVisual3D();
            visual.Children.Add(PVC(new(100, 0, 0), new(100, 260, 0), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(100, 260, 0), new(100, 260, 110.5), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(0, 260, 110.5), new(200, 260, 110.5), 30, Colors.LawnGreen));
            visual.Children.Add(Cap(new(100, 0, 0), new(0, 1, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(0, 260, 110.5), new(1, 0, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(200, 260, 110.5), new(1, 0, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(100, 260, 0), 14.9, Colors.LawnGreen));
            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(vector, angle)));
            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
            visual.Transform = transformGroup;
            ROV3DView.Children.Add(visual);
        }
        private void AlgaeType2(Point3D point, Vector3D vector, double angle)
        {
            var visual = new ModelVisual3D();
            visual.Children.Add(PVC(new(45, 321, 100), new(91, 321, 100), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(91, 321, 100), new(91, 0, 100), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(91, 0, 100), new(0, 0, 100), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(0, 0, 100), new(0, 100, 100), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(0, 100, 0), new(0, 100, 200), 30, Colors.LawnGreen));
            visual.Children.Add(Cap(new(0, 100, 0), new(0, 0, 1), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(0, 100, 200), new(0, 0, 1), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(45, 321, 100), new(1, 0, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(91, 321, 100), 14.9, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(91, 0, 100), 14.9, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(0, 0, 100), 14.9, Colors.LawnGreen));
            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(vector, angle)));
            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
            visual.Transform = transformGroup;
            ROV3DView.Children.Add(visual);
        }
        private void AlgaeType3(Point3D point, Vector3D vector, double angle)
        {
            var visual = new ModelVisual3D();
            visual.Children.Add(PVC(new(100, 0, 321), new(100, 45, 321), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(100, 45, 321), new(100, 45, 0), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(100, 45, 0), new(100, 91, 0), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(100, 91, 0), new(100, 91, 110), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(200, 91, 110), new(0, 91, 110), 30, Colors.LawnGreen));
            visual.Children.Add(PVC(new(0, 91, 110), new(0, 91, 0), 30, Colors.LawnGreen));
            visual.Children.Add(Cap(new(100, 0, 321), new(0, 1, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(200, 91, 110), new(1, 0, 0), 15, Colors.LawnGreen));
            visual.Children.Add(Cap(new(0, 91, 0), new(0, 0, 1), 15, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(100, 45, 321), 14.9, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(100, 45, 0), 14.9, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(100, 91, 0), 14.9, Colors.LawnGreen));
            visual.Children.Add(Sphere(new(0, 91, 110), 14.9, Colors.LawnGreen));
            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(vector, angle)));
            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
            visual.Transform = transformGroup;
            ROV3DView.Children.Add(visual);
        }
        private void task2()
        {
            var visual = new ModelVisual3D();
            
            visual.Children.Add(PVC(new Point3D(0, 0, 0), new Point3D(0, 0, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(0, 0, 91), new Point3D(1382, 0, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(1382, 0, 91), new Point3D(1382, 0, 0), 30, Colors.WhiteSmoke));
            visual.Children.Add( PVC(new Point3D(1382, 0, 0), new Point3D(0, 0, 0),30, Colors.WhiteSmoke));

            visual.Children.Add(PVC(new Point3D(0, 1200, 0), new Point3D(0, 1200, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(0, 1200, 91), new Point3D(1382, 1200, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(1382, 1200, 91), new Point3D(1382, 1200, 0), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(1382, 1200, 0), new Point3D(0, 1200, 0), 30, Colors.WhiteSmoke));
                    
            visual.Children.Add(PVC(new Point3D(91, 0, 0), new Point3D(91, 1200, 0), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(91, 0, 91), new Point3D(91, 1200, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(1291, 0, 0), new Point3D(1291, 1200, 0), 30, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(1291, 0, 91), new Point3D(1291, 1200, 91), 30, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(0, 0, 0), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(0, 0, 91), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(1382, 0, 0), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(1382, 0, 91), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(0, 1200, 0), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(0, 1200, 91), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(1382, 1200, 0), 14.9, Colors.WhiteSmoke));
            visual.Children.Add(Sphere(new(1382, 1200, 91), 14.9, Colors.WhiteSmoke));
            var importer = new ModelImporter
            {
                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.WhiteSmoke))
            };
            var task = importer.Load("Task2.obj");
            var group = new Transform3DGroup();
            group.Children.Add(new ScaleTransform3D(100, 100, 100));
            group.Children.Add(new TranslateTransform3D(691, 600, 45.5));
            task.Transform = group;
            visual.Children.Add(new ModelVisual3D { Content = task });
            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new TranslateTransform3D(5000, 10000, 16));
            visual.Transform = transformGroup;
            ROV3DView.Children.Add(visual);
        }
        private void task3()
        {
            var visual = new ModelVisual3D();
            visual.Children.Add(PVC(new Point3D(0, 0, 0), new Point3D(0, 0, 200), 50, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(0, 0, 95), new Point3D(0, 0, 105), 50.5, Colors.Black));

            visual.Children.Add(PVC(new Point3D(75, 0, 0), new Point3D(75, 0, 200), 50, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(75, 0, 105), new Point3D(75, 0, 115), 50.5, Colors.Black));
            visual.Children.Add(PVC(new Point3D(75, 0, 85), new Point3D(75, 0, 95), 50.5, Colors.Black));

            visual.Children.Add(PVC(new Point3D(150, 0, 0), new Point3D(150, 0, 200), 50, Colors.WhiteSmoke));
            visual.Children.Add(PVC(new Point3D(150, 0, 95), new Point3D(150, 0, 105), 50.5, Colors.Black));
            visual.Children.Add(PVC(new Point3D(150, 0, 120), new Point3D(150, 0, 130), 50.5, Colors.Black));
            visual.Children.Add(PVC(new Point3D(150, 0, 75), new Point3D(150, 0, 85), 50.5, Colors.Black));

            Transform3DGroup transformGroup = new();
            transformGroup.Children.Add(new TranslateTransform3D(5000, 7500, 16));
            visual.Transform = transformGroup;
            ROV3DView.Children.Add(visual);
        }
        public TubeVisual3D PVC(Point3D p1, Point3D p2, double dia, Color col)
        {
            var tube = new TubeVisual3D
            {
                Path = [p1, p2],
                Diameter = dia,
                Fill = new SolidColorBrush(col),
                Material = MaterialHelper.CreateMaterial(col),
                BackMaterial = MaterialHelper.CreateMaterial(col),
                ThetaDiv = 60,
            };
            return tube;
        }
        private ModelVisual3D Cap(Point3D center, Vector3D normal, double radius, Color color)
        {
            var builder = new MeshBuilder();
            builder.AddCone(center, normal, radius, radius, 1, true, true, 60);
            var mesh = builder.ToMesh();
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = MaterialHelper.CreateMaterial(color),
                BackMaterial = MaterialHelper.CreateMaterial(color)
            };
            var modelvisual = new ModelVisual3D { Content = model };
            return modelvisual;
        }
        private SphereVisual3D Sphere(Point3D p, double rad, Color col)
        {
            var sphere = new SphereVisual3D
            {
                Center = p,
                Radius = rad,
                Fill = new SolidColorBrush(col),
                Material = MaterialHelper.CreateMaterial(col),
                BackMaterial = MaterialHelper.CreateMaterial(col),
                ThetaDiv = 60

            };
          
            return sphere;
        }
        public void StartAutoPath(double durationPerStep)
        {
            stepDurationSeconds = durationPerStep;
            if (!AutoMoving)
                AutoMove();
        }
        private void AutoMove()
        {
            if (TrackPoints.Count == 0)
            {
                AutoMoving = false;
                return;
            }
            AutoMoving = true;
            var nextPoint = TrackPoints.Dequeue();
            Move(nextPoint, 1, AutoMove);
        }
        private void ManualMove_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ManualMove(MoveDir);
            });
        }
        private void ManualMove(int dir)
        {
            GetCenter();
            var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();           
            double X = 0;
            double Y = 0;
            switch (dir)
            {
                case 0:
                    X = Math.Cos((90 + Yaw) * Math.PI / 180.0) * 50;
                    Y = Math.Sin((90 + Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 1:
                    X = Math.Cos((-90 + Yaw) * Math.PI / 180.0) * 50;
                    Y = Math.Sin((-90 + Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 2:
                    X = Math.Cos((Yaw) * Math.PI / 180.0) * 50;
                    Y = Math.Sin((Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 3:
                    X = -Math.Cos((Yaw) * Math.PI / 180.0) * 50;
                    Y = -Math.Sin((Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 4:
                    X = Math.Cos((45 + Yaw) * Math.PI / 180.0) * 50;
                    Y = Math.Sin((45 + Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 5:
                    X = -Math.Cos((45 - Yaw) * Math.PI / 180.0) * 50;
                    Y = Math.Sin((45 - Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 6:
                    X = Math.Cos((45 - Yaw) * Math.PI / 180.0) * 50;
                    Y = -Math.Sin((45 - Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 7:
                    X = -Math.Cos((45 + Yaw) * Math.PI / 180.0) * 50;
                    Y = -Math.Sin((45 + Yaw) * Math.PI / 180.0) * 50;
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.2, GetCenter);
                    break;
                case 8:
                    transformGroup.Children.Add(new RotateTransform3D(
                         new AxisAngleRotation3D(new Vector3D(0, 0, 1), -10), new Point3D(X_Position, Y_Position, Z_Position)));
                    if (Yaw <= -179)
                        Yaw = 170;
                    else
                        Yaw -= 10;
                    break;
                case 9:
                    transformGroup.Children.Add(new RotateTransform3D(
                        new AxisAngleRotation3D(new Vector3D(0, 0, 1), +10), new Point3D(X_Position, Y_Position, Z_Position)));
                    if (Yaw >= 179)
                        Yaw = -170;
                    else
                        Yaw += 10;
                    break;
                case 10:
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position + 100), 0.2, GetCenter);
                    break;
                case 11:
                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position - 100), 0.2, GetCenter);
                    break;
                default:
                    break;
            }
        }
        private void Move(Point3D targetPosition, double durationSeconds, Action? onComplete)
        {
            GetCenter();
            var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();
            model.Transform = transformGroup;
            if (AutoMoving)
                RotateROV(targetPosition, new Point3D(X_Position, Y_Position, Z_Position));
            Vector3D movement = targetPosition - new Point3D(X_Position, Y_Position, Z_Position);
            if ((X_Position + movement.X) > X_bound || (X_Position + movement.X) < 310)
            {
                movement.X = movement.X > 0 ? (X_bound - X_Position) : (310 - X_Position);
            }
            if ((Y_Position + movement.Y) > Y_bound || (Y_Position + movement.Y) < 310)
            {
                movement.Y = movement.Y > 0 ? Y_bound - Y_Position : 310 - Y_Position;
            }
            if ((Z_Position + movement.Z) > Z_bound || (Z_Position + movement.Z) < 182)
            {
                movement.Z = movement.Z > 0 ? Z_bound - Z_Position : 182 - Z_Position;
            }
            if (movement.LengthSquared > 0)
            {
                var translateTransform = new TranslateTransform3D();
                transformGroup.Children.Add(translateTransform);
                var animationX = new DoubleAnimation(
                    0, movement.X ,
                    TimeSpan.FromSeconds(durationSeconds));
                var animationY = new DoubleAnimation(
                    0, movement.Y ,
                    TimeSpan.FromSeconds(durationSeconds));
                var animationZ = new DoubleAnimation(
                    0, movement.Z,
                    TimeSpan.FromSeconds(durationSeconds));

                int completedCount = 0;
                EventHandler? onAnimationCompleted = null;
                onAnimationCompleted = (s, e) =>
                {
                    completedCount++;
                    if (completedCount == 3)
                        onComplete?.Invoke();
                };
                animationX.Completed += onAnimationCompleted;
                animationY.Completed += onAnimationCompleted;
                animationZ.Completed += onAnimationCompleted;
                translateTransform.BeginAnimation(TranslateTransform3D.OffsetXProperty, animationX);
                translateTransform.BeginAnimation(TranslateTransform3D.OffsetYProperty, animationY);
                translateTransform.BeginAnimation(TranslateTransform3D.OffsetZProperty, animationZ);
            }
        }
        private void RotateROV(Point3D targetPosition, Point3D currentPosition)
        {var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();
            model.Transform = transformGroup;
            double angle;
            Point3D center = new(X_Position, Y_Position, Z_Position);
            if (targetPosition.X < 310 && x_position <= 310)
            {
                transformGroup.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
                angle = 90;// targetPosition.X < 310 ? 90 : -90;
                transformGroup.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle), center));
                Yaw = angle;
                return;
            }
            Vector3D movement = targetPosition - currentPosition;
            if (targetPosition.Y > (currentPosition.Y ))
            {
                transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
                angle = Math.Atan2(movement.X, movement.Y) * (180 / Math.PI);
                angle = (targetPosition.X > currentPosition.X) ? -angle : angle;
                Yaw = angle;
            }
            else if (targetPosition.Y < (currentPosition.Y ))
            {
                transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -180 - Yaw), center));
                angle = Math.Atan2(Math.Abs(movement.X), Math.Abs(movement.Y)) * (180 / Math.PI);
                angle = (targetPosition.X > currentPosition.X) ? angle : -angle;
                Yaw = (currentPosition.X > targetPosition.X) ? 180 + angle : -180 + angle;
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
                angle = (targetPosition.X > currentPosition.X) ? (Math.Abs(Yaw) <= 90) ? -90 : 90 : (Math.Abs(Yaw) <= 90) ? 90 : -90;
                Yaw = angle;
            }
            transformGroup.Children.Add(new RotateTransform3D(
               new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle), center));
        }
        private void GetCenter()
        {
            var bounds = model.Bounds;
            X_Position = bounds.X + bounds.SizeX / 2;
            Y_Position = bounds.Y + bounds.SizeY / 2;
            Z_Position = bounds.Z + bounds.SizeZ / 2;
            var pos = Position;
            pos.X = Math.Round(X_Position, 2);
            pos.Y = Math.Round(Y_Position, 2);
            pos.Z = Math.Round(Z_Position, 2);
            Position = pos;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            MoveTimer?.Stop();
            MoveTimer?.Dispose();
        }
    }
}