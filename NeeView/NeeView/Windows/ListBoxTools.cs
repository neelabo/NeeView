using NeeView.Windows.Media;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public static class ListBoxTools
    {
        public static (ListBoxItem? item, double rate) PointToViewItemRate(ListBox listBox, DragEventArgs e, Orientation orientation)
        {
            var (item, distance) = ListBoxTools.PointToViewItem(listBox, e.GetPosition(listBox));

            if (item is null)
            {
                return (null, 0.0);
            }

            var rate = orientation == Orientation.Horizontal
                ? e.GetPosition(item).X / item.ActualWidth
                : e.GetPosition(item).Y / item.ActualHeight;

            return (item, rate);
        }

        public static (ListBoxItem? item, double distance) PointToViewItem(ListBox listBox, Point point)
        {
            // ポイントされている項目を取得
            var element = VisualTreeUtility.HitTest<ListBoxItem>(listBox, point);
            if (element != null)
            {
                return (element, 0.0);
            }

            // ポイントに最も近い項目を取得
            var nearest = VisualTreeUtility.FindVisualChildren<ListBoxItem>(listBox)?.Where(e => e.IsVisible)
                .Select(e => (item: e, distance: GetDistance(point, listBox, e)))
                .OrderBy(e => Math.Abs(e.distance))
                .FirstOrDefault();
            return nearest ?? (null, 0.0);

            static double GetDistance(Point p0, ListBox listBox, ListBoxItem element)
            {
                var p1 = element.TranslatePoint(new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5), listBox);
                // Y座標の差分を優先する
                return Math.Abs(p0.Y - p1.Y) * 8192 + Math.Abs(p0.X - p1.X);
            }
        }
    }
}