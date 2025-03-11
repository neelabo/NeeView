using System;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// from NeeView.DragTransformControl.NScroll
    /// v 矩形で扱うようにした
    /// v dx,dyが逆。コンテンツの移動方向
    /// v RepeatLimiter は別管理にしよう
    /// v ScrollLock は別管理にしよう
    /// </summary>
    public class NScroll
    {
        private const double _scrollCountThreshold = 0.9;

        private PageFrameContext _context;
        private Rect _contentRect;
        private Rect _viewRect;


        public NScroll(PageFrameContext context, Rect contentRect, Rect viewRect)
        {
            _context = context;
            _contentRect = contentRect;
            _viewRect = viewRect;
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="parameter">N字スクロールコマンドパラメータ</param>
        public ScrollResult ScrollN(int direction, IScrollNTypeParameter parameter, double endMargin)
        {
            return ScrollN(direction, _context.ReadOrder.ToSign(), parameter, endMargin);
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="bookReadDirection">右開き:+1 / 左開き:-1</param>
        /// <param name="parameter">N字スクロールコマンドパラメータ</param>
        public ScrollResult ScrollN(int direction, int bookReadDirection, IScrollNTypeParameter parameter, double endMargin)
        {
            //var endMargin = parameter is IScrollNTypeEndMargin e ? e.EndMargin : 0.0;
            return ScrollN(direction, bookReadDirection, parameter.ScrollType, parameter.Scroll, endMargin);
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="bookReadDirection">右開き:+1 / 左開き:-1</param>
        /// <param name="scrollType">スクロールのタイプ</param>
        /// <param name="minScroll">最小移動距離</param>
        /// <param name="rate">移動距離の割合</param>
        /// <param name="endMargin">終端判定マージン</param>
        private ScrollResult ScrollN(int direction, int bookReadDirection, NScrollType scrollType, double rate, double endMargin)
        {
            var delta = GetNScrollDelta(direction, bookReadDirection, scrollType, rate, endMargin);
            return new ScrollResult(scrollType, delta);
        }


        // N字スクロール：スクロール距離を計算
        private Vector GetNScrollDelta(int direction, int bookReadDirection, NScrollType scrollType, double rate, double endMargin)
        {
            var area = new DragArea(_viewRect, _contentRect);

            return scrollType switch
            {
                NScrollType.NType => GetNTypeScrollDelta(area, direction, bookReadDirection, rate, endMargin),
                NScrollType.ZType => GetZTypeScrollDelta(area, direction, bookReadDirection, rate, endMargin),
                NScrollType.Diagonal => GetDiagonalScrollDelta(area, direction, bookReadDirection, rate, endMargin),
                NScrollType.Horizontal => GetHorizontalScrollDelta(area, direction, bookReadDirection, rate, endMargin),
                NScrollType.Vertical => GetVerticalScrollDelta(area, direction, bookReadDirection, rate, endMargin),
                _ => throw new NotSupportedException()
            };
        }

        private Vector GetNTypeScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMargin)
        {
            var delta = SnapZero(GetNScrollVertical(area, direction, bookReadDirection, rate), endMargin);
            if (delta.Y == 0.0)
            {
                delta = SnapZero(GetNScrollNewLineVertical(area, direction, bookReadDirection, rate), endMargin);
            }
            return delta;
        }

        private Vector GetZTypeScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMargin)
        {
            var delta = SnapZero(GetNScrollHorizontal(area, direction, bookReadDirection, rate), endMargin);
            if (delta.X == 0.0)
            {
                delta = SnapZero(GetNScrollNewLineHorizontal(area, direction, bookReadDirection, rate), endMargin);
            }
            return delta;
        }

        private static Vector GetDiagonalScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMargin)
        {
            var deltaX = GetNScrollHorizontal(area, direction, bookReadDirection, rate);
            var deltaY = GetNScrollVertical(area, direction, bookReadDirection, rate);
            return SnapZero(new Vector(deltaX.X, deltaY.Y), endMargin);
        }

        private static Vector GetHorizontalScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMargin)
        {
            var deltaX = GetNScrollHorizontal(area, direction, bookReadDirection, rate);
            return SnapZero(new Vector(deltaX.X, 0.0), endMargin);
        }

        private static Vector GetVerticalScrollDelta(DragArea area, int direction, int bookReadDirection, double rate, double endMargin)
        {
            var deltaY = GetNScrollVertical(area, direction, bookReadDirection, rate);
            return SnapZero(new Vector(0.0, deltaY.Y), endMargin);
        }

        private static Vector GetNScrollHorizontal(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            if (direction * bookReadDirection > 0)
            {
                delta.X = SnapZero(GetNScrollHorizontalToRight(area, rate));
            }
            else
            {
                delta.X = SnapZero(GetNScrollHorizontalToLeft(area, rate));
            }

            return delta;
        }

        private static Vector GetNScrollVertical(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            if (direction > 0)
            {
                delta.Y = SnapZero(GetNScrollVerticalToBottom(area, rate));
            }
            else
            {
                delta.Y = SnapZero(GetNScrollVerticalToTop(area, rate));
            }

            return delta;
        }

        // N字スクロール改行：水平方向
        private static Vector GetNScrollNewLineHorizontal(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            var canHorizontalScroll = area.Over.Width > 0.0;
            var rateY = canHorizontalScroll ? 1.0 : rate;
            if (direction > 0)
            {
                delta.Y = SnapZero(GetNScrollVerticalToBottom(area, rateY));
            }
            else
            {
                delta.Y = SnapZero(GetNScrollVerticalToTop(area, rateY));
            }

            if (delta.Y != 0.0)
            {
                if (direction * bookReadDirection > 0)
                {
                    delta.X = SnapZero(GetNScrollHorizontalMoveToLeft(area));
                }
                else
                {
                    delta.X = SnapZero(GetNScrollHorizontalMoveToRight(area));
                }
            }

            return delta;
        }

        // N字スクロール改行：垂直方向
        private static Vector GetNScrollNewLineVertical(DragArea area, int direction, int bookReadDirection, double rate)
        {
            var delta = new Vector();

            var canVerticalScroll = area.Over.Height > 0.0;
            var rateX = canVerticalScroll ? 1.0 : rate;
            if (direction * bookReadDirection > 0)
            {
                delta.X = SnapZero(GetNScrollHorizontalToRight(area, rateX));
            }
            else
            {
                delta.X = SnapZero(GetNScrollHorizontalToLeft(area, rateX));
            }

            if (delta.X != 0.0)
            {
                if (direction > 0)
                {
                    delta.Y = SnapZero(GetNScrollVerticalMoveToTop(area));
                }
                else
                {
                    delta.Y = SnapZero(GetNScrollVerticalMoveToBottom(area));
                }
            }

            return delta;
        }

        /// <summary>
        /// Snap zero.
        /// if abs(x) < margin, then 0.0
        /// </summary>
        private static double SnapZero(double value, double margin = 1.0)
        {
            return -margin < value && value < margin ? 0.0 : value;
        }

        private static Vector SnapZero(Vector v, double margin)
        {
            if (v.IsZero())
            {
                return v;
            }

            if (v.LengthSquared < margin * margin)
            {
                return new Vector();
            }

            return v;
        }


        // N字スクロール：上方向スクロール距離取得
        private static double GetNScrollVerticalToTop(DragArea area, double rate)
        {
            if (area.Over.Top < 0.0)
            {
                double dy = Math.Abs(area.Over.Top);
                var n = (int)(dy / (area.ViewRect.Height * rate) + _scrollCountThreshold);
                if (n > 1)
                {
                    dy = Math.Min(dy / n, dy);
                }
                return dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：下方向スクロール距離取得
        private static double GetNScrollVerticalToBottom(DragArea area, double rate)
        {
            if (area.Over.Bottom > 0.0)
            {
                double dy = Math.Abs(area.Over.Bottom);
                var n = (int)(dy / (area.ViewRect.Height * rate) + _scrollCountThreshold);
                if (n > 1)
                {
                    dy = Math.Min(dy / n, dy);
                }
                return -dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：上端までの移動距離取得
        private static double GetNScrollVerticalMoveToTop(DragArea area)
        {
            return Math.Abs(area.Over.Top);
        }

        // N字スクロール：下端までの移動距離取得
        private static double GetNScrollVerticalMoveToBottom(DragArea area)
        {
            return -Math.Abs(area.Over.Bottom);
        }

        // N字スクロール：左端までの移動距離取得
        private static double GetNScrollHorizontalMoveToLeft(DragArea area)
        {
            return Math.Abs(area.Over.Left);
        }

        // N字スクロール：右端までの移動距離取得
        private static double GetNScrollHorizontalMoveToRight(DragArea area)
        {
            return -Math.Abs(area.Over.Right);
        }

        // N字スクロール：左方向スクロール距離取得
        private static double GetNScrollHorizontalToLeft(DragArea area, double rate)
        {
            if (area.Over.Left < 0.0)
            {
                double dx = Math.Abs(area.Over.Left);
                var n = (int)(dx / (area.ViewRect.Width * rate) + _scrollCountThreshold);
                if (n > 1)
                {
                    dx = Math.Min(dx / n, dx);
                }
                return dx;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：右方向スクロール距離取得
        private static double GetNScrollHorizontalToRight(DragArea area, double rate)
        {
            if (area.Over.Right > 0.0)
            {
                double dx = Math.Abs(area.Over.Right);
                var n = (int)(dx / (area.ViewRect.Width * rate) + _scrollCountThreshold);
                if (n > 1)
                {
                    dx = Math.Min(dx / n, dx);
                }
                return -dx;
            }
            else
            {
                return 0.0;
            }
        }
    }
}
