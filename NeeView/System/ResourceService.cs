using NeeLaboratory.Resources;
using NeeView.Properties;
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
                var name = key[1..];

                var text = TextResources.GetStringRaw(name);
                if (text is not null)
                {
                    return text;
                }
                else
                {
                    Debug.WriteLine($"Error: Not found resource key: {name}");
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
            return TextResources.Replace(s, false) ?? "";
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
            return TextResources.Replace(s, true) ?? "";
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// </summary>
        public static string Replace(string s)
        {
            return TextResources.Replace(s, true) ?? "";
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <param name="isRecursive">結果に含まれるキーを変換する</param>
        /// <returns>存在しない場合は null</returns>
        public static string? GetResourceString(string key, bool isRecursive)
        {
            // TODO: 普通の GetString() でもよいと思う。isRecursive は true しか使ってないようです。

            if (key is null || key[0] != '@') return null;
            var rawKey = key[1..];

            return TextResources.GetStringRaw(rawKey);
        }

        /// <summary>
        /// 連結単語文字列を生成
        /// </summary>
        public static string Join(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens.Select(e => string.Format(CultureInfo.InvariantCulture, TextResources.GetStringRaw("TokenFormat") ?? "", e)));
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

        /// <summary>
        /// 開発用：全言語のテキストリソースを検証する
        /// </summary>
        [Conditional("DEBUG")]
        public static void ValidateAllCultures()
        {
            foreach (var culture in Properties.TextResources.LanguageResource.Cultures)
            {
                Properties.TextResources.LoadCulture(culture);

                // 参照キーの存在チェック
                ValidateResourceKeys();

                // ヘルプテキスト目視チェック
                //SearchOptionManual.OpenSearchOptionManual();
                //System.Threading.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 開発用：すべてのリソースについて含まれるリソースキー名が実在するかを確認する
        /// </summary>
        [Conditional("DEBUG")]
        private static void ValidateResourceKeys()
        {
            var failureCount = 0;
            var keyRegex = new Regex(@"@[a-zA-Z0-9_\.\-#]+[a-zA-Z0-9]");

            foreach (var pair in Properties.TextResources.Resource.Map)
            {
                foreach (Match match in keyRegex.Matches(pair.Value.Text.String))
                {
                    if (GetString(match.Value, false) is null)
                    {
                        Debug.WriteLine($"Resource key not found: {match.Value} in {pair.Key}");
                        failureCount++;
                    }
                }
            }

            Debug.Assert(failureCount == 0, $"{failureCount} resource keys are undefined.");
        }
    }
}
