using NeeView.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public class ListBoxDropAssistProfile : DropAssistProfile
    {
        public sealed override FrameworkElement GetAdornerTarget(FrameworkElement itemsControl)
        {
            var presenter = VisualTreeUtility.GetChildElement<ScrollContentPresenter>(itemsControl);
            Debug.Assert(presenter != null);
            return presenter;
        }

        public sealed override DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation)
        {
            Debug.Assert(itemsControl is ListBox);
            var (item, rate) = ListBoxTools.PointToViewItemRate((ListBox)itemsControl, e, orientation);
            var delta = item is not null && allowInsert ? GetInsertOffset(rate, IsFolder(item)) : 0;
            return new DropTargetItem(item, delta);
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return false;
        }

        public sealed override FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point)
        {
            Debug.Assert(itemsControl is ListBox);
            return VisualTreeUtility.HitTest<ListBoxItem>(itemsControl, point);
        }
    }
}
