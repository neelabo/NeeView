using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class QuickAccessEntryToImageSourceConverter : IMultiValueConverter
    {
        public double Width { get; set; } = 16.0;

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) throw new InvalidOperationException();

            if (values[0] is not IQuickAccessEntry entry)
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


            if (entry is QuickAccessRoot)
            {
                return ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_lightning");
            }
            else if (entry is QuickAccessFolder)
            {
                return ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "fic_lightning");
            }
            else if (entry is QuickAccess quickAccess)
            {
                var frames = PathToPlaceIconConverter.Convert(new QueryPath(quickAccess.Path));
                return frames.GetImageSource(Width * scale);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
