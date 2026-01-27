using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    public interface IHasWindowController
    {
        WindowController WindowController { get; }
    }

    public class WindowController
    {
        readonly Window _window;
        readonly WindowStateManager _windowStateManager;


        public WindowController(Window window, WindowStateManager windowStateManager)
        {
            _window = window;
            _windowStateManager = windowStateManager;
        }


        public WindowStateManager WindowStateManager => _windowStateManager;

        public virtual bool IsTopmost
        {
            get => _window.Topmost;
            set => _window.Topmost = value;
        }


        public void ToggleMinimize()
        {
            _windowStateManager.ToggleMinimize();
        }

        public void ToggleMaximize()
        {
            _windowStateManager.ToggleMaximize();
        }

        public void ToggleFullScreen()
        {
            _windowStateManager.ToggleFullScreen();
        }

        public void SetFullScreen(bool isFullScreen)
        {
            _windowStateManager.SetFullScreen(isFullScreen);
        }

        public void ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
        }
    }
}
