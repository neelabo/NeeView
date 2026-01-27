using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public partial class PanelListBannerImage : Control
    {
        static PanelListBannerImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PanelListBannerImage), new FrameworkPropertyMetadata(typeof(PanelListBannerImage)));
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(PanelListBannerImage), new PropertyMetadata(null));
    }


    public class BooleanToBannerStretchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.BannerItemProfile.ImageStretch;
            }
            else
            {
                return System.Windows.Media.Stretch.Uniform;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BooleanToBannerViewboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.BannerItemProfile.Viewbox;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BooleanToBannerAlignmentYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.BannerItemProfile.AlignmentY;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BannerBackgroundBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush brush)
            {
                if (brush is SolidColorBrush solidColorBrush && solidColorBrush.Color.A != 0)
                {
                    return brush;
                }
                else
                {
                    return Config.Current.Panels.BannerItemProfile.Background;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BannerProfileToolTopEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.BannerItemProfile.IsImagePopupEnabled;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
