using NeeView.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public class TreeViewDropAssistProfile : DropAssistProfile
    {
        public override FrameworkElement GetAdornerTarget(FrameworkElement itemsControl)
        {
            var presenter = VisualTreeUtility.GetChildElement<ScrollContentPresenter>(itemsControl);
            Debug.Assert(presenter != null);
            return presenter;
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return true;
        }

        public override FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point)
        {
            Debug.Assert(itemsControl is TreeView);
            var item = VisualTreeUtility.HitTest<TreeViewItem>(itemsControl, point);
            return TreeViewTools.GetHeader(item);
        }

        public override DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation)
        {
            Debug.Assert(itemsControl is TreeView);
            var (item, view, rate) = TreeViewTools.PointToViewItemRate((TreeView)itemsControl, e, orientation);
            var delta = item is not null && allowInsert ? GetInsertOffset(rate, IsFolder(item)) : 0;
            return new DropTargetItem(item, view, delta);
        }
    }

}
