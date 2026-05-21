using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// Viewのスクロール操作
    /// あれ、これいる？
    /// </summary>
    public class ViewTransformContext : IPointControl
    {
        private PageFrameContext _context;
        private PageFrameContainerViewBox _viewBox;
        private PageFrameContainerCollectionRectMath _rectMath;
        private PageFrameScrollViewer _scrollViewer;


        public ViewTransformContext(PageFrameContext context, PageFrameContainerViewBox viewBox, PageFrameContainerCollectionRectMath rectMath, PageFrameScrollViewer scrollViewer)
        {
            _context = context;
            _viewBox = viewBox;
            _rectMath = rectMath;
            _scrollViewer = scrollViewer;
        }


        public Point Point => _scrollViewer.Point;
        public Point ViewPoint => _scrollViewer.ViewPoint;

        public Rect ViewRect => _viewBox.Rect;
        public Rect CanvasRect => _rectMath.GetContainersRect(_viewBox.Rect);


        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            AddPoint(value - Point, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            AddPoint(value, span, easeX, easeY, false);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY, bool areaLimit)
        {
            _scrollViewer.AddPoint(value, span, easeX, easeY, areaLimit);
        }

        public Vector GetVelocity()
        {
            return _scrollViewer.GetVelocity();
        }

        public void ResetVelocity()
        {
            _scrollViewer.ResetVelocity();
        }
    }

}
