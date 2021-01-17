﻿using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    public class WindowBorder : BindableBase
    {
        private Window _window;
        private WindowChromeAccessor _windowChromeAccessor;
        private Thickness _thickness;
        private WeakBindableBase<WindowConfig> _windowConfig;

        public WindowBorder(Window window, WindowChromeAccessor windowChromeAccessor)
        {
            _window = window;
            _windowChromeAccessor = windowChromeAccessor;

            _window.DpiChanged +=
                (s, e) => Update();

            _window.StateChanged +=
                (s, e) => Update();

            _windowChromeAccessor.AddPropertyChanged(nameof(WindowChromeAccessor.IsEnabled),
                (s, e) => Update());

            _windowConfig = new WeakBindableBase<WindowConfig>(Config.Current.Window);
            _windowConfig.AddPropertyChanged(nameof(WindowConfig.WindowChromeFrame),
                (s, e) => Update());

            Update();
        }

        public Thickness Thickness
        {
            get { return _thickness; }
            set { SetProperty(ref _thickness, value); }
        }

        public void Update()
        {
            // NOTE: Windows7 only
            if (!Windows7Tools.IsWindows7) return;

            if (_window.WindowState == WindowState.Minimized) return;

            if (_windowChromeAccessor.IsEnabled && _window.WindowState == WindowState.Normal && Config.Current.Window.WindowChromeFrame == WindowChromeFrame.WindowFrame)
            {
                var dipScale = (_window is IDpiScaleProvider dipProvider) ? dipProvider.GetDpiScale() : new DpiScale(1.0, 1.0);
                var x = 1.0 / dipScale.DpiScaleX;
                var y = 1.0 / dipScale.DpiScaleY;
                this.Thickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.Thickness = default;
            }
        }
    }
}
