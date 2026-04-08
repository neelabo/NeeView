using NeeView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.StringTemplate
{
    public static class Formatter
    {
        private static readonly char[] _suffixes = ['1', '2', 'L', 'R'];

        public static StringFormat<TSource> CreateStringFormat<TSource>(string format, Dictionary<string, KeyInfo<TSource>> keyMap)
        {
            var parseFormat = StringFormatParser.Parse(format);
            return new(parseFormat.Format, parseFormat.Words.Select(e => FindWordInfo(e, keyMap)));
        }

        public static string Format<TSource>(StringFormat<TSource> format, TSource source)
        {
            var args = format.Words.Select(e => GetFormattedWord(source, e)).ToArray();

            return string.Format(format.Format, args);
        }

        private static WordInfo<TSource> FindWordInfo<TSource>(string placeholder, Dictionary<string, KeyInfo<TSource>> keyMap)
        {
            var tokens = placeholder.Split(':', 2);

            var word = tokens[0].Trim();
            var format = tokens.Length > 1 ? tokens[1] : "";

            if (string.IsNullOrEmpty(word))
            {
                return new(placeholder, null, "", "");
            }

            if (keyMap.TryGetValue(word, out var info))
            {
                return new(placeholder, info, "", format);
            }

            var suffixMaybe = word.Last();
            if (_suffixes.Contains(suffixMaybe))
            {
                var key = word[..^1];
                if (keyMap.TryGetValue(key, out info))
                {
                    return new(placeholder, info, suffixMaybe.ToString(), format);
                }
            }

            return new(placeholder, null, "", "");
        }

        private static string GetFormattedWord<TSource>(TSource source, WordInfo<TSource> wordInfo)
        {
            try
            {
                if (wordInfo.FormatInfo is null)
                {
                    return GetDefaultWord(wordInfo.Placeholder);
                }

                return wordInfo.FormatInfo.Formatter(source, wordInfo.Format, wordInfo.Suffix);
            }
            catch
            {
                return GetDefaultWord(wordInfo.Placeholder);
            }
        }

        private static string GetDefaultWord(string w)
        {
            return "{" + w + "}";
        }
    }
}

