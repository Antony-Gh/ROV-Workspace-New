using System;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ROV_GUI_Control.ViewModels
{
    public class RotationController
    {
        private readonly AxisAngleRotation3D _rotationX;
        private readonly AxisAngleRotation3D _rotationY;
        private readonly AxisAngleRotation3D _rotationZ;
        private readonly Dispatcher _dispatcher;

        public RotationController(Model3D model, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            _rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
            _rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            _rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);

            var group = new Transform3DGroup();
            group.Children.Add(new RotateTransform3D(_rotationX));
            group.Children.Add(new RotateTransform3D(_rotationY));
            group.Children.Add(new RotateTransform3D(_rotationZ));

            model.Transform = group;
        }

        public void UpdateRotationAsync(double pitch, double yaw, double roll)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _rotationX.Angle = pitch;
                _rotationY.Angle = yaw;
                _rotationZ.Angle = roll;
            }));
        }

    }

}
