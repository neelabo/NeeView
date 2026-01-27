using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(FolderCollection), typeof(Visibility))]
    public class FolderCollectionToFolderRecursiveVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderCollection collection)
            {
                return (collection.FolderParameter.IsFolderRecursive == true) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
