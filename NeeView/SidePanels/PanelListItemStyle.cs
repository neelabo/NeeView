using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// フォルダーリストの表示方法
    /// </summary>
    public enum PanelListItemStyle
    {
        [AliasName]
        Normal, // テキストのみ

        [AliasName]
        Content, // コンテンツ

        [AliasName]
        Banner, // バナー

        [AliasName]
        Thumbnail, // サムネイル
    };

    public static class PanelListItemStyleExtensions
    {
        public static bool HasThumbnail(this PanelListItemStyle my)
        {
            return (my == PanelListItemStyle.Content || my == PanelListItemStyle.Banner);
        }
    }

    //
    public class PanelListItemStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PanelListItemStyle v0)
                return false;

            if (parameter is not PanelListItemStyle v1)
                return false;

            return v0 == v1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
