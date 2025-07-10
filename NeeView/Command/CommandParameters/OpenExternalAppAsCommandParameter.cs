using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class OpenExternalAppAsCommandParameter : CommandParameter
    {
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;
        private int _index;


        /// <summary>
        /// 複数ページのときの動作
        /// </summary>
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { _multiPagePolicy = value; }
        }

        /// <summary>
        /// 選択された外部アプリの番号。0 は未選択
        /// </summary>
        [PropertyMember(NoteConverter = typeof(IntToExternalAppString))]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, Math.Max(0, value)); }
        }
    }


    public class IntToExternalAppString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index) return "";

            if (index <= 0) return TextResources.GetString("Word.SelectionMenu");
            index--;

            var items = Config.Current.System.ExternalAppCollection;
            if (items.Count <= index) return TextResources.GetString("Word.Undefined");

            return Config.Current.System.ExternalAppCollection[index].DisplayName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
