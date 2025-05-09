using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ImageSourceCollectionからDPIを加味した適切サイズの画像を取得
    /// </summary>
    public class ImageSourceCollectionToImageSourceConverter : IMultiValueConverter
    {
        public double Width { get; set; } = 16.0;

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) throw new InvalidOperationException();

            if (values[0] is not IImageSourceCollection frames)
            {
                return DependencyProperty.UnsetValue;
            }

            if (values[1] is double scale)
            {
            }
            else if (values[1] is DpiScale dpiScale)
            {
                scale = dpiScale.DpiScaleX;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }

            return frames.GetImageSource(Width * scale);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
