using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents; // Required for 'Run'
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows;

namespace ROV_GUI_Control.Converters
{
    public class Forward : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
    public class BooleanToEnableDisableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return "";
            if (values[0] is bool isPower)
            {
                if (isPower)
                {
                    return (bool)values[1] ? "Disable" : "Enable";
                }
                else
                {
                    return "Enable";
                }
            }
            return "";
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StreamButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock textBlock = new();
            if ((bool)value)
            {
                textBlock.Inlines.Add(new Run("Stop Streaming ") { Foreground = Brushes.Black });
                textBlock.Inlines.Add(new Run("⏹") { Foreground = Brushes.Red });
            }
            else
            {
                textBlock.Inlines.Add(new Run("Start Streaming ") { Foreground = Brushes.Black });
                textBlock.Inlines.Add(new Run("▶️") { Foreground = Brushes.Green });
            }
            return textBlock;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToLower() == "ON";
        }
    }

    public class LEDButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock textBlock = new();
            if ((bool)value)
            {
                textBlock.Inlines.Add(new Run("Light OFF ") { Foreground = Brushes.Black });
                textBlock.Inlines.Add(new Run("💡") { Foreground = Brushes.LimeGreen });
            }
            else
            {
                textBlock.Inlines.Add(new Run("Light ON ") { Foreground = Brushes.Black });
                textBlock.Inlines.Add(new Run("💡") { Foreground = Brushes.DarkRed });
            }
            return textBlock;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToLower() == "ON";
        }
    }
    public class LEDLightnessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return "";
            if (values[0] is bool isLight && values[1] is double brightness)
            {
                if (isLight)
                {
                    double opcity = (double)values[1] / 100;
                    DropShadowEffect shadowEffect = new()
                    {
                        Color = Colors.LimeGreen,
                        BlurRadius = 10,
                        ShadowDepth = 1,
                        Opacity = 1
                    };
                    Label label = new()
                    {
                        Content = "☀️",
                        FontSize = 17,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(0, 0, 0, 0),
                        Opacity = opcity,
                        Effect = shadowEffect,
                        Foreground = (opcity <= 0.25) ? Brushes.Yellow : (opcity <= 0.5) ? Brushes.YellowGreen : (opcity <= .75) ? Brushes.LightGreen : Brushes.LimeGreen
                    };
                    return label;
                }
            }
            return "";
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
