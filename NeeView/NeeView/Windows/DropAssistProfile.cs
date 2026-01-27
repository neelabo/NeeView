using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public abstract class DropAssistProfile
    {
        public abstract FrameworkElement GetAdornerTarget(FrameworkElement itemsControl);
        public abstract DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation);
        public abstract bool IsFolder(FrameworkElement? item);
        public abstract FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point);

        public virtual Rect AdjustElementRect(Rect rect) => rect;

        protected static int GetInsertOffset(double rate, bool isFolder)
        {
            if (isFolder)
            {
                return rate switch
                {
                    < 0.25 => -1,
                    > 0.75 => +1,
                    _ => 0
                };
            }
            else
            {
                return rate switch
                {
                    < 0.5 => -1,
                    _ => +1
                };
            }
        }
    }
}
