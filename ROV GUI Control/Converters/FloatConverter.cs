using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ROV_GUI_Control.Converters
{
    public class FloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;

            if (string.IsNullOrWhiteSpace(str))
                return 0.0f;

            if (float.TryParse(str, out float result))
                return result;

            return DependencyProperty.UnsetValue;
        }
    }
}
