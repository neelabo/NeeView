using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public abstract record class InputGestureSource
    {
        public abstract InputGesture GetInputGesture();
        public abstract string GetDisplayString();
    }


    public class InputGestureSourceToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return "";

            if (value is InputGestureSource gesture)
            {
                return gesture.GetDisplayString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
