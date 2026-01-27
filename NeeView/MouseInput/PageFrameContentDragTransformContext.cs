using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 表示座標系操作のリソース : ページフレーム用
    /// </summary>
    public class PageFrameContentDragTransformContext : ContentDragTransformContext
    {
        private readonly PageFrameContainer _container;
        private readonly ICanvasToViewTranslator _canvasToViewTranslator;


        public PageFrameContentDragTransformContext(FrameworkElement sender, ITransformControl transform, PageFrameContainer container, ICanvasToViewTranslator canvasToViewTranslator, ViewConfig viewConfig, MouseConfig mouseConfig)
            : base(sender, transform, viewConfig, mouseConfig)
        {
            _container = container;
            _canvasToViewTranslator = canvasToViewTranslator;
            ContentRect = CreateContentRect(_container);
        }


        public override void Initialize(Point point, int timestamp)
        {
            base.Initialize(point, timestamp);

            RotateCenter = GetCenterPosition(ViewConfig.RotateCenter, false);
            ScaleCenter = GetCenterPosition(ViewConfig.ScaleCenter, true);
            FlipCenter = GetCenterPosition(ViewConfig.FlipCenter, false);
        }

        private Point GetCenterPosition(DragControlCenter dragControlCenter, bool allowAuto)
        {
            return dragControlCenter switch
            {
                DragControlCenter.View => ViewRect.Center(), // NOTE: 常に(0,0)
                DragControlCenter.Target => ContentRect.Center(),
                DragControlCenter.Cursor => First,
                DragControlCenter.Auto => allowAuto ? GetAutoCenterPosition() : ViewRect.Center(),
                _ => throw new NotImplementedException(),
            };
        }

        private Point GetAutoCenterPosition()
        {
            var center = new Point(0.0, 0.0);

            if (AutoCenterContext.CanAutoCenterX)
            {
                var xa = ViewRect.Left + ContentRect.Width * 0.5;
                var xb = ViewRect.Right - ContentRect.Width * 0.5;
                if (Math.Abs(xb - xa) > 0.01)
                {
                    AutoCenterContext.RateX = (ContentRect.Center().X - xa) / (xb - xa);
                }
                center.X = ViewRect.Left + ViewRect.Width * AutoCenterContext.RateX;
            }

            if (AutoCenterContext.CanAutoCenterY)
            {
                var ya = ViewRect.Top + ContentRect.Height * 0.5;
                var yb = ViewRect.Bottom - ContentRect.Height * 0.5;
                if (Math.Abs(yb - ya) > 0.01)
                {
                    AutoCenterContext.RateY = (ContentRect.Center().Y - ya) / (yb - ya);
                }
                center.Y = ViewRect.Top + ViewRect.Height * AutoCenterContext.RateY;
            }

            return center;
        }

        public override PageFrameContent? GetPageFrameContent()
        {
            return _container.Content as PageFrameContent;
        }

        public override void UpdateRect()
        {
            base.UpdateRect();

            UpdateViewRect();
            ContentRect = CreateContentRect(_container);
        }

        private Rect CreateContentRect(PageFrameContainer container)
        {
            var rect = container.GetContentRect();
            var p0 = _canvasToViewTranslator.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.TopLeft));
            var p1 = _canvasToViewTranslator.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.BottomRight));
            var contentRect = new Rect(p0, p1);
            return contentRect;
        }
    }

}
