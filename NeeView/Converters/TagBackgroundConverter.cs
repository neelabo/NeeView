using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class TagBackgroundConverter : IMultiValueConverter
    {
        public Brush DefaultBrush { get; set; } = Brushes.Gray;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var brush = (values[0] as Brush) ?? (values[1] as Brush) ?? DefaultBrush;
                var opacity = (double)values[2];

                var newBrush = brush.Clone();
                newBrush.Opacity = opacity;
                return newBrush;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Converter Error: {ex.Message}");
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
