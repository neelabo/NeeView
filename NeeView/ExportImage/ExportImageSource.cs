using NeeView.PageFrames;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    public class ExportImageSource : IExportPageSource
    {
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

        public PageFrameContent PageFrameContent { get; }

        public string BookAddress { get; }

        public List<Page> Pages { get; }

        public FrameworkElement View { get; }

        public Brush? Background { get; }

        public Brush? BackgroundFront { get; }

        public Transform ViewTransform { get; }

        public Effect? ViewEffect { get; }
    }
}
