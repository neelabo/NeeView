using NeeView.Effects;
using System;
using System.Linq;

namespace NeeView
{
    public static class ExportImageSourceFactory
    {
        // TODO: PageFrameBoxPresenter のメンバでもいいかも？
        // TODO: BookAddress の取得方法。BookOperation.Current.Address はよろしくない
        public static ExportImageSource Create()
        {
            var _presenter = PageFrameBoxPresenter.Current;

            var pageFrameContent = _presenter.GetSelectedPageFrameContent();
            if (pageFrameContent is null) throw new InvalidOperationException();

            var element = pageFrameContent.ViewElement;
            if (element is null) throw new InvalidOperationException();

            var transform = pageFrameContent.ViewTransform;
            if (transform is null) throw new InvalidOperationException();

            var direction = pageFrameContent.PageFrame.Direction;

            var elements = pageFrameContent.PageFrame.Elements.Where(e => !e.IsDummy).Select(e => new PageNameElement(e.Page, e.PagePart)).ToList();

            var pages = pageFrameContent.PageFrame.Elements.Select(e => e.Page).ToList();

            var background = MainViewComponent.Current.Background;
            var bg1 = background.Bg1Brush;
            var bg2 = background.Bg2Brush;

            //var rotateTransform = new RotateTransform(viewComponent.DragTransform.Angle);
            //var scaleTransform = new ScaleTransform(viewComponent.DragTransform.ScaleX, viewComponent.DragTransform.ScaleY);
            //var transform = new TransformGroup();
            //transform.Children.Add(scaleTransform);
            //transform.Children.Add(rotateTransform);

            var context = new ExportImageSource()
            {
                PageFrameContent = pageFrameContent,
                BookAddress = BookOperation.Current.Address ?? throw new InvalidOperationException("book is null"),
                Direction = direction,
                Elements = elements,
                Pages = pages,
                View = element,
                ViewTransform = transform,
                ViewEffect = ImageEffect.Current.Effect,
                Background = bg1,
                BackgroundFront = bg2
            };

            return context;
        }

    }
}
