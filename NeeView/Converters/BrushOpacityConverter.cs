using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class BrushOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush originalBrush)
            {
                Brush newBrush = originalBrush.Clone();

                double opacity = 0.5;
                if (parameter != null && double.TryParse(parameter.ToString(), out double parsedOpacity))
                {
                    opacity = parsedOpacity;
                }

                newBrush.Opacity = opacity;
                return newBrush;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
