using NeeLaboratory.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;


namespace NeeView
{
    public static class TitleStringFormatter
    {
        public const string BookKey = "Book";
        public const string PageMaxKey = "PageMax";
        public const string PagePartKey = "PagePart";
        public const string PageKey = "Page";
        public const string FullPathKey = "FullPath";
        public const string EntryPathKey = "EntryPath";
        public const string NameKey = "Name";
        public const string BitsKey = "Bits";
        public const string SizeKey = "Size";
        public const string ViewScaleKey = "ViewScale";
        public const string ScaleKey = "Scale";

        private static readonly char[] _suffixes = ['1', '2', 'L', 'R'];


        private static readonly Dictionary<string, TitleStringKeyFormatInfo> _wordFormatMap = new Dictionary<string, TitleStringKeyFormatInfo>(StringComparer.Ordinal)
        {
            [BookKey] = new(GetBookWord, TitleStringChangedAction.ViewContentChanged),
            [PageMaxKey] = new(GetPageMaxWord, TitleStringChangedAction.ViewContentChanged),
            [PagePartKey] = new(GetPagePartWord, TitleStringChangedAction.ViewContentChanged),
            [PageKey] = new(GetPageWord, TitleStringChangedAction.ViewContentChanged),
            [FullPathKey] = new(GetFullPathWord, TitleStringChangedAction.ViewContentChanged),
            [EntryPathKey] = new(GetEntryPathWord, TitleStringChangedAction.ViewContentChanged),
            [NameKey] = new(GetNameWord, TitleStringChangedAction.ViewContentChanged),
            [BitsKey] = new(GetBitsWord, TitleStringChangedAction.ViewContentChanged),
            [SizeKey] = new(GetSizeWord, TitleStringChangedAction.ViewContentChanged),
            [ViewScaleKey] = new(GetViewScaleWord, TitleStringChangedAction.ViewContentChanged | TitleStringChangedAction.ScaleChanged | TitleStringChangedAction.StretchChanged),
            [ScaleKey] = new(GetScaleWord, TitleStringChangedAction.ViewContentChanged | TitleStringChangedAction.ScaleChanged | TitleStringChangedAction.StretchChanged),
        };

        public static TitleFormatSource CreateFormatSource(string format)
        {
            var parseFormat = StringFormatParser.Parse(format);
            return new(parseFormat.Format, parseFormat.Words.Select(e => FindWordInfo(e)));
        }


        public static string Format(TitleFormatSource format, TitleSource source)
        {
            var args = format.Words.Select(e => GetFormattedWord(source, e)).ToArray();

            return string.Format(format.Format, args);
        }

        private static TitleStringWordInfo FindWordInfo(string placeholder)
        {
            var tokens = placeholder.Split(':', 2);

            var word = tokens[0].Trim();
            var format = tokens.Length > 1 ? tokens[1] : "";

            if (string.IsNullOrEmpty(word))
            {
                return new(placeholder, null, "", "");
            }

            if (_wordFormatMap.TryGetValue(word, out var info))
            {
                return new(placeholder, info, "", format);
            }

            var suffixMaybe = word.Last();
            if (_suffixes.Contains(suffixMaybe))
            {
                var key = word[..^1];
                if (_wordFormatMap.TryGetValue(key, out info))
                {
                    return new(placeholder, info, suffixMaybe.ToString(), format);
                }
            }

            return new(placeholder, null, "", "");
        }

        private static string GetFormattedWord(TitleSource source, TitleStringWordInfo wordInfo)
        {
            try
            {
                if (wordInfo.FormatInfo is null)
                {
                    return GetDefaultWord(wordInfo.Placeholder);
                }

                return wordInfo.FormatInfo.Formatter(wordInfo.Format, source, wordInfo.Suffix);
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

        private static string GetBookWord(string format, TitleSource source, string suffix)
        {
            if (source.Book is null) return "";
            string bookName = LoosePath.GetDisplayName(source.Book.Path);
            return FormatValue(format, bookName);
        }

        private static string GetPageMaxWord(string format, TitleSource source, string suffix)
        {
            if (source.Book is null) return "";
            var pageMax = source.Book.Pages.Count;
            return FormatValue(format, pageMax);
        }

        private static string GetPageWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            if (content is null) return "";
            var pageNumber = content.Page.IndexPlusOne;
            return FormatValue(format, pageNumber);
        }

        private static string GetPagePartWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            if (content is null) return "";
            var pagePart = content.Element.PagePart.ToSuffix();
            return FormatValue(format, pagePart);
        }

        private static string GetFullPathWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            if (content is null) return "";
            var fullPath = content.Element.Page.EntryFullName;
            return FormatValue(format, fullPath);
        }

        private static string GetEntryPathWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            if (content is null) return "";
            var entryPath = content.Element.Page.EntryName;
            return FormatValue(format, entryPath);
        }

        private static string GetNameWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            if (content is null) return "";
            var name = content.Element.Page.EntryLastName;
            return FormatValue(format, name);
        }

        private static string GetBitsWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            var pictureInfo = content?.Element.Page.Content?.PictureInfo;
            if (pictureInfo is null) return "";
            return FormatValue(format, pictureInfo.BitsPerPixel);
        }

        private static string GetSizeWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            var pictureInfo = content?.Element.Page.Content?.PictureInfo;
            if (pictureInfo is null) return "";
            return FormatValue(format, (int)pictureInfo.OriginalSize.Width) + " x " + FormatValue(format, (int)pictureInfo.OriginalSize.Height);
        }

        private static string GetViewScaleWord(string format, TitleSource source, string suffix)
        {
            var viewScale = source.ViewScale;
            return FormatValue(format, viewScale);
        }

        private static string GetScaleWord(string format, TitleSource source, string suffix)
        {
            var content = GetViewContent(source, suffix);
            var scale = source.ViewScale * GetOriginalScale(content) * source.DpiScale;
            return FormatValue(format, scale);

            double GetOriginalScale(ViewContent? content)
            {
                if (content is null) return 1.0;
                var pageElement = content.Element;
                return (source.FrameContent?.PageFrame.Scale ?? 1.0) * pageElement.Scale;
            }
        }

        private static string FormatValue(string format, object value)
        {
            if (value is string s && format != "")
            {
                if (string.IsNullOrEmpty(s))
                {
                    return "";
                }

                if (format[0] == '/')
                {
                    return ReplaceSeparator(format[1..], s);
                }
                else
                {
                    return StringFormatValue(format, s);
                }
            }
            else
            {
                var fmt = format == "" ? "{0}" : "{0:" + format + "}";
                return string.Format(CultureInfo.InvariantCulture, fmt, value);
            }
        }

        private static string ReplaceSeparator(string format, string value)
        {
            var separator = string.Concat(format.EscapeStringWalker().Select(e => e.Char));
            var parts = value.Split(LoosePath.Separators, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(separator, parts);
        }

        private static string StringFormatValue(string format, string value)
        {
            var numberSign = new EscapedChar('#');
            var count = format.EscapeStringWalker().Count(e => e == numberSign);

            int index = 0;
            var sb = new StringBuilder();

            foreach (var c in format.EscapeStringWalker())
            {
                if (c == numberSign)
                {
                    var x = GetStringFromEnd(value, index, count);
                    sb.Append(x);
                    index++;
                }
                else
                {
                    sb.Append(c.Char);
                }
            }

            return sb.ToString();
        }

        private static string GetStringFromEnd(string s, int index, int size)
        {
            Debug.Assert(index < size);
            if (size <= index)
            {
                return "";
            }

            var p = s.Length - (size - index);
            if (p < 0)
            {
                return "";
            }
            else if (index == 0)
            {
                return s[0..(p + 1)];
            }
            else
            {
                return s[p].ToString();
            }
        }

        public static ViewContent? GetViewContent(TitleSource _source, string suffix)
        {
            var _elements = _source.Contents;

            if (_elements.Count == 0)
            {
                return null;
            }

            return suffix switch
            {
                "" => _elements.First(),
                "1" => _elements.First(),
                "2" => _elements.Last(),
                "L" => _source.ViewContentDirection > 0 ? _elements.First() : _elements.Last(),
                "R" => _source.ViewContentDirection > 0 ? _elements.Last() : _elements.First(),
                _ => throw new NotSupportedException($"Invalid suffix: {suffix}"),
            };
        }
    }
}
