using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    // {Book} ... ブック名
    // {Name} ... ページファイル名
    // {EntryPath} .... ページのエントリパス。ブックを基準とした相対パス
    // {Page} ... ページ番号 (数字)
    // {Index} ... 連番 (数字)
    //
    // .NET の String.Format 書式
    // 例:{Index:000} → 001,002,...
    //
    //  2ページを1画像に出力する場合のページに属する変数には [1,2,L,R] のサフィックスが使用可能 (1=基準ページ、2=次のページ、L=左ページ、R=右ページ)
    // 例:{Book}_{Page1}-{Page2}

    public static class ExportFileNameFormat
    {
        public const string BookKey = "Book";
        public const string NameKey = "Name";
        public const string EntryPathKey = "EntryPath";
        public const string PageKey = "Page";
        public const string IndexKey = "Index";

        public static PageNameElement GetPageElement(IExportPageSource _source, string suffix)
        {
            var _elements = _source.Elements;

            return suffix switch
            {
                "" => _elements.First(),
                "1" => _elements.First(),
                "2" => _elements.Last(),
                "L" => _source.Direction > 0 ? _elements.Last() : _elements.First(),
                "R" => _source.Direction > 0 ? _elements.First() : _elements.Last(),
                _ => throw new NotSupportedException($"Invalid suffix: {suffix}"),
            };
        }

        private static IPageNameSource GetPage(IExportPageSource _source, string suffix)
        {
            var element = GetPageElement(_source, suffix);
            return element.Page;
        }

        public static string Format(string format, IExportPageSource _source, int index)
        {
            var (words, newFormat) = StringFormatParser.Parse(format);

            List<object?> args = new();

            foreach (var w in words)
            {
                var tokens = w.Split(':', 2);

                var word = tokens[0].Trim();
                var fmt = tokens.Length > 1 ? tokens[1] : null;

                if (word == BookKey)
                {
                    var s = LoosePath.GetFileNameWithoutExtension(_source.BookAddress);
                    s = FormatString(s, fmt);
                    args.Add(s);
                }
                else if (word.StartsWith(NameKey))
                {
                    var s = LoosePath.WithoutExtension(GetPage(_source, GetSuffix(word, NameKey)).EntryLastName);
                    s = FormatString(s, fmt);
                    args.Add(s);
                }
                else if (word.StartsWith(EntryPathKey))
                {
                    var s = LoosePath.WithoutExtension(GetPage(_source, GetSuffix(word, EntryPathKey)).EntryName);
                    s = FormatString(s, fmt);
                    args.Add(s);
                }
                else if (word.StartsWith(PageKey))
                {
                    var element = GetPageElement(_source, GetSuffix(word, PageKey));
                    var n = element.Page.Index + 1;
                    var s = FormatString(n, fmt) + element.PagePart.ToSuffix();
                    args.Add(s);
                }
                else if (word == IndexKey)
                {
                    var n = index;
                    var s = FormatString(n, fmt);
                    args.Add(s);
                }
                else
                {
                    throw new NotSupportedException($"Unknown: {word}");
                }
            }

            return string.Format(newFormat, args.ToArray());
        }

        private static string GetSuffix(string value, string key)
        {
            Debug.Assert(value.StartsWith(key));
            return value.Substring(key.Length);
        }

        private static string FormatString<T>(T value, string? fmt)
            where T : notnull
        {
            if (fmt is null)
            {
                return value.ToString() ?? "";
            }
            else
            {
                return string.Format($"{{0:{fmt}}}", value);
            }
        }

        /// <summary>
        /// 実際にフォーマットしてみる
        /// </summary>
        /// <param name="source">入力ページの情報</param>
        /// <param name="format">出力ネームフォーマット</param>
        /// <param name="mode">オリジナル出力/ビュー出力</param>
        /// <param name="imageFormat">ビュー出力のときの画像種類</param>
        /// <returns></returns>
        public static string Format(string format, IExportPageSource source, int index, ExportImageMode mode, BitmapImageFormat imageFormat)
        {
            var ext = mode == ExportImageMode.Original
                ? LoosePath.GetExtension(source.Elements.First().Page.EntryName)
                : imageFormat.GetExtension();

            var name = ExportFileNameFormat.Format(format, source, index);

            return name + ext;
        }


        public static ExportPageSource CreateDummyFileNameSource(int pageCount, int direction)
        {
            var source1 = new ExportPageSource("BookName", 1, [new PageNameElement(new PageNameSource(1, "Dir\\Foo.jpg"))]);
            var source2 = new ExportPageSource("BookName", direction, [new PageNameElement(new PageNameSource(1, "Dir\\Foo.jpg")), new PageNameElement(new PageNameSource(2, "Dir\\Bar.jpg"))]);

            return pageCount == 1 ? source1 : source2;
        }
    }

}

