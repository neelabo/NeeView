using NeeView.Effects;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    public class ExportImageSource
    {
        public ExportImageSource(PageFrameContent pageFrameContent, string? bookAddress, List<Page> pages, FrameworkElement view, Brush? background, Brush? backgroundFront, Transform viewTransform, Effect? viewEffect)
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

        public PageFrameContent PageFrameContent { get; private set; }

        public string? BookAddress { get; private set; }

        public List<Page> Pages { get; private set; }

        public FrameworkElement View { get; private set; }

        public Brush? Background { get; private set; }

        public Brush? BackgroundFront { get; private set; }


        public Transform ViewTransform { get; private set; }

        public Effect? ViewEffect { get; private set; }


        public static ExportImageSource Create()
        {
            var _presenter = PageFrameBoxPresenter.Current;
            var pageFrameContent = _presenter.GetSelectedPageFrameContent();
            if (pageFrameContent is null) throw new InvalidOperationException();

            var element = pageFrameContent.ViewElement;
            if (element is null) throw new InvalidOperationException();

            var transform = pageFrameContent.ViewTransform;
            if (transform is null) throw new InvalidOperationException();

            var pages = pageFrameContent.PageFrame.Elements.Select(e => e.Page).ToList();

            var background = MainViewComponent.Current.Background;
            var bg1 = background.Bg1Brush;
            var bg2 = background.Bg2Brush;

            //var rotateTransform = new RotateTransform(viewComponent.DragTransform.Angle);
            //var scaleTransform = new ScaleTransform(viewComponent.DragTransform.ScaleX, viewComponent.DragTransform.ScaleY);
            //var transform = new TransformGroup();
            //transform.Children.Add(scaleTransform);
            //transform.Children.Add(rotateTransform);

            var context = new ExportImageSource(
                pageFrameContent: pageFrameContent,
                bookAddress: BookOperation.Current.Address,
                pages: pages,
                view: element,
                viewTransform: transform,
                viewEffect: ImageEffect.Current.Effect,
                background: bg1,
                backgroundFront: bg2
            );

            return context;
        }

    }
}
