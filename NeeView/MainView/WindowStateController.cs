using System;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class WindowStateController
    {
        public IHasWindowController _defaultController;

        public WindowStateController(IHasWindowController defaultController)
        {
            if (defaultController is null) throw new ArgumentNullException(nameof(defaultController));

            _defaultController = defaultController;
        }

        public bool IsFullScreen => GetWindowController(null).IsFullScreen;
        public bool IsFullDesktop => GetWindowController(null).IsFullDesktop;

        public void ToggleMinimize(object? sender)
        {
            GetWindowController(sender)?.ToggleMinimize();
        }

        public void ToggleMaximize(object? sender)
        {
            GetWindowController(sender)?.ToggleMaximize();
        }

        public void ToggleFullScreen(object? sender)
        {
            GetWindowController(sender)?.ToggleFullScreen();
        }

        public void ToggleFullDesktop(object? sender)
        {
            GetWindowController(sender)?.ToggleFullDesktop();
        }

        public void SetFullScreen(object? sender, bool isFullScreen)
        {
            GetWindowController(sender).SetFullScreen(isFullScreen);
        }

        public void SetFullDesktop(object? sender, bool isFullDesktop)
        {
            GetWindowController(sender).SetFullDesktop(isFullDesktop);
        }

        public void ToggleTopmost(object? sender)
        {
            GetWindowController(sender).ToggleTopmost();
        }



        // NOTE: no use sender
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private WindowController GetWindowController(object? sender)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) as IHasWindowController;
            return (window ?? _defaultController).WindowController;
        }
    }

}
