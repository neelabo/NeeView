using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Resources;

namespace NeeView
{
    public static partial class HtmlHelpUtility
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex _spaceRegex { get; }

        /// <summary>
        /// ヘルプ用HTMLヘッダ生成
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string CreateHeader(string title)
        {
            string stylesheet = "";
            var fileUri = new Uri("/Resources/Style.css", UriKind.Relative);
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(fileUri);
            using (var sr = new StreamReader(info.Stream))
            {
                stylesheet = sr.ReadToEnd();
                stylesheet = _spaceRegex.Replace(stylesheet, " ");
            }

            string s = "<!DOCTYPE html>\n" +
                @"<html><head>" +
                @"<meta charset=""utf-8"">" +
                "<style>" + stylesheet + "</style>" +
                @"<title>" + title + "</title></head>";

            return s;
        }

        /// <summary>
        /// ヘルプ用HTMLフッタ生成
        /// </summary>
        /// <returns></returns>
        public static string CreateFooter()
        {
            return @"</html>";
        }

    }
}


