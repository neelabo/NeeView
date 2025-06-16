using NeeView.Windows.Media;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public static class TreeViewTools
    {
        public static (TreeViewItem? item, FrameworkElement? header, double rate) PointToViewItemRate(TreeView treeView, DragEventArgs e, Orientation orientation)
        {
            var (item, view, distance) = TreeViewTools.PointToViewItem(treeView, e.GetPosition(treeView));

            if (item is null)
            {
                return (null, null, 0.0);
            }

            view ??= item;

            var rate = orientation == Orientation.Horizontal
                ? e.GetPosition(view).X / view.ActualWidth
                : e.GetPosition(view).Y / view.ActualHeight;

            return (item, view, rate);
        }

        public static (TreeViewItem? item, FrameworkElement? header, double distance) PointToViewItem(TreeView treeView, Point point)
        {
            // ポイントされている項目を取得
            var element = VisualTreeUtility.HitTest<TreeViewItem>(treeView, point);
            if (element != null)
            {
                return (element, GetHeader(element), 0.0);
            }

            // ポイントに最も近い項目を取得
            var nearest = VisualTreeUtility.FindVisualChildren<TreeViewItem>(treeView)?.Where(e => e.IsVisible)
                .Select(e => (item: e, view: GetHeader(e), distance: GetDistance(point, treeView, e)))
                .OrderBy(e => Math.Abs(e.distance))
                .FirstOrDefault();
            return nearest ?? (null, null, 0.0);

            static double GetDistance(Point p0, TreeView treeView, TreeViewItem element)
            {
                var p1 = element.TranslatePoint(new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5), treeView);
                // Y座標の差分を優先する
                return Math.Abs(p0.Y - p1.Y) * 8192 + Math.Abs(p0.X - p1.X);
            }
        }

        public static FrameworkElement? GetHeader(TreeViewItem? item)
        {
            if (item is null) return null;
            return VisualTreeUtility.FindVisualChild<ContentPresenter>(item, "PART_Header");

            // TODO: DataTemplate から検索できるようにする
            //return VisualTreeUtility.GetChildElement<ContentPresenter>(item, "PART_Header");
        }
    }
}