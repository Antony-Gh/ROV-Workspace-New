using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace ROV_GUI_Control.ViewModels
{
    public class _3DViewModel : INotifyPropertyChanged
    {
        private readonly AxisAngleRotation3D rotation;
        private readonly RotateTransform3D rotateTransform;
        private readonly Transform3DGroup transformGroup;

        private Model3D _model;
        private bool _isRotating;

        private ModelVisual3D _modelVisual;
        public ModelVisual3D ModelVisual
        {
            get => _modelVisual;
            private set
            {
                _modelVisual = value;
                OnPropertyChanged(nameof(ModelVisual));
            }
        }

        public _3DViewModel()
        {
            rotation = new(new Vector3D(0, 1, 0), 0);
            rotateTransform = new(rotation);
            transformGroup = new();


            //transformGroup.Children.Add(rotateTransform);

            // Start async load
            _ = LoadModelAsync();

            // Start rotation render loop
            CompositionTarget.Rendering += OnRendering;
            _isRotating = true;
        }

        private async Task LoadModelAsync()
        {
            try
            {
                var importer = new ModelImporter();
                await Task.Delay(100);
                Model3D model = null;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    model = importer.Load("ROV2025.obj");

                    var bounds = model.Bounds;
                    var center = new Point3D(
                        bounds.X + bounds.SizeX / 2,
                        bounds.Y + bounds.SizeY / 2,
                        bounds.Z + bounds.SizeZ / 2
                    );
                    rotateTransform.CenterX = center.X;
                    rotateTransform.CenterY = center.Y;
                    rotateTransform.CenterZ = center.Z;
                    transformGroup.Children.Add(rotateTransform);
                    model.Transform = transformGroup;
                    
                    if (model is Model3DGroup modelGroup)
                    {
                        Color[] partColors = [Colors.Red, Colors.WhiteSmoke, Colors.Blue];
                        int i = 0;
                        foreach (var child in modelGroup.Children)
                        {
                            if (child is GeometryModel3D geometry)
                            {
                                geometry.Material = new DiffuseMaterial(
                                    new SolidColorBrush(partColors[i % partColors.Length]));
                                i++;
                            }
                        }
                    }
                    _model = model;
                    ModelVisual = new ModelVisual3D { Content = _model };
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load 3D model: {ex.Message}");
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            /*if (!_isRotating) 
                return;
            rotation.Angle += 3;
            if (rotation.Angle > 360) 
                rotation.Angle = 0;*/
        }

        public void StartRotation() => _isRotating = true;
        public void StopRotation() => _isRotating = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
