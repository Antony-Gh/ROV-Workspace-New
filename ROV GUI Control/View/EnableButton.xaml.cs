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
    /// Interaction logic for EnableButton.xaml
    /// </summary>
    public partial class EnableButton : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty EnableStateProperty =
           DependencyProperty.Register("EnableState", typeof(bool), typeof(EnableButton),
           new PropertyMetadata(false, OnEnableStateChanged));
        public bool EnableState
        {
            get { return (bool)GetValue(EnableStateProperty); }
            set { SetValue(EnableStateProperty, value); }
        }

        public static readonly DependencyProperty EnableCommandProperty =
            DependencyProperty.Register("EnableCommand", typeof(ICommand),
                typeof(EnableButton), new PropertyMetadata(null));
        public ICommand EnableCommand
        {
            get { return (ICommand)GetValue(EnableCommandProperty); }
            set { SetValue(EnableCommandProperty, value); }
        }
        private static void OnEnableStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnableButton enableButton)
            {
                if (enableButton.EnableState)
                    enableButton.ON();
                else
                    enableButton.OFF();
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
        public EnableButton()
        {
            InitializeComponent();
            EnableState = false;
            part1.RenderTransformOrigin = new Point(0.5, 0.5);
            part2.RenderTransformOrigin = new Point(0.5, 0.5);
            part3.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        public void ON()
        {
            color = (Color)ColorConverter.ConvertFromString("#FF10FF05");
            scolor = (Color)ColorConverter.ConvertFromString("#FF0D4D0A");
            part3.Content = "ENA";
        }
        public void OFF()
        {
            color = (Color)ColorConverter.ConvertFromString("#FFFF4D4D");
            scolor = (Color)ColorConverter.ConvertFromString("#FFF32626");
            part3.Content = "DSA";
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
        }
        private void MouseLEAVE(object sender, MouseEventArgs e)
        {
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 46;
            pressArea.Width = 46;
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
            EnableCommand?.Execute(null);
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
