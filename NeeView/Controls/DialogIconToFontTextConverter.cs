using System;
using System.Windows.Data;

namespace NeeView
{
    public class DialogIconToFontTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MessageDialogIcon icon)
            {
                return icon switch
                {
                    MessageDialogIcon.Error
                        => "\uE783",
                    MessageDialogIcon.Warning
                        => "\uE7BA",
                    MessageDialogIcon.Information
                        => "\uE946",
                    MessageDialogIcon.Question
                        => "\uE897",
                    _
                        => ""
                };
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
