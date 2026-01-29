using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for Compass.xaml
    /// </summary>
    public partial class Compass : UserControl
    {
        public static readonly DependencyProperty CompValueProperty = DependencyProperty.Register("CompValue", typeof(double), typeof(Compass),
          new PropertyMetadata((double)0.00, OnCompValuePropertyChanged));
        public double CompValue
        {
            get { return (double)GetValue(CompValueProperty); }
            set { SetValue(CompValueProperty, value); }
        }
        private static void OnCompValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Compass f = d as Compass;
            f.OnCompValueChanged(e);
        }
        public virtual void OnCompValueChanged(DependencyPropertyChangedEventArgs e)
        {
            AniCompScale(CompValue);
        }
        public Compass()
        {
            InitializeComponent();
            DrawMinScale();
            DrawAngleScale();
        }
        private void DrawMinScale()
        {
            Double ScaleTicktAngle = 22.5;
            Double ScaleTicksValue = 22.5;
            Double minvalue = 0;
            for (Double i = -90; i < 270; i += ScaleTicktAngle)
            {
                Double i_radian = (i * Math.PI) / 180;
                if (minvalue > 360)
                    break;
                if ( i % 22.5 == 0)
                {
                    Rectangle ScaleTickRect = new()
                    {
                        Height = 1,
                        Width = 2.5,
                        Fill = new SolidColorBrush(Colors.WhiteSmoke)
                    };
                    System.Windows.Point p = new(0.5, 0.5);
                    ScaleTickRect.RenderTransformOrigin = p;
                    ScaleTickRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    ScaleTickRect.VerticalAlignment = VerticalAlignment.Center;
                    TransformGroup ScaleTickRectTFG = new();
                    RotateTransform ScaleTickRectRT = new()
                    {
                        Angle = i
                    };
                    ScaleTickRectTFG.Children.Add(ScaleTickRectRT);
                    TranslateTransform ScaleTickRectTTF = new()
                    {
                        X = (int)((28) * Math.Cos(i_radian)),
                        Y = (int)((28) * Math.Sin(i_radian))
                    };
                    ScaleTickRectTFG.Children.Add(ScaleTickRectTTF);
                    ScaleTickRect.RenderTransform = ScaleTickRectTFG;
                    range_container.Children.Add(ScaleTickRect);
                }
                minvalue += ScaleTicksValue;
            }

            }
        private void DrawAngleScale()
        {
            Double ScaleTicktAngle = 10;
            Double ScaleTicksValue = 10;
            Double minvalue = 0;
            for (Double i = -90; i < 270; i += ScaleTicktAngle)
            {
                Double i_radian = (i * Math.PI) / 180;
                if (minvalue > 360)
                    break;
                if (i == 0 || i % 30 == 0)
                {
                    TextBlock ScaleTB = new();
                    TX(ScaleTB);
                    ScaleTB.Height = 10;
                    ScaleTB.Width = 16;
                    ScaleTB.FontSize = 8;
                    ScaleTB.VerticalAlignment = VerticalAlignment.Center;
                    ScaleTB.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    TransformGroup ScaleTickRectTFG = new();
                    TranslateTransform ScaleTickRectTTF = new();
                    RotateTransform ScaleTickRectRT = new();
                    ScaleTickRectTTF.X = (int)((63) * Math.Cos(i_radian));
                    ScaleTickRectTTF.Y = (int)((63) * Math.Sin(i_radian));
                    ScaleTB.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTickRectRT.Angle = i - 270;
                    ScaleTickRectTFG.Children.Add(ScaleTickRectRT);
                    ScaleTickRectTFG.Children.Add(ScaleTickRectTTF);
                    ScaleTB.RenderTransform = ScaleTickRectTFG;
                    ScaleTB.Text = minvalue.ToString();
                    range_container.Children.Add(ScaleTB);
                }
                else
                {
                    Rectangle ScaleTickRect = new()
                    {
                        Height = 1.5,
                        Width = 7,
                        Fill = new SolidColorBrush(Colors.WhiteSmoke)
                    };
                    System.Windows.Point p = new(0.5, 0.5);
                    ScaleTickRect.RenderTransformOrigin = p;
                    ScaleTickRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    ScaleTickRect.VerticalAlignment = VerticalAlignment.Center;
                    TransformGroup ScaleTickRectTFG = new();
                    RotateTransform ScaleTickRectRT = new()
                    {
                        Angle = i
                    };
                    ScaleTickRectTFG.Children.Add(ScaleTickRectRT);
                    TranslateTransform ScaleTickRectTTF = new()
                    {
                        X = (int)((63) * Math.Cos(i_radian)),
                        Y = (int)((63) * Math.Sin(i_radian))
                    };
                    ScaleTickRectTFG.Children.Add(ScaleTickRectTTF);
                    ScaleTickRect.RenderTransform = ScaleTickRectTFG;
                    range_container.Children.Add(ScaleTickRect);
                }
                minvalue += ScaleTicksValue;
            }
        }
        private void TX(TextBlock tx)
        {
            tx.Width = 14;
            tx.Height = 10;
            tx.FontFamily = new FontFamily("Courier New");
            tx.FontWeight = FontWeights.Bold;
            tx.Foreground = new SolidColorBrush(Colors.White);
            tx.TextAlignment = TextAlignment.Center;
            tx.VerticalAlignment = VerticalAlignment.Top;
            tx.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        }
        protected void AniCompScale(Double value)
        {
            RotateTransform CompAngleRT = new()
            {
                Angle = value
            };
            range_container.RenderTransform = CompAngleRT;
        }
    }
}
