using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView.ComponentModel
{
    public static class RectExtensions
    {
        public static bool LessThanZero(this Rect rect)
        {
            return rect.Width <= 0.0 || rect.Height <= 0.0;
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.Left + rect.Width * 0.5, rect.Top + rect.Height * 0.5);
        }

        public static double Edge(this Rect rect, PageFrameOrientation orientation, int direction)
        {
            Debug.Assert(direction is -1 or 0 or +1);
            return direction switch
            {
                -1 => rect.EdgeMin(orientation),
                +1 => rect.EdgeMax(orientation),
                _ => rect.EdgeCenter(orientation),
            };
        }

        public static double EdgeMin(this Rect rect, PageFrameOrientation orientation)
        {
            return orientation == PageFrameOrientation.Horizontal ? rect.Left : rect.Top;
        }

        public static double EdgeMax(this Rect rect, PageFrameOrientation orientation)
        {
            return orientation == PageFrameOrientation.Horizontal ? rect.Right : rect.Bottom;
        }

        public static double EdgeCenter(this Rect rect, PageFrameOrientation orientation)
        {
            return (rect.EdgeMin(orientation) + rect.EdgeMax(orientation)) * 0.5;
        }

        public static Rect Translate(this Rect rect, double dx, double dy)
        {
            return new Rect(rect.X + dx, rect.Y + dy, rect.Width, rect.Height);
        }

        public static Rect InflateValid(this Rect rect, double dx, double dy)
        {
            double x;
            double width;
            if (rect.Width + dx * 2.0 >= 0.0)
            {
                x = rect.X - dx;
                width = rect.Width + dx * 2.0;
            }
            else
            {
                x = rect.X + rect.Width * 0.5;
                width = 0.0;
            }

            double y;
            double height;
            if (rect.Height + dy * 2.0 >= 0.0)
            {
                y = rect.Y - dy;
                height = rect.Height + dy * 2.0;
            }
            else
            {
                y = rect.Y + rect.Height * 0.5;
                height = 0.0;
            }

            return new Rect(x, y, width, height);
        }

        public static Rect InflateLeft(this Rect rect, double dx)
        {
            return new Rect(rect.X - dx, rect.Y, rect.Width + dx, rect.Height);
        }

        public static Rect InflateRight(this Rect rect, double dx)
        {
            return new Rect(rect.X, rect.Y, rect.Width + dx, rect.Height);
        }

        public static Rect InflateTop(this Rect rect, double dy)
        {
            return new Rect(rect.X, rect.Y - dy, rect.Width, rect.Height + dy);
        }

        public static Rect InflateBottom(this Rect rect, double dy)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height + dy);
        }

        public static Rect InflateHorizontal(this Rect rect, double dx, int direction)
        {
            Debug.Assert(direction is -1 or 1);
            return (direction < 0) ? InflateLeft(rect, dx) : InflateRight(rect, dx);
        }

        public static Rect InflateVertical(this Rect rect, double dy, int direction)
        {
            Debug.Assert(direction is -1 or 1);
            return (direction < 0) ? InflateTop(rect, dy) : InflateBottom(rect, dy);
        }

        public static Rect Union(this IEnumerable<Rect> rects)
        {
            if (!rects.Any()) return Rect.Empty;

            var left = rects.Select(x => x.Left).Min();
            var right = rects.Select(x => x.Right).Max();
            var top = rects.Select(x => x.Top).Min();
            var bottom = rects.Select(x => x.Bottom).Max();
            return new Rect(left, top, right - left, bottom - top);
        }
    }


}
