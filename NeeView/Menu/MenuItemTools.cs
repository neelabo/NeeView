using System;
using System.Globalization;

namespace NeeView
{
    public static class MenuItemTools
    {
        /// <summary>
        /// 数字をコンテキストメニューショートカット文字列にする
        /// </summary>
        /// <remarks>
        /// 1 -> _1<br/>
        /// 123 => 12_3
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string IntegerToAccessKey(int value)
        {
            var s = value.ToString(CultureInfo.InvariantCulture);
            if (s.Length == 1)
            {
                return "_" + s;
            }
            else
            {
                return s[0..^1] + "_" + s[^1];
            }
        }

        /// <summary>
        /// コンテキストメニュー文字列をエスケープする
        /// </summary>
        /// <remarks>
        /// _ -> __
        /// </remarks>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EscapeMenuItemString(string source)
        {
            return source.Replace("_", "__", StringComparison.Ordinal);
        }
    }


}
