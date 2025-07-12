using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;

namespace NeeView.Windows
{
    public class WindowChromeSource
    {
        private readonly Window _window;
        private readonly WindowChrome _windowChrome;
        private readonly WindowChromePatch _windowChromePatch;
        private readonly SnapLayoutPresenter _snapLayoutPresenter;


        public WindowChromeSource(Window window) : this(window, CreateDefaultWindowChrome())
        {
        }

        public WindowChromeSource(Window window, WindowChrome chrome)
        {
            _window = window;
            _windowChrome = chrome;
            _windowChromePatch = new WindowChromePatch(_window, _windowChrome);

            // NOTE: SnapLayoutPresenter の WndProc をここで登録。順番によっては WM_NCHITTEST 等のメッセージが受信できなくなるため。
            _snapLayoutPresenter = new SnapLayoutPresenter(_window);

            // 操作無効を伝達
            _window.IsEnabledChanged += (s, e) => UpdateSnapLayoutEnabled();
            _window.IsHitTestVisibleChanged += (s, e) => UpdateSnapLayoutEnabled();
        }


        public WindowChrome WindowChrome => _windowChrome;
        public WindowChromePatch WindowChromePatch => _windowChromePatch;
        public SnapLayoutPresenter SnapLayoutPresenter => _snapLayoutPresenter;


        private static WindowChrome CreateDefaultWindowChrome()
        {
            var chrome = new WindowChrome();
            chrome.CornerRadius = new CornerRadius();
            chrome.UseAeroCaptionButtons = false;
            chrome.CaptionHeight = 0;
            chrome.GlassFrameThickness = new Thickness(1, 30, 1, 1); // Win11SnapLayoutを機能させるため上部フレーム領域を設定する
            chrome.ResizeBorderThickness = new Thickness(4);
            return chrome;
        }

        public void SetMaximizeButtonSource(IMaximizeButtonSource? maximizeButton)
        {
            _snapLayoutPresenter.SetMaximizeButtonSource(maximizeButton);
        }

        private void UpdateSnapLayoutEnabled()
        {
            _snapLayoutPresenter.IsEnabled = _window.IsEnabled && _window.IsHitTestVisible;
        }
    }
}
