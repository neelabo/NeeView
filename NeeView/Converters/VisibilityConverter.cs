using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class VisibilityConverter : IValueConverter
    {
        public Visibility Visible { get; set; } = Visibility.Visible;
        public Visibility Hidden { get; set; } = Visibility.Hidden;
        public Visibility Collapsed { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value switch
            {
                Visibility.Visible => Visible,
                Visibility.Hidden => Hidden,
                Visibility.Collapsed => Collapsed,
                _ => Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class VisibilityInverseConverter : VisibilityConverter
    {
        public VisibilityInverseConverter()
        {
            Visible = Visibility.Hidden;
            Hidden = Visibility.Visible;
            Collapsed = Visibility.Visible;
        }
    }
}
