using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    public class ExportImageSource : IExportPageSource
    {
#if false
        public ExportImageSource(PageFrameContent pageFrameContent, string bookAddress, List<Page> pages, FrameworkElement view, Brush? background, Brush? backgroundFront, Transform viewTransform, Effect? viewEffect)
        {
            PageFrameContent = pageFrameContent;
            BookAddress = bookAddress;
            Pages = pages;
            View = view;
            Background = background;
            BackgroundFront = backgroundFront;
            ViewTransform = viewTransform;
            ViewEffect = viewEffect;
        }
#endif

        public required PageFrameContent PageFrameContent { get; init; }

        public required string BookAddress { get; init; }

        public required int Direction { get; init; } = 1;

        public required List<PageNameElement> Elements { get; init; }

        public required List<Page> Pages { get; init; }

        public required FrameworkElement View { get; init; }

        public required Brush? Background { get; init; }

        public required Brush? BackgroundFront { get; init; }

        public required Transform ViewTransform { get; init; }

        public required Effect? ViewEffect { get; init; }

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
