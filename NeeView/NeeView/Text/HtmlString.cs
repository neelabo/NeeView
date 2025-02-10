using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeeView.Text
{
    public static partial class HtmlString
    {
        /// <summary>
        ///  imgタグ用正規表現
        /// </summary>
        [GeneratedRegex(@"<img(?:\s+[^>]*\s+|\s+)src\s*=\s*(?:(?<quot>[""'])(?<url>.*?)\k<quot>|(?<url>[^\s>]+))[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
        private static partial Regex _imageTagRegex { get; }

        /// <summary>
        /// imgタグ抜き出し
        /// </summary>
        /// <returns>imgタグのURLリスト</returns>
        public static List<string> CollectImgSrc(string? source)
        {
            if (source is null) return new List<string>();

            var matchCollection = _imageTagRegex.Matches(source);
            var urls = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                urls.Add(match.Groups["url"].Value);
            }

            return urls;
        }
    }
}


