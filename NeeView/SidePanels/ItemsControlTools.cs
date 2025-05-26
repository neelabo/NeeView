using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeView.Windows.Media;

namespace NeeView
{
    // TODO: 名前変更？ ItemsControlVisualTreeHelper
    public static class ItemsControlTools
    {
        /// <summary>
        /// 座標から選択項目と相対レートを求める
        /// </summary>
        /// <param name="itemsControl"></param>
        /// <param name="e">マウスイベント引数</param>
        /// <param name="orientation">相対方向</param>
        /// <returns>選択項目と相対レート</returns>
        public static (ContentPresenter? item, double rate) PointToViewItemRate(ItemsControl itemsControl, DragEventArgs e, Orientation orientation)
        {
            var (item, distance) = ItemsControlTools.PointToViewItem(itemsControl, e.GetPosition(itemsControl));

            if (item is null)
            {
                return (null, 0.0);
            }

            var rate = orientation == Orientation.Horizontal
                ? e.GetPosition(item).X / item.ActualWidth
                : e.GetPosition(item).Y / item.ActualHeight;

            return (item, rate);
        }

        /// <summary>
        /// 座標から選択項目と相対距離を求める
        /// </summary>
        /// <param name="itemsControl"></param>
        /// <param name="e">マウスイベント引数</param>
        /// <returns>選択項目と相対距離</returns>
        public static (ContentPresenter? item, double distance) PointToViewItem(ItemsControl itemsControl, Point point)
        {
            // ポイントされている項目を取得
            var element = ItemsControlTools.ItemHitTest(itemsControl, point);
            if (element != null)
            {
                return (element, 0.0);
            }

            // ポイントに最も近い項目を取得
            var nearest = CollectItemContainer(itemsControl)?.Where(e => e.IsVisible)
                .Select(e => (item: e, distance: GetDistance(point, itemsControl, e)))
                .OrderBy(e => Math.Abs(e.distance))
                .FirstOrDefault();
            return nearest ?? (null, 0.0);

            static double GetDistance(Point p0, ItemsControl listBox, ContentPresenter element)
            {
                var p1 = element.TranslatePoint(new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5), listBox);
                // Y座標の差分を優先する
                return Math.Abs(p0.Y - p1.Y) * 8192 + Math.Abs(p0.X - p1.X);
            }
        }

        /// <summary>
        /// ItemsControl の項目コンテナを取得する
        /// </summary>
        public static List<ContentPresenter> CollectItemContainer(ItemsControl itemsControl)
        {
            var itemsHost = VisualTreeUtility.FindVisualChild<Panel>(itemsControl);
            if (itemsHost is null || !itemsHost.IsItemsHost) throw new InvalidOperationException("Unauthorized ItemsControl.");

            return itemsHost.Children.Cast<ContentPresenter>().ToList();
        }

        /// <summary>
        /// ItemsControl 項目コンテナのヒットテスト
        /// </summary>
        public static ContentPresenter? ItemHitTest(ItemsControl itemsControl, Point point)
        {
            var element = VisualTreeUtility.HitTest(itemsControl, point);
            if (element is null)
            {
                return null;
            }

            ContentPresenter? item = null;
            while (element is not null)
            {
                item = element as ContentPresenter;
                element = VisualTreeUtility.GetParentElement<ContentPresenter>(element, itemsControl);
            }
            return item;
        }
    }
}
