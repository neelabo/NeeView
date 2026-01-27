using NeeView.ComponentModel;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// ページ表示開始位置
    /// </summary>
    public class FramePointMath
    {
        private PageFrameContext _context;
        private Rect _rect;
        private Rect _viewRect;


        public FramePointMath(PageFrameContext context, Rect rect, Rect viewRect)
        {
            _context = context;
            _rect = rect;
            _viewRect = viewRect;
        }


        public bool ContainsHorizontal()
        {
            return _rect.Width <= _viewRect.Width + 0.01;
        }

        public bool ContainsVertical()
        {
            return _rect.Height <= _viewRect.Height + 0.01;
        }

        public HorizontalAlignment GetHorizontalAlignment(int direction)
        {
            return direction < 0 ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public VerticalAlignment GetVerticalAlignment(int direction)
        {
            return direction < 0 ? VerticalAlignment.Bottom : VerticalAlignment.Top;
        }


        public Point GetAlignedPoint(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            var o = _rect.Center();
            var x = o.X + (_viewRect.Width - _rect.Width) * horizontalAlignment.ToDirection() * 0.5;
            var y = o.Y + (_viewRect.Height - _rect.Height) * verticalAlignment.ToDirection() * 0.5;
            return new Point(x, y);
        }

    }

    public static class HorizontalAlignmentExtensions
    {
        // アライメント基準に配置する方向
        public static int ToDirection(this HorizontalAlignment horizontalAlignment)
        {
            return horizontalAlignment switch
            {
                HorizontalAlignment.Left => +1,
                HorizontalAlignment.Right => -1,
                _ => 0,
            };
        }
    }

    public static class VerticalAlignmentExtensions
    {
        // アライメント基準に配置する方向
        public static int ToDirection(this VerticalAlignment verticalAlignment)
        {
            return verticalAlignment switch
            {
                VerticalAlignment.Top => +1,
                VerticalAlignment.Bottom => -1,
                _ => 0,
            };
        }
    }



}
