using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public abstract class DropAssistProfile
    {
        public abstract FrameworkElement GetAdornerTarget(FrameworkElement itemsControl);
        public abstract DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation);
        public abstract bool IsFolder(FrameworkElement? item);
        public abstract bool IsParentFolder(FrameworkElement? item);
        public abstract FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point);

        public virtual Rect AdjustElementRect(Rect rect) => rect;

        protected static int GetInsertOffset(double rate, bool isFolder, bool isParentFolder)
        {
            if (isParentFolder)
            {
                return rate switch
                {
                    > 1.0 => +1,
                    _ => 0
                };
            }
            else if (isFolder)
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

        protected static bool IsOver(double rate)
        {
            return 0.0 <= rate && rate <= 1.0;
        }
    }
}
