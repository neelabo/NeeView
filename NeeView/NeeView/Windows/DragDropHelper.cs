// from https://github.com/takanemu/WPFDragAndDropSample

using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    /// <summary>
    /// ドラッグ開始距離判定
    /// </summary>
    public static class DragDropHelper
    {
        public static bool IsDragDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// ドラッグが端にある時に自動スクロールさせる.
        /// </summary>
        public static bool AutoScroll(object? sender, DragEventArgs e)
        {
            // NOTE: エクスプローラーからのドラッグにはScrollフラグがついていないので判定しない
            ////if ((e.AllowedEffects & DragDropEffects.Scroll) == 0)
            ////{
            ////    return;
            ////}

            if (sender is not FrameworkElement container)
            {
                return false;
            }

            ScrollViewer? scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(container);
            if (scrollViewer == null)
            {
                return false;
            }

            var point = e.GetPosition(scrollViewer);
            if (double.IsNaN(point.X))
            {
                return false;
            }

            double margin = 32.0;
            double offset = VirtualizingPanel.GetScrollUnit(container) == ScrollUnit.Pixel ? 32.0 : 1.0;
            double oldVerticalOffset = scrollViewer.VerticalOffset;

            if (point.Y < 0.0 + margin)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
                scrollViewer.UpdateLayout();
                return oldVerticalOffset != scrollViewer.VerticalOffset;
            }
            else if (point.Y > scrollViewer.ActualHeight - margin)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
                scrollViewer.UpdateLayout();
                return oldVerticalOffset != scrollViewer.VerticalOffset;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// DragOver,DragEnterを無効にする終端処理を設定
        /// </summary>
        /// <param name="element"></param>
        public static void AttachDragOverTerminator(FrameworkElement element)
        {
            element.AllowDrop = true;
            element.DragOver += Element_DragOverTerminator;
            element.DragEnter += Element_DragOverTerminator;

            static void Element_DragOverTerminator(object sender, DragEventArgs e)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
}
