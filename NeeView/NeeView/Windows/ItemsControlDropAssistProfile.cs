using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public class ItemsControlDropAssistProfile : DropAssistProfile
    {
        public sealed override FrameworkElement GetAdornerTarget(FrameworkElement itemsControl)
        {
            return itemsControl;
        }

        public sealed override DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation)
        {
            Debug.Assert(itemsControl is ItemsControl);
            var (item, rate) = ItemsControlTools.PointToViewItemRate((ItemsControl)itemsControl, e, orientation);
            var delta = item is not null && allowInsert ? GetInsertOffset(rate, IsFolder(item), IsParentFolder(item)) : 0;
            var isOver = item is not null && IsOver(rate);
            return new DropTargetItem(item, delta, isOver);
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return false;
        }

        public override bool IsParentFolder(FrameworkElement? item)
        {
            return false;
        }

        public sealed override FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point)
        {
            Debug.Assert(itemsControl is ItemsControl);
            return ItemsControlTools.ItemHitTest((ItemsControl)itemsControl, point);
        }
    }
}
