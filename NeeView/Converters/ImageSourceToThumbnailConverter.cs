using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ImageSourceが nullの場合、デフォルト画像を割り当てる。サムネイル用
    /// </summary>
    [ValueConversion(typeof(ImageSource), typeof(ImageSource))]
    public class ImageSourceToThumbnailConverter : IValueConverter
    {
        private static readonly ImageSource? _defaultThumbnail = MainWindow.Current.Resources["thumbnail_default"] as ImageSource;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return _defaultThumbnail;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
