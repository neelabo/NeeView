using NeeView.Windows.Controls;
using NeeView.Windows.Media;
using System.Windows;


namespace NeeView
{
    /// <summary>
    /// 基本的なAutoHideBehavior補足
    /// </summary>
    public class BasicAutoHideDescription : AutoHideDescription
    {
        private readonly FrameworkElement _target;

        public BasicAutoHideDescription(FrameworkElement target)
        {
            _target = target;
        }

        public override bool IsVisibleLocked()
        {
            var targetElement = ContextMenuWatcher.TargetElement;
            if (targetElement != null)
            {
                return VisualTreeUtility.HasParentElement(targetElement, _target, true);
            }

            var dragElement = DragDropWatcher.DragElement;
            if (dragElement != null)
            {
                return VisualTreeUtility.HasParentElement(dragElement, _target, true);
            }

            var popupTargetElement = PopupWatcher.TargetElement;
            if (popupTargetElement != null)
            {
                return VisualTreeUtility.HasParentElement(popupTargetElement, _target, true);
            }

            var renameElement = MainWindow.Current.RenameManager.RenameElement;
            if (renameElement != null)
            {
                return VisualTreeUtility.HasParentElement(renameElement, _target);
            }

            return false;
        }
    }
}
