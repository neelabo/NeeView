using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class CommandNameToStringConverter : IValueConverter
    {
        private readonly Func<CommandElement, string> _getCommandTextFunc;

        public CommandNameToStringConverter(Func<CommandElement, string> getCommandTextFunc)
        {
            _getCommandTextFunc = getCommandTextFunc;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CommandTools.GetCommandText(value as string, _getCommandTextFunc);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class CommandNameToLongTextConverter : CommandNameToStringConverter
    {
        public CommandNameToLongTextConverter() : base((CommandElement e) => e.LongText)
        {
        }
    }


    public class CommandNameToMenuTextConverter : CommandNameToStringConverter
    {
        public CommandNameToMenuTextConverter() : base((CommandElement e) => e.Menu)
        {
        }
    }

}
