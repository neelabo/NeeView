using NeeView.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class ExportFileNameFormat
    {
        public const string BookKey = "Book";
        public const string PartKey = "Part";
        public const string PageKey = "Page";
        public const string EntryPathKey = "EntryPath";
        public const string NameKey = "Name";
        public const string IndexKey = "Index";

        private static readonly Dictionary<string, KeyInfo<ExportIndexPageSource>> _wordFormatMap = new Dictionary<string, KeyInfo<ExportIndexPageSource>>(StringComparer.Ordinal)
        {
            [BookKey] = new(GetBookWord),
            [PartKey] = new(GetPartWord),
            [PageKey] = new(GetPageWord),
            [EntryPathKey] = new(GetEntryPathWord),
            [NameKey] = new(GetNameWord),
            [IndexKey] = new(GetIndexWord),
        };


        public static ExportPageSource CreateDummyFileNameSource(int pageCount, int direction)
        {
            var source1 = new ExportPageSource("BookName", 1, [new PageNameElement(new PageNameSource(1, "Dir\\Foo.jpg"))]);
            var source2 = new ExportPageSource("BookName", direction, [new PageNameElement(new PageNameSource(1, "Dir\\Foo.jpg")), new PageNameElement(new PageNameSource(2, "Dir\\Bar.jpg"))]);

            return pageCount == 1 ? source1 : source2;
        }

        public static string Format(string format, ExportIndexPageSource source)
        {
            var stringFormat = Formatter.CreateStringFormat(format, _wordFormatMap);
            return Formatter.Format(stringFormat, source);
        }

        public static string Format(string format, IExportPageSource source, int index)
        {
            return Format(format, new ExportIndexPageSource(source, index));
        }

        public static string Format(string format, IExportPageSource source, int index, ExportImageMode mode, BitmapImageFormat imageFormat)
        {
            var ext = mode == ExportImageMode.Original
                ? LoosePath.GetExtension(source.Elements.First().Page.EntryName)
                : imageFormat.GetExtension();

            var name = Format(format, source, index);

            return name + ext;
        }

        private static string GetBookWord(ExportIndexPageSource source, string format, string suffix)
        {
            var bookName = LoosePath.GetFileNameWithoutExtension(source.BookAddress);
            return StringFormatTools.FormatValue(format, bookName);
        }

        private static string GetPageWord(ExportIndexPageSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var pageNumber = content.Page.Index + 1;
            return StringFormatTools.FormatValue(format, pageNumber);
        }

        private static string GetPartWord(ExportIndexPageSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var pagePart = content.PagePart.ToSuffix();
            return StringFormatTools.FormatValue(format, pagePart);
        }

        private static string GetEntryPathWord(ExportIndexPageSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var entryPath = LoosePath.WithoutExtension(content.Page.EntryName);
            return StringFormatTools.FormatValue(format, entryPath);
        }

        private static string GetNameWord(ExportIndexPageSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var name = LoosePath.WithoutExtension(content.Page.EntryLastName);
            return StringFormatTools.FormatValue(format, name);
        }

        private static string GetIndexWord(ExportIndexPageSource source, string format, string suffix)
        {
            return StringFormatTools.FormatValue(format, source.Index);
        }

        public static PageNameElement GetContent(ExportIndexPageSource source, string suffix)
        {
            var _elements = source.Elements;

            return suffix switch
            {
                "" => _elements.First(),
                "1" => _elements.First(),
                "2" => _elements.Last(),
                "L" => source.Direction > 0 ? _elements.First() : _elements.Last(),
                "R" => source.Direction > 0 ? _elements.Last() : _elements.First(),
                _ => throw new NotSupportedException($"Invalid suffix: {suffix}"),
            };
        }

    }
}

