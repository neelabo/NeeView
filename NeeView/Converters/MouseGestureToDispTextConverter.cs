using System;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    // ジェスチャー表示用コンバータ
    [ValueConversion(typeof(string), typeof(string))]
    public class MouseGestureToDisplayTextConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MouseSequence gesture)
            {
                return gesture.GetDisplayString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
