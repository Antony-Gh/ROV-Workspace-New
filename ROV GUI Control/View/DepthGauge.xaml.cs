using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FontFamily = System.Windows.Media.FontFamily;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for DepthGauge.xaml
    /// </summary>
    /// 
    public partial class DepthGauge : UserControl
    {
        private double _lastDrawnDepth = 0;

        public static readonly DependencyProperty DepthValueProperty = DependencyProperty.Register("DepthValue", typeof(double), typeof(DepthGauge),
           new PropertyMetadata((double)0.00, OnDepthValuePropertyChanged));
        public double DepthValue
        {
            get { return (double)GetValue(DepthValueProperty); }
            set { SetValue(DepthValueProperty, value); }
        }
        private static void OnDepthValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DepthGauge f = d as DepthGauge;
            f.OnDepthValueChanged(e);
        }
        public virtual void OnDepthValueChanged(DependencyPropertyChangedEventArgs e)
        {
            //double newDepth = (double)e.NewValue;
            // Only update if change is significant (e.g. 0.1 meters)
           // if (Math.Abs(newDepth - _lastDrawnDepth) > 0.1)
            
                _lastDrawnDepth = DepthValue;
            _ = DrawDepthScale();   
        }
        readonly double MaxDepth = 650;
        readonly double MinDepth = 0;
        readonly double MajorDivisionsCount = 65;
        readonly double MinorDivisionsCount = 2;
        public int ScaleValuePrecision = 1;
        public double ScaleFontSize = 8;
        public DepthGauge()
        {
            InitializeComponent();
            _ = DrawDepthScale();
            DrawMillieScale();
            //DataContext = new GaugeDepthViewModel();
           // this.SetBinding(DepthValueProperty, new Binding("Depth_Value"));
        }
        private Task DrawDepthScale()
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DepthScale.Children.Clear();


                double min = 0;
            double max = DepthValue + 25;
            double minvalue = 0;
            int start = (int)(DepthValue - 25) / 10;
            if (DepthValue > 25)
            {
                min = start * 10;
                minvalue = start * 10;
            }
            if (max > 650)
                max = 650;
            min = min * 2.5 + 120;
            max = max * 2.5 + 120;
            double SLLen = 1;
            double XTB = 1;
            double MinorTickLineLen = 50;
            Double MajorTicksStep = 1625 / MajorDivisionsCount;
            Double MajorTicksValue = (MaxDepth - MinDepth) / MajorDivisionsCount;
            MajorTicksValue = Math.Round(MajorTicksValue, ScaleValuePrecision);
                Line zerotickLine = new()
                {
                X1 = 0,
                Y1 = 120,
                X2 = 240,
                Y2 = 120,
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 2
            };
                DepthScale.Children.Add(zerotickLine);
                for (Double i = min; i <= max; i += MajorTicksStep)
            {
                double dx = DepthValue * 2.5 + 120;
                if (i <= dx)
                { SLLen = 70 - (dx - i) * 0.4; XTB = 67 + (dx - i) * 0.28; ScaleFontSize = 8 - (dx - i) * 0.032; }
                else if (i > dx)
                { SLLen = 70 - (i - dx) * 0.4; XTB = 67 + (i - dx) * 0.28; ScaleFontSize = 8 - (i - dx) * 0.032; }
                Line MajorTickLine = new()
                {
                    X1 = (240 - SLLen) / 2,
                    Y1 = i,
                    X2 = (240 - SLLen) / 2 + SLLen,
                    Y2 = i,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2
                };
                TranslateTransform RScalevalue = new();
                TranslateTransform LScalevalue = new();
                RScalevalue.X = XTB + SLLen + 22;
                RScalevalue.Y = i - 5;
                LScalevalue.X = XTB;
                LScalevalue.Y = i - 5;
                TextBlock RTB = new(); TX(RTB);
                TextBlock LTB = new(); TX(LTB);
                RTB.FontSize = ScaleFontSize;
                LTB.FontSize = ScaleFontSize;

                if (Math.Round(minvalue, ScaleValuePrecision) <= Math.Round(MaxDepth, ScaleValuePrecision))
                {
                    minvalue = Math.Round(minvalue, ScaleValuePrecision);
                    RTB.Text = minvalue.ToString();
                    LTB.Text = minvalue.ToString();
                    minvalue += MajorTicksValue;
                }
                else
                {
                    break;
                }
                RTB.RenderTransform = RScalevalue;
                LTB.RenderTransform = LScalevalue;
                DepthScale.Children.Add(RTB);
                DepthScale.Children.Add(LTB);
                DepthScale.Children.Add(MajorTickLine);
                 
                    Double onedegree = MajorTicksStep / MinorDivisionsCount;
                if ((i < 1745) && (Math.Round(minvalue, ScaleValuePrecision) <= 650))
                {
                    for (Double mi = i + onedegree; mi < i + MajorTicksStep; mi += onedegree)
                    {

                        if (mi <= dx)
                        { MinorTickLineLen = 65 - (dx - mi) * 0.56; }
                        else if (mi > dx) { MinorTickLineLen = 65 - (mi - dx) * 0.56; }
                        Line MinorTickLine = new()
                        {
                            X1 = (240 - MinorTickLineLen) / 2,
                            Y1 = mi,
                            X2 = (240 - MinorTickLineLen) / 2 + MinorTickLineLen,
                            Y2 = mi,
                            Stroke = System.Windows.Media.Brushes.Black,
                            StrokeThickness = 0.5
                        };
                        DepthScale.Children.Add(MinorTickLine);
                        }
                }
            }
                _ = AniDepthScale(_lastDrawnDepth);

            }).Task;
        }
        private void DrawMillieScale()
        {
            MillieScale.Children.Clear();
            Double ScaleTicktAngle = 3.6;
            Double ScaleTicksValue = 1;
            Double minvalue = 0; ;
            for (Double i = -90; i < 269; i += ScaleTicktAngle)
            {
                Double i_radian = (i * Math.PI) / 180;
                if (minvalue == 0 || minvalue % 10 == 0)
                {
                    TextBlock ScaleTB = new();
                    TX(ScaleTB);
                    ScaleTB.Foreground = new SolidColorBrush(Colors.White);
                    ScaleTB.Height = 10;
                    ScaleTB.Width = 16;
                    ScaleTB.FontSize = 9;
                    ScaleTB.VerticalAlignment = VerticalAlignment.Center;
                    ScaleTB.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    TransformGroup ScaleTickRectTFG = new();
                    TranslateTransform ScaleTickRectTTF = new();
                    RotateTransform ScaleTickRectRT = new();
                    ScaleTickRectTTF.X = (int)((71) * Math.Cos(i_radian));
                    ScaleTickRectTTF.Y = (int)((71) * Math.Sin(i_radian));
                    ScaleTB.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTickRectRT.Angle = i - 270;
                    ScaleTickRectTFG.Children.Add(ScaleTickRectRT);
                    ScaleTickRectTFG.Children.Add(ScaleTickRectTTF);
                    ScaleTB.RenderTransform = ScaleTickRectTFG;
                    ScaleTB.Text = (minvalue / 10).ToString();
                    MillieScale.Children.Add(ScaleTB);
                }
                else
                {
                    Rectangle ScaleTickRect = new()
                    {
                        Height = 0.5,
                        Width = 5,
                        Fill = new SolidColorBrush(Colors.White)
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
                        X = (int)((73) * Math.Cos(i_radian)),
                        Y = (int)((73) * Math.Sin(i_radian))
                    };
                    ScaleTickRectTFG.Children.Add(ScaleTickRectTTF);
                    ScaleTickRect.RenderTransform = ScaleTickRectTFG;
                    MillieScale.Children.Add(ScaleTickRect);
                }
                minvalue += ScaleTicksValue;
            }
        }
        private void TX(TextBlock tx)
        {
            tx.Width = 14;
            tx.Height = 10;
            tx.FontFamily = new FontFamily("Areil");
            tx.FontWeight = FontWeights.Bold;
            tx.Foreground = new SolidColorBrush(Colors.Black);
            tx.TextAlignment = TextAlignment.Center;
            tx.VerticalAlignment = VerticalAlignment.Top;
            tx.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        }
        private Task AniDepthScale(Double value)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DoubleAnimation depth = new()
                {
                    To = -value * 2.5
                };
                ;
                depth.Duration = new Duration(TimeSpan.FromMilliseconds(20));
                Storyboard sb = new();
                Storyboard.SetTargetName(depth, "needle0");
                Storyboard.SetTargetProperty(depth, new PropertyPath(TranslateTransform.YProperty));
                sb.Children.Add(depth);

                if (value != 0)
                {
                    sb.Begin(this);
                }
                TranslateTransform needleTransform = (TranslateTransform)this.FindName("needle0");
                double finalAngle = (needleTransform.Y);
                /*
                TransformGroup AngleScaleTFG = new TransformGroup();
                TranslateTransform DepthScaleTF = new TranslateTransform();
                RotateTransform MiilieScaleRT = new RotateTransform();
                DepthScaleTF.Y = -value * 2.5;
                double vb = (value / 10) - (int)(value / 10);
                MiilieScaleRT.Angle = -vb * 3.6 * 100;
                DepthScale.RenderTransform = DepthScaleTF;
                MillieScale.RenderTransform = MiilieScaleRT;*/
            }).Task;
        }

    }
}
