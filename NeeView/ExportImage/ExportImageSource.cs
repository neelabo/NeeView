using NeeView.PageFrames;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    public class ExportImageSource : IExportPageSource
    {
        public required PageFrameContent PageFrameContent { get; init; }

        public required string BookAddress { get; init; }

        public required int Direction { get; init; } = 1;

        public required List<PageNameElement> Elements { get; init; }

        public required List<Page> Pages { get; init; }

        public required FrameworkElement View { get; init; }

        public required Brush? Background { get; init; }

        public required Brush? BackgroundFront { get; init; }

        public required Transform ViewTransform { get; init; }

        public required EffectLayerCollection EffectLayers { get; init; }

    }

    public interface IPageNameSource
    {
        int Index { get; }
        string EntryName { get; }
        string EntryLastName { get; }
    }

    public class PageNameSource : IPageNameSource
    {
        public PageNameSource(int index, string entryName)
        {
            Index = index;
            EntryName = entryName;
        }

        public int Index { get; init; }
        public string EntryName { get; init; }
        public string EntryLastName => LoosePath.GetFileName(EntryName);
    }

    public enum ExportFileExtension
    {
        Default,
        Jpeg,
        Png,
    }


    public class PageNameElement
    {
        public PageNameElement(IPageNameSource page) : this(page, PagePart.All)
        {
        }

        public PageNameElement(IPageNameSource page, PagePart pagePart)
        {
            Page = page;
            PagePart = pagePart;
        }

        public IPageNameSource Page { get; init; }
        public PagePart PagePart { get; init; }
    }
}
