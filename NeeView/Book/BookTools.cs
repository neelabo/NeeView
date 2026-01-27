using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public static class BookTools
    {
        public static string PathToBookName(string path)
        {
            return path.EndsWith(@":\", StringComparison.Ordinal) ? path : LoosePath.GetFileName(path);
        }

        public static bool CanBookmark(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && !path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal);
        }
    }


    public class PathToBookNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return BookTools.PathToBookName(path);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
