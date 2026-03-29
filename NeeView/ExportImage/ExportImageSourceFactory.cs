using NeeView.Effects;
using System;
using System.Linq;

namespace NeeView
{
    public static class ExportImageSourceFactory
    {
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
