using System;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    public static class FocusTools
    {
        /// <summary>
        /// 要素の所属するウィンドウがアクティブな場合のみフォーカスする
        /// </summary>
        /// <param name="element">フォーカスする要素</param>
        /// <returns>フォーカスできた場合は true</returns>
        public static bool FocusIfWindowActive(UIElement element)
        {
            if (element is null) return false;

            var window = Window.GetWindow(element);
            if (window is null || !window.IsActive)
            {
                return false;
            }

            return element.Focus();
        }

        /// <summary>
        /// 表示直後のフォーカスができないことがある場合の保証
        /// </summary>
        /// <param name="control">表示判定を行うコントロール</param>
        /// <param name="focusAction">フォーカス処理</param>
        public static void FocusAtOnce(FrameworkElement control, Action focusAction)
        {
            if (control.IsVisible)
            {
                control.Dispatcher.BeginInvoke(() =>
                {
                    focusAction();
                }, DispatcherPriority.Input);
            }
            else
            {
                control.IsVisibleChanged += Control_IsVisibleChanged;
            }

            void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                control.IsVisibleChanged -= Control_IsVisibleChanged;
                if (control.IsVisible)
                {
                    control.Dispatcher.BeginInvoke(() =>
                    {
                        focusAction();
                    }, DispatcherPriority.Input);
                }
            }
        }
    }

}
