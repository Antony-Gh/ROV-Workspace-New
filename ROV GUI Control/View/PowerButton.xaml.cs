using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
namespace ROV_GUI_Control.View
{
    public partial class PowerButton : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty PowerStateProperty =
           DependencyProperty.Register("PowerState", typeof(bool), typeof(PowerButton),
           new PropertyMetadata(false, OnPowerStateChanged));
        public bool PowerState
        {
            get { return (bool)GetValue(PowerStateProperty); }
            set { SetValue(PowerStateProperty, value); }
        }
        public static readonly DependencyProperty PowerCommandProperty =
            DependencyProperty.Register("PowerCommand", typeof(ICommand),
                typeof(PowerButton), new PropertyMetadata(null));
        public ICommand PowerCommand
        {
            get { return (ICommand)GetValue(PowerCommandProperty); }
            set { SetValue(PowerCommandProperty, value); }
        }
        private static void OnPowerStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PowerButton powerButton)
            {
                if (powerButton.PowerState)
                    powerButton.ON();
                else
                    powerButton.OFF();
            }
        }
        public event RoutedEventHandler Click;
        private Color _color = (Color)ColorConverter.ConvertFromString("#FFFF4D4D");
        public Color color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(color));
            }
        }
        private Color _scolor = (Color)ColorConverter.ConvertFromString("#FFF32626");
        public Color scolor
        {
            get => _scolor;
            set
            {
                _scolor = value;
                OnPropertyChanged(nameof(scolor));
            }
        }
        public PowerButton()
        {
            InitializeComponent();
            PowerState = false;
            part1.RenderTransformOrigin = new Point(0.5, 0.5);
            part2.RenderTransformOrigin = new Point(0.5, 0.5);
            part3.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        public void ON()
        {
            color = (Color)ColorConverter.ConvertFromString("#FF10FF05");
            scolor = (Color)ColorConverter.ConvertFromString("#FF0D4D0A");
        }
        public void OFF()
        {
            color = (Color)ColorConverter.ConvertFromString("#FFFF4D4D");
            scolor = (Color)ColorConverter.ConvertFromString("#FFF32626");
        }
        public void DOWN()
        {
            part1.RenderTransform = new ScaleTransform(.93, 0.93);
            part2.RenderTransform = new ScaleTransform(.93, 0.93);
            part3.RenderTransform = new ScaleTransform(.93, 0.93);
            pressArea.Height = 45;
            pressArea.Width = 45;
        }
        public void UP()
        {
            part1.RenderTransform = new ScaleTransform(1, 1);
            part2.RenderTransform = new ScaleTransform(1, 1);
            part3.RenderTransform = new ScaleTransform(1, 1);
            pressArea.Height = 47;
            pressArea.Width = 47;
        }
        private void MouseENTER(object sender, MouseEventArgs e)
        {
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 47;
            pressArea.Width = 47;
            part3.Data = Geometry.Parse("M 8.5,30 A 21.5,21.5 0 1 0 8.5,29.9");
        }
        private void MouseLEAVE(object sender, MouseEventArgs e)
        {
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 46;
            pressArea.Width = 46;
            part3.Data = Geometry.Parse("M 9,30 A 21,21 0 1 0 9,29.9");
            BackgroundBrush.Color = Colors.Transparent;
        }
        private void ClickAnimation_Completed(object sender, EventArgs e)
        {
            BackgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            BackgroundBrush.Color = Colors.Transparent;
        }
        private void MouseLeftButtonUP(object sender, MouseButtonEventArgs e)
        {
            UP();
        }
        private void MouseLeftButtonDOWN(object sender, MouseButtonEventArgs e)
        {
            DOWN();
            PowerCommand?.Execute(null);
            Storyboard clickAnimation = (Storyboard)FindResource("ClickAnimation");
            BackgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            BackgroundBrush.Color = Colors.Transparent;
            clickAnimation.Stop();
            clickAnimation.Begin();
            Click?.Invoke(this, new RoutedEventArgs());
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}