using System.Windows;

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
    }

}
