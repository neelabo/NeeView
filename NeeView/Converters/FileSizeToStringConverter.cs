using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：ファイルサイズのKB表示
    [ValueConversion(typeof(PageMode), typeof(bool))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var length = (long)value;
            return ByteToDisplayString(length);
        }

        public static string ByteToDisplayString(long length)
        {
            if (length < 0)
            {
                return "";
            }
            else if (length == 0)
            {
                return "0 KB";
            }
            else
            {
                return $"{(length + 1023) / 1024:#,0} KB";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
