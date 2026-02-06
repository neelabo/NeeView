using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class IntToDestinationFolderString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index) return "";

            if (index <= 0) return TextResources.GetString("Word.SelectionMenu");
            index--;

            var items = Config.Current.System.DestinationFolderCollection;
            if (items.Count <= index) return TextResources.GetString("Word.Undefined");

            return items[index].Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
