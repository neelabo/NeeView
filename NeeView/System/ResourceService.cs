using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace NeeView
{
    internal static partial class ResourceService
    {
        /// <summary>
        /// リソースキーのパターン
        /// </summary>
        [GeneratedRegex(@"@\[([^\]]+)\]")]
        private static partial Regex _resourceKeyRegex { get; }

        /// <summary>
        /// テキストキーのパターン
        /// </summary>
        [GeneratedRegex(@"@[a-zA-Z0-9_\.\-#]+[a-zA-Z0-9]")]
        private static partial Regex _keyRegex { get; }

        /// <summary>
        /// @で始まる文字列はリソースキーとしてその値を返す。
        /// そうでない場合はそのまま返す。
        /// </summary>
        public static string GetString(string? key)
        {
            return GetString(key, true);
        }

        /// <summary>
        /// @で始まる文字列はリソースキーとしてその値を返す。
        /// そうでない場合はそのまま返す。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="fallback">trueの場合、存在しないときにはそのまま key を返す。falseの場合、空文字を返す</param>
        /// <returns></returns>
        public static string GetString(string? key, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(key) || key[0] != '@')
            {
                return key ?? "";
            }
            else
            {
                var text = GetResourceString(key);
                if (text != null)
                {
                    return Replace(text, fallback);
                }
                else
                {
                    Debug.WriteLine($"Error: Not found resource key: {key[1..]}");
                    return fallback ? key : "";
                }
            }
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// 対応するリソースキーがなければ空文字に置換する。
        /// </summary>
        /// <remarks>
        /// HtmlNode の TextEvaluator 用
        /// </remarks>
        public static string ReplaceEmpty(string s)
        {
            return Replace(s, false);
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// 対応するリソースキーがなければそのままにする。
        /// </summary>
        /// <remarks>
        /// HtmlNode の TextEvaluator 用
        /// </remarks>
        public static string ReplaceFallback(string s)
        {
            return Replace(s, true);
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// </summary>
        public static string Replace(string s)
        {
            return Replace(s, true);
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える
        /// </summary>
        /// <param name="s">変換元文字列</param>
        /// <param name="fallback">true のとき、リソースキーが存在しないときはリソースキー名のままにする。false のときは "" に変換する</param>
        /// <param name="depth">再帰の深さ。計算リミット用</param>
        /// <returns></returns>
        public static string Replace(string s, bool fallback, int depth = 0)
        {
            // limit is 5 depth
            if (depth >= 5) return s;

            return ReplaceEmbeddedText(_keyRegex.Replace(s, ReplaceMatchEvaluator));

            string ReplaceMatchEvaluator(Match m)
            {
                var s = GetResourceString(m.Value);
                return s is not null ? Replace(s, fallback, depth + 1) : (fallback ? m.Value : "");
            }
        }

        /// <summary>
        /// @[...] という文字列をテキストリソースのパスとして文字列を入れ替える
        /// </summary>
        public static string ReplaceEmbeddedText(string s)
        {
            return _resourceKeyRegex.Replace(s, FileNameToTextMatchEvaluator);
        }

        private static string FileNameToTextMatchEvaluator(Match match)
        {
            var fileName = match.Groups[1].Value;
            var fileSource = new AppFileSource(new Uri(fileName, UriKind.Relative));
            using var stream = fileSource.Open();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <returns>存在しない場合は null</returns>
        public static string? GetResourceString(string key)
        {
            if (key is null || key[0] != '@') return null;

            var rawKey = key[1..];
            var tokens = rawKey.Split('.', 2);
            return tokens[0] switch
            {
                nameof(Key)
                    => (tokens.Length >= 2 && Enum.TryParse<Key>(tokens[1], out var inputKey)) ? inputKey.GetDisplayString() : null,
                nameof(ModifierKeys)
                    => (tokens.Length >= 2 && Enum.TryParse<ModifierKeys>(tokens[1], out var modifierKey)) ? modifierKey.GetDisplayString() : null,
                _
                    => Properties.TextResources.GetStringRaw(rawKey),
            };
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <param name="isRecursive">結果に含まれるキーを変換する</param>
        /// <returns>存在しない場合は null</returns>
        public static string? GetResourceString(string key, bool isRecursive)
        {
            var text = GetResourceString(key);

            if (text != null && isRecursive)
            {
                return Replace(text);
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// 連結単語文字列を生成
        /// </summary>
        public static string Join(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens.Select(e => string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetStringRaw("TokenFormat") ?? "", e)));
        }

        /// <summary>
        /// リソースキー名補正
        /// </summary>
        /// <remarks>
        /// 先頭に @ があることを保証する
        /// </remarks>
        /// <param name="key"></param>
        /// <returns>@付きリソースキー名</returns>
        public static string ValidateKeyName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "Undefined";
            }
            else if (key[0] == '@')
            {
                return key;
            }
            else
            {
                return '@' + key;
            }
        }
    }
}
