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

        public bool IsFullScreen => _windowStateManager.IsFullScreen;
        public bool IsFullDesktop => _windowStateManager.IsFullDesktop;


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

        public void ToggleFullDesktop()
        {
            _windowStateManager.ToggleFullDesktop();
        }

        public void SetFullScreen(bool isFullScreen)
        {
            _windowStateManager.SetFullScreen(isFullScreen);
        }

        public void SetFullDesktop(bool isFullDesktop)
        {
            _windowStateManager.SetFullDesktop(isFullDesktop);
        }

        public void ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
        }
    }
}
