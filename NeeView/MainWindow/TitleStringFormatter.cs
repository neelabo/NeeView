using NeeView.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NeeView
{
    public static class TitleStringFormatter
    {
        public const string BookKey = "Book";
        public const string PageMaxKey = "PageMax";
        public const string PartKey = "Part";
        public const string PageKey = "Page";
        public const string FullPathKey = "FullPath";
        public const string EntryPathKey = "EntryPath";
        public const string NameKey = "Name";
        public const string BitsKey = "Bits";
        public const string SizeKey = "Size";
        public const string ViewScaleKey = "ViewScale";
        public const string ScaleKey = "Scale";

        private static readonly Dictionary<string, KeyInfo<TitleSource>> _wordFormatMap = new Dictionary<string, KeyInfo<TitleSource>>(StringComparer.Ordinal)
        {
            [BookKey] = new(GetBookWord, StringFormatChangedAction.ViewContentChanged),
            [PageMaxKey] = new(GetPageMaxWord, StringFormatChangedAction.ViewContentChanged),
            [PartKey] = new(GetPartWord, StringFormatChangedAction.ViewContentChanged),
            [PageKey] = new(GetPageWord, StringFormatChangedAction.ViewContentChanged),
            [FullPathKey] = new(GetFullPathWord, StringFormatChangedAction.ViewContentChanged),
            [EntryPathKey] = new(GetEntryPathWord, StringFormatChangedAction.ViewContentChanged),
            [NameKey] = new(GetNameWord, StringFormatChangedAction.ViewContentChanged),
            [BitsKey] = new(GetBitsWord, StringFormatChangedAction.ViewContentChanged),
            [SizeKey] = new(GetSizeWord, StringFormatChangedAction.ViewContentChanged),
            [ViewScaleKey] = new(GetViewScaleWord, StringFormatChangedAction.ViewContentChanged | StringFormatChangedAction.ScaleChanged | StringFormatChangedAction.StretchChanged),
            [ScaleKey] = new(GetScaleWord, StringFormatChangedAction.ViewContentChanged | StringFormatChangedAction.ScaleChanged | StringFormatChangedAction.StretchChanged),
        };


        public static StringFormat<TitleSource> CreateFormatSource(string format)
        {
            return Formatter.CreateStringFormat(format, _wordFormatMap);
        }

        public static string Format(StringFormat<TitleSource> format, TitleSource source)
        {
            return Formatter.Format(format, source);
        }


        private static string GetBookWord(TitleSource source, string format, string suffix)
        {
            if (source.Book is null) return "";
            string bookName = LoosePath.GetDisplayName(source.Book.Path);
            return StringFormatTools.FormatValue(format, bookName);
        }

        private static string GetPageMaxWord(TitleSource source, string format, string suffix)
        {
            if (source.Book is null) return "";
            var pageMax = source.Book.Pages.Count;
            return StringFormatTools.FormatValue(format, pageMax);
        }

        private static string GetPageWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var pageNumber = content.Page.IndexPlusOne;
            return StringFormatTools.FormatValue(format, pageNumber);
        }

        private static string GetPartWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var pagePart = content.Element.PagePart.ToSuffix();
            return StringFormatTools.FormatValue(format, pagePart);
        }

        private static string GetFullPathWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var fullPath = content.Element.Page.EntryFullName;
            return StringFormatTools.FormatValue(format, fullPath);
        }

        private static string GetEntryPathWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var entryPath = content.Element.Page.EntryName;
            return StringFormatTools.FormatValue(format, entryPath);
        }

        private static string GetNameWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            if (content is null) return "";
            var name = content.Element.Page.EntryLastName;
            return StringFormatTools.FormatValue(format, name);
        }

        private static string GetBitsWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            var pictureInfo = content?.Element.Page.Content?.PictureInfo;
            if (pictureInfo is null) return "";
            return StringFormatTools.FormatValue(format, pictureInfo.BitsPerPixel);
        }

        private static string GetSizeWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            var pictureInfo = content?.Element.Page.Content?.PictureInfo;
            if (pictureInfo is null) return "";
            return StringFormatTools.FormatValue(format, (int)pictureInfo.OriginalSize.Width) + " x " + StringFormatTools.FormatValue(format, (int)pictureInfo.OriginalSize.Height);
        }

        private static string GetViewScaleWord(TitleSource source, string format, string suffix)
        {
            var viewScale = source.ViewScale;
            return StringFormatTools.FormatValue(format, viewScale);
        }

        private static string GetScaleWord(TitleSource source, string format, string suffix)
        {
            var content = GetContent(source, suffix);
            var scale = source.ViewScale * GetOriginalScale(content) * source.DpiScale;
            return StringFormatTools.FormatValue(format, scale);

            double GetOriginalScale(ViewContent? content)
            {
                if (content is null) return 1.0;
                var pageElement = content.Element;
                return (source.FrameContent?.PageFrame.Scale ?? 1.0) * pageElement.Scale;
            }
        }

        public static ViewContent? GetContent(TitleSource source, string suffix)
        {
            var elements = source.Contents;

            if (elements.Count == 0)
            {
                return null;
            }

            return suffix switch
            {
                "" => elements.First(),
                "1" => elements.First(),
                "2" => elements.Last(),
                "L" => source.ViewContentDirection > 0 ? elements.First() : elements.Last(),
                "R" => source.ViewContentDirection > 0 ? elements.Last() : elements.First(),
                _ => throw new NotSupportedException($"Invalid suffix: {suffix}"),
            };
        }
    }
}

