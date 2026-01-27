using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// DragDrop状態を監視
    /// </summary>
    public class DragDropWatcher
    {
        public static event EventHandler<TargetElementChangedEventArgs>? TargetElementChanged;

        /// <summary>
        /// 現在のドラッグターゲット
        /// </summary>
        public static UIElement? DragElement { get; private set; }

        /// <summary>
        /// DragStartBehavior用フック
        /// </summary>
        public static DragDropHook DragDropHook { get; } = new DragDropHook();


        public static void SetDragElement(object sender, UIElement? element)
        {
            if (DragElement != element)
            {
                DragElement = element;
                TargetElementChanged?.Invoke(sender, new TargetElementChangedEventArgs(element));
            }
        }

    }

    /// <summary>
    /// DragStartBehavior用フック
    /// </summary>
    public class DragDropHook : IDragDropHook
    {
        public void BeginDragDrop(object sender, DependencyObject dragSource, object data, DragDropEffects allowedEffects)
        {
            if (dragSource is UIElement element)
            {
                DragDropWatcher.SetDragElement(sender, element);
            }
        }

        public void EndDragDrop(object sender, DependencyObject dragSource, object data, DragDropEffects allowedEffects)
        {
            DragDropWatcher.SetDragElement(sender, null);
        }
    }

}
