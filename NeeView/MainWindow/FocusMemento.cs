using System;
using System.Windows;
using System.Windows.Input;
using NeeView.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// フォーカスの保存
    /// </summary>
    /// <param name="Owner">Element の属する Window</param>
    /// <param name="Element">フォーカスを復元するコントロール</param>
    /// <param name="Point">Owner 上の座標。Element が無効の場合にこの座標でのフォーカスを試みる</param>
    public record class FocusMemento(Window Owner, WeakReference<FrameworkElement> Element, Point Point)
    {
        public static FocusMemento? Create()
        {
            var element = Keyboard.FocusedElement as FrameworkElement;

            if (element is not null)
            {
                var window = Window.GetWindow(element);
                var center = new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5);
                return new FocusMemento(window, new WeakReference<FrameworkElement>(element), element.TranslatePoint(center, window));
            }

            return null;
        }

        public void RestoreFocus()
        {
            if (this.Element.TryGetTarget(out var element) && Window.GetWindow(element) == this.Owner)
            {
                element.Focus();
                return;
            }
            else
            {
                VisualTreeUtility.HitTestToFocus(this.Owner, this.Point);
                this.Owner.Activate();
            }
        }
    }

}
