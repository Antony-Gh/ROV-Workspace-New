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
    /// Interaction logic for SettingsButton.xaml
    /// </summary>
    public partial class SettingsButton : UserControl
    {
        public static readonly DependencyProperty SettingsCommandProperty =
           DependencyProperty.Register("SettingsCommand", typeof(ICommand),
               typeof(SettingsButton), new PropertyMetadata(null));
        public ICommand SettingsCommand
        {
            get { return (ICommand)GetValue(SettingsCommandProperty); }
            set { SetValue(SettingsCommandProperty, value); }
        }
        public event RoutedEventHandler Click;
        public SettingsButton()
        {
            InitializeComponent();
            part1.RenderTransformOrigin = new Point(0.5, 0.5);
            part2.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        public void DOWN()
        {
            part1.RenderTransform = new ScaleTransform(.93, 0.93);
            part2.RenderTransform = new ScaleTransform(.93, 0.93);
            pressArea.Height = 45;
            pressArea.Width = 45;
        }
        public void UP()
        {
            part1.RenderTransform = new ScaleTransform(1, 1);
            part2.RenderTransform = new ScaleTransform(1, 1);
            pressArea.Height = 47;

        }
        private void MouseENTER(object sender, MouseEventArgs e)
        {
            pressArea.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF001D45"));
            pressArea.Height = 47;
            pressArea.Width = 47;
        }
        private void MouseLEAVE(object sender, MouseEventArgs e)
        {
            UP();
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
            SettingsCommand?.Execute(null);
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
    }
}
