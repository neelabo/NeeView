using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ポップアップ外の MouseWheel 操作を無効化する
    /// </summary>
    public class DisablePopupBackgroundWheel : IDisposable
    {
        private readonly Window? _window;
        private readonly Popup _popup;
        private bool _disposedValue;

        public DisablePopupBackgroundWheel(UIElement popupTarget, Popup popup)
        {
            _window = Window.GetWindow(popupTarget);
            _popup = popup;

            _window?.PreviewMouseWheel += BlockMouseWheel;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _window?.PreviewMouseWheel -= BlockMouseWheel;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void BlockMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_popup.Child.IsMouseOver)
            {
                e.Handled = true;
            }
        }
    }


}
