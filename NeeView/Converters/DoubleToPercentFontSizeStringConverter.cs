using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToPercentFontSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultFontSize = GetDefaultFontSize();
            var unit = parameter as string;

            if (value is double v)
            {
                return unit switch
                {
                    "pt" => $"{PixelToPoint(v * defaultFontSize):F1}pt",
                    "px" => $"{v * defaultFontSize:F1}px",
                    _ => $"{v:P0}",
                };
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultFontSize = GetDefaultFontSize();

            var s = value as string;
            if (s is null || string.IsNullOrWhiteSpace(s))
            {
                return 0.0;
            }

            var valueWithoutPercentage = s.TrimEnd(' ', '%');
            if (double.TryParse(valueWithoutPercentage, out double x))
            {
                return x / 100.0;
            }

            try
            {
                var converter = new FontSizeConverter();
                double? pixel = converter.ConvertFromString(s) as double?;
                if (pixel.HasValue)
                {
                    return pixel.Value / defaultFontSize;
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        protected virtual double GetDefaultFontSize()
        {
            return FontType.Message.ToFontSize();
        }

        private static double PixelToPoint(double pixel)
        {
            // 1pt == (96/72) px
            return pixel * 72.0 / 96.0;
        }

        private static double PointToPixel(double point)
        {
            // 1pt == (96/72) px
            return point * 96.0 / 72.0;
        }
    }


    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToPercentMessageFontSizeStringConverter : DoubleToPercentFontSizeStringConverter
    {
        protected override double GetDefaultFontSize()
        {
            return FontType.Message.ToFontSize();
        }
    }


    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToPercentMenuFontSizeStringConverter : DoubleToPercentFontSizeStringConverter
    {
        protected override double GetDefaultFontSize()
        {
            return FontType.Menu.ToFontSize();
        }
    }

}
