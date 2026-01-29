using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for ConnectButton.xaml
    /// </summary>
    public partial class ConnectButton : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ConnectStateProperty =
           DependencyProperty.Register("ConnectState", typeof(bool), typeof(ConnectButton),
           new PropertyMetadata(false, OnConnectStateChanged));
        public bool ConnectState
        {
            get { return (bool)GetValue(ConnectStateProperty); }
            set { SetValue(ConnectStateProperty, value); }
        }

        public static readonly DependencyProperty ConnectCommandProperty =
            DependencyProperty.Register("ConnectCommand", typeof(ICommand),
                typeof(ConnectButton), new PropertyMetadata(null));
        public ICommand ConnectCommand
        {
            get { return (ICommand)GetValue(ConnectCommandProperty); }
            set { SetValue(ConnectCommandProperty, value); }
        }
        private static void OnConnectStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectButton connectButton)
            {
                if (connectButton.ConnectState)
                    connectButton.ON();
                else
                    connectButton.OFF();
            }
        }
        public event RoutedEventHandler Click;
        private Color _color =  (Color)ColorConverter.ConvertFromString("#FFFF4D4D");
        public Color color
        {
            get=> _color;
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
        public ConnectButton()
        {
            InitializeComponent();
            ConnectState = false;
            part1.RenderTransformOrigin = new Point(0.5, 0.5);
            part2.RenderTransformOrigin = new Point(0.5, 0.5);
            part3.RenderTransformOrigin = new Point(0.5, 0.5);
            part4.RenderTransformOrigin = new Point(0.5, 0.5);
            part5.RenderTransformOrigin = new Point(0.5, 0.5);
            part6.RenderTransformOrigin = new Point(0.5, 0.5);
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
            part4.RenderTransform = new ScaleTransform(.93, 0.93);
            part5.RenderTransform = new ScaleTransform(.93, 0.93);
            part6.RenderTransform = new ScaleTransform(.93, 0.93);
            pressArea.Height = 45;
            pressArea.Width = 45;
        }
        public void UP()
        {
            part1.RenderTransform = new ScaleTransform(1, 1);
            part2.RenderTransform = new ScaleTransform(1, 1);
            part3.RenderTransform = new ScaleTransform(1, 1);
            part4.RenderTransform = new ScaleTransform(1, 1);
            part5.RenderTransform = new ScaleTransform(1, 1);
            part6.RenderTransform = new ScaleTransform(1, 1);
            pressArea.Height = 47;
            pressArea.Width = 47;
        }
        private void MouseENTER(object sender, MouseEventArgs e)
        {
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 47;
            pressArea.Width = 47;
            //part3.Data = Geometry.Parse("M 8.5,30 A 21.5,21.5 0 1 0 8.5,29.9");
        }
        private void MouseLEAVE(object sender, MouseEventArgs e)
        {
            UP();
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 46;
            pressArea.Width = 46;
            //part3.Data = Geometry.Parse("M 9,30 A 21,21 0 1 0 9,29.9");
            BackgroundBrush.Color = Colors.Transparent;
        }
        private void ClickAnimation_Completed(object sender, EventArgs e)
        {
            BackgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            BackgroundBrush.Color = Colors.Transparent;
        }
        private void MouseLeftButtonUP(object sender, MouseButtonEventArgs e)
        {
            ConnectCommand?.Execute(null);
            UP();
        }
        private void MouseLeftButtonDOWN(object sender, MouseButtonEventArgs e)
        {
            DOWN();
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