using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Size = System.Windows.Size;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using FontFamily = System.Windows.Media.FontFamily;
using System.Windows.Media.Effects;
using System.Drawing;
namespace ROV_GUI_Control.View
{
    public partial class Gauge : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Gauge),
           new PropertyMetadata(0.0, OnValuePropertyChanged));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Gauge f = d as Gauge;
            f.OnValueChanged(e);
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(Gauge), new FrameworkPropertyMetadata(100.0, new PropertyChangedCallback(OnMaxPropertyChanged)));
        public double MaxValue
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        private static void OnMaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Gauge f = d as Gauge;
            f.DrawScale();
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(Gauge), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnMinPropertyChanged)));
        public double MinValue
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        private static void OnMinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Gauge f = d as Gauge;
            f.DrawScale();
        }
        readonly double OptimalRangeStartValue = 20;
        readonly double OptimalRangeEndValue = 70;
        private Path rangeIndicator;
        private Double ArcRadius1 = 100;
        private Double ArcRadius2 = 140;
        private readonly int AnimatingSpeed = 10;
        private bool IsInitialValueSet = false;
        public double ScaleSweepAngle = 270;
        public double MinorDivisionsCount = 5;
        public double MajorDivisionsCount = 10;
        public int ScaleValuePrecision = 1;
        public double ScaleStartAngle = 135;
        public double NeedleStartAngle = 225;
        public double ScaleRadius = 65;
        public double ScaleLabelRadius = 50;
        public double ScaleLabelFontSize = 8;
        public Size MajorTickSize = new(10, 1.5);
        public Size MinorTickSize = new(5, 0.5);
        public Size ScaleLabelSize = new(20, 8);
      
        public static Color ScaleLabelForeground = Colors.White;
        public static Color MajorTickColor = Colors.Black;
        public static Color MinorTickColor = Colors.Black;
        private readonly double NeedleWidth = 6;
        private readonly double NeedleHeight = 65;
        private readonly SolidColorBrush needlecolor1 = Brushes.Green;
        private readonly SolidColorBrush needlecolor2 = Brushes.Yellow;
        public Gauge()
        {
            InitializeComponent();
            DrawScale();
            SetNeedle();
            DrawRangeIndicator();
        }
        public virtual void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            double newValue = (double)e.NewValue;
            double oldValue = (double)e.OldValue;
            if (newValue > this.MaxValue)
            {
                newValue = this.MaxValue;
            }
            else if (newValue < this.MinValue)
            {
                newValue = this.MinValue;
            }

            if (oldValue > this.MaxValue)
            {
                oldValue = this.MaxValue;
            }
            else if (oldValue < this.MinValue)
            {
                oldValue = this.MinValue;
            }
            double db1 = 0;
            Double oldcurr_realworldunit = 0;
            Double newcurr_realworldunit = 0;
            Double realworldunit = (ScaleSweepAngle / (MaxValue - MinValue));
            if (oldValue == 0 && !IsInitialValueSet)
            {
                oldValue = MinValue;
                IsInitialValueSet = true;
            }
            if (oldValue < 0)
            {
                db1 = MinValue + Math.Abs(oldValue);
                _ = ((double)(Math.Abs(db1 * realworldunit)));
            }
            else
            {
                db1 = Math.Abs(oldValue - MinValue);
                oldcurr_realworldunit = ((double)(db1 * realworldunit));
            }

            if (newValue < 0)
            {
                db1 = MinValue + Math.Abs(newValue);
                _ = ((double)(Math.Abs(db1 * realworldunit)));
            }
            else
            {
                db1 = Math.Abs(newValue - MinValue);
                newcurr_realworldunit = ((double)(db1 * realworldunit));
            }
            Double oldcurrentvalueAngle = needle0.Angle;
            Double newcurrentvalueAngle = (0);
            AnimateNeedle(oldcurrentvalueAngle, newcurrentvalueAngle);
        }
        private void AnimateNeedle(double oldcurrentvalueAngle, double newcurrentvalueAngle)
        {

            DoubleAnimation needlerot = new()
            {
                From = oldcurrentvalueAngle,
                To = newcurrentvalueAngle + NeedleStartAngle
            };
            double dur = Math.Abs(oldcurrentvalueAngle - newcurrentvalueAngle) * AnimatingSpeed;
            needlerot.Duration = new Duration(TimeSpan.FromMilliseconds(dur));
            Storyboard sb = new ();
            Storyboard.SetTargetName(needlerot, "needle0");
            Storyboard.SetTargetProperty(needlerot, new PropertyPath(RotateTransform.AngleProperty));
            sb.Children.Add(needlerot);
            
            if (newcurrentvalueAngle != oldcurrentvalueAngle)
            {
                sb.Begin(this);
            }
            RotateTransform needleTransform = (RotateTransform)this.FindName("needle0");
            double finalAngle = (needleTransform.Angle - NeedleStartAngle) / 2.7;
            textvalue.Text = (finalAngle).ToString();
            //textvalue.Text = Value.ToString();
        }
        private void SetNeedle()
        {
            needle0.Angle = NeedleStartAngle;
            needlegrid.Width = NeedleWidth;
            Point Point1 = new(0, 72.5);
            Point Point2 = new((NeedleWidth / 2) - 0.1, 72.5 - NeedleHeight);
            Point Point3 = new((NeedleWidth / 2) + 0.1, 72.5 - NeedleHeight);
            Point Point4 = new(NeedleWidth, 72.5);
            Point Point5 = new((NeedleWidth / 2) - 0.1, 72.5 - 17.5);
            Point Point6 = new((NeedleWidth / 2) + 0.1, 72.5 - 17.5);
            PointCollection polygon1Points = [Point1, Point2, Point3, Point4];
            needle1.Points = polygon1Points;
            PointCollection polygon2Points =
            [
                Point1,
                Point5,
                Point2,
                Point3,
                Point6,
                Point4,
            ];
            needle2.Points = polygon2Points;
            needle1.Fill = needlecolor1;
            needle2.Fill = needlecolor2;
            needle1.Opacity = 10;
        }
        private void DrawScale()
        {
            tick_container.Children.Clear();
            Double majorTickUnitAngle = ScaleSweepAngle / MajorDivisionsCount;
            _ = ScaleSweepAngle / MinorDivisionsCount;
            Double majorTicksUnitValue = (MaxValue - MinValue) / MajorDivisionsCount;
            majorTicksUnitValue = Math.Round(majorTicksUnitValue, ScaleValuePrecision);
            Double minvalue = MinValue; ;
            for (Double i = ScaleStartAngle; i <= (ScaleStartAngle + ScaleSweepAngle); i += majorTickUnitAngle)
            {
                DrawTick(i, MajorTickSize, MinorTickColor, true, minvalue);
                if (Math.Round(minvalue, ScaleValuePrecision) <= Math.Round(MaxValue, ScaleValuePrecision))
                {
                    minvalue = Math.Round(minvalue, ScaleValuePrecision);
                    minvalue += majorTicksUnitValue;
                }
                else
                {
                    break;
                }
                Double onedegree = ((i + majorTickUnitAngle) - i) / (MinorDivisionsCount);
                if ((i < (ScaleStartAngle + ScaleSweepAngle)) && (Math.Round(minvalue, ScaleValuePrecision) <= Math.Round(MaxValue, ScaleValuePrecision)))
                {
                    for (Double mi = i + onedegree; mi < (i + majorTickUnitAngle); mi += onedegree)
                    { 
                        DrawTick(mi, MinorTickSize, MinorTickColor, false);
                    }
                }
            }
        }
        private void DrawTick(double angle, Size tickSize, Color color, bool isMajorTick, double? currentValue = null)
        {
            var tickRect = new Rectangle
            {
                Height = tickSize.Height,
                Width = tickSize.Width,
                Fill = new SolidColorBrush(color),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform { Angle = angle });
            double angleRadian = angle * Math.PI / 180;
            transformGroup.Children.Add(new TranslateTransform
            {
                X = ScaleRadius * Math.Cos(angleRadian),
                Y = ScaleRadius * Math.Sin(angleRadian)
            });
            tickRect.RenderTransform = transformGroup;
            tickRect.RenderTransformOrigin = new Point(0.5, 0.5);
            tick_container.Children.Add(tickRect);

            if (isMajorTick && currentValue.HasValue)
            {
                if (Math.Round(currentValue.Value, ScaleValuePrecision) <= Math.Round(MaxValue, ScaleValuePrecision))
                {
                    var labelTransform = new TranslateTransform
                    {
                        X = ScaleLabelRadius * Math.Cos(angleRadian),
                        Y = ScaleLabelRadius * Math.Sin(angleRadian)
                    };
                    var tb = new TextBlock
                    {
                        Text = Math.Round(currentValue.Value, ScaleValuePrecision).ToString(),
                        Height = ScaleLabelSize.Height,
                        Width = ScaleLabelSize.Width,
                        FontSize = ScaleLabelFontSize,
                        Foreground = new SolidColorBrush(ScaleLabelForeground),
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        FontFamily = new FontFamily("Arial"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        RenderTransform = labelTransform
                    };
                    tick_container.Children.Add(tb);
                }
            }
        }
        private void DrawRangeIndicator()
        {
            Double realworldunit = (ScaleSweepAngle / (MaxValue - MinValue));
            Double optimalStartAngle;
            Double optimalEndAngle;
            double db;
            if (OptimalRangeStartValue < 0)
            {
                db = MinValue + Math.Abs(OptimalRangeStartValue);
                optimalStartAngle = ((double)(Math.Abs(db * realworldunit)));
            }
            else
            {
                db = Math.Abs(MinValue) + OptimalRangeStartValue;
                optimalStartAngle = ((double)(db * realworldunit));
            }
            if (OptimalRangeEndValue < 0)
            {
                db = MinValue + Math.Abs(OptimalRangeEndValue);
                optimalEndAngle = ((double)(Math.Abs(db * realworldunit)));
            }
            else
            {
                db = Math.Abs(MinValue) + OptimalRangeEndValue;
                optimalEndAngle = ((double)(db * realworldunit));
            }
            Double optimalStartAngleFromStart = (ScaleStartAngle + optimalStartAngle);
            Double optimalEndAngleFromStart = (ScaleStartAngle + optimalEndAngle);
            ArcRadius1 = 68;
            ArcRadius2 = 60;

            double endAngle = ScaleStartAngle + ScaleSweepAngle;
            Point A = GetCircumferencePoint(ScaleStartAngle, ArcRadius1);
            Point B = GetCircumferencePoint(ScaleStartAngle, ArcRadius2);
            Point C = GetCircumferencePoint(optimalStartAngleFromStart, ArcRadius2);
            Point D = GetCircumferencePoint(optimalStartAngleFromStart, ArcRadius1);
            bool isReflexAngle = Math.Abs(optimalStartAngleFromStart - ScaleStartAngle) > 180.0;
            DrawSegment(A, B, C, D, isReflexAngle, Colors.Red);

            Point A1 = GetCircumferencePoint(optimalStartAngleFromStart, ArcRadius1);
            Point B1 = GetCircumferencePoint(optimalStartAngleFromStart, ArcRadius2);
            Point C1 = GetCircumferencePoint(optimalEndAngleFromStart, ArcRadius2);
            Point D1 = GetCircumferencePoint(optimalEndAngleFromStart, ArcRadius1);
            bool isReflexAngle1 = Math.Abs(optimalEndAngleFromStart - optimalStartAngleFromStart) > 180.0;
            DrawSegment(A1, B1, C1, D1, isReflexAngle1, Colors.Yellow);

            Point A2 = GetCircumferencePoint(optimalEndAngleFromStart, ArcRadius1);
            Point B2 = GetCircumferencePoint(optimalEndAngleFromStart, ArcRadius2);
            Point C2 = GetCircumferencePoint(endAngle, ArcRadius2);
            Point D2 = GetCircumferencePoint(endAngle, ArcRadius1);
            bool isReflexAngle2 = Math.Abs(endAngle - optimalEndAngleFromStart) > 180.0;
            DrawSegment(A2, B2, C2, D2, isReflexAngle2, Colors.Lime);
        }
        private Point GetCircumferencePoint(Double angle, Double radius)
        {
            Double angle_radian = (angle * Math.PI) / 180;
            Double X = (Double)((72.5) + (radius) * Math.Cos(angle_radian));
            Double Y = (Double)((72.5) + (radius) * Math.Sin(angle_radian));
            Point p = new(X, Y);
            return p;
        }
        private void DrawSegment(Point p1, Point p2, Point p3, Point p4, bool reflexangle, Color clr)
        {
            PathSegmentCollection segments =
            [
                new LineSegment() { Point = p2 },
                new ArcSegment()
                {
                    Size = new Size(ArcRadius2, ArcRadius2),
                    Point = p3,
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = reflexangle


                },
                new LineSegment() { Point = p4 },
                new ArcSegment()
                {
                    Size = new Size(ArcRadius1, ArcRadius1),
                    Point = p1,
                    SweepDirection = SweepDirection.Counterclockwise,
                    IsLargeArc = reflexangle

                },
            ];
            Color rangestrokecolor;
            if (clr == Colors.Transparent)
            {
                rangestrokecolor = clr;
            }
            else
                rangestrokecolor = Colors.White;

            DropShadowEffect shadowEffect = new()
            {
                Color = clr,
                BlurRadius = 15,
                Direction = 270,
                ShadowDepth = 3
            };
            rangeIndicator = new Path()
            {
                StrokeLineJoin = PenLineJoin.Round,
                Stroke = new SolidColorBrush(rangestrokecolor),
                Fill = new SolidColorBrush(clr),
                Opacity = 0.65,
                StrokeThickness = 0.25,
                Effect = shadowEffect,
                Data = new PathGeometry()
                {
                    Figures =
                     [
                        new PathFigure()
                        {
                            IsClosed = true,
                            StartPoint = p1,
                            Segments = segments
                        }
                    ]
                }
            };
            range_container.Children.Add(rangeIndicator);
        }
    }
}
