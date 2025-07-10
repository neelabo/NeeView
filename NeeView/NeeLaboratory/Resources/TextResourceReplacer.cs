using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Resources
{
    public partial class TextResourceReplacer
    {
        /// <summary>
        /// リソースキーのパターン
        /// </summary>
        [GeneratedRegex(@"@\[([^\]]+)\]")]
        private static partial Regex _resourceKeyRegex { get; }

        /// <summary>
        /// テキストキーのパターン
        /// </summary>
        [GeneratedRegex(@"@([a-zA-Z0-9_\.\-#]+[a-zA-Z0-9])")]
        private static partial Regex _keyRegex { get; }



        private readonly ITextResource _resource;
        private int _wordReplaceCount;
        private int _fileReplaceCount;

        public TextResourceReplacer(ITextResource resource)
        {
            _resource = resource;
        }


        public ReplaceResult Replace(string s, bool fallback)
        {
            _wordReplaceCount = 0;
            _fileReplaceCount = 0;
            var text = ReplaceCore(s, fallback, 0);
            return new ReplaceResult(text, _wordReplaceCount, _fileReplaceCount);
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える
        /// </summary>
        /// <param name="s">変換元文字列</param>
        /// <param name="fallback">true のとき、リソースキーが存在しないときはリソースキー名のままにする。false のときは "" に変換する</param>
        /// <param name="depth">再帰の深さ。計算リミット用</param>
        /// <returns></returns>
        public string ReplaceCore(string s, bool fallback, int depth)
        {
            // limit is 5 depth
            if (depth >= 5) return s;

            return ReplaceEmbeddedText(_keyRegex.Replace(s, ReplaceMatchEvaluator));

            string ReplaceMatchEvaluator(Match m)
            {
                _wordReplaceCount++;

                var key = m.Groups[1].Value;
                var s = _resource.GetResourceString(key)?.String;
                return s is not null ? ReplaceCore(s, fallback, depth + 1) : (fallback ? m.Value : "");
            }
        }

        /// <summary>
        /// @[...] という文字列をテキストリソースのパスとして文字列を入れ替える
        /// </summary>
        public string ReplaceEmbeddedText(string s)
        {
            return _resourceKeyRegex.Replace(s, FileNameToTextMatchEvaluator);

            string FileNameToTextMatchEvaluator(Match match)
            {
                _fileReplaceCount++;

                var fileName = match.Groups[1].Value;
                var fileSource = new AppFileSource(new Uri(fileName, UriKind.Relative));
                using var stream = fileSource.Open();
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }


        public record ReplaceResult(string Text, int WordReplaceCount, int FileReplaceCount);
    }
}
