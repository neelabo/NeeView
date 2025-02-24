using NeeLaboratory.ComponentModel;
using NeeView.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class SystemVisualParameters : BindableBase
    {
        static SystemVisualParameters() => Current = new SystemVisualParameters();
        public static SystemVisualParameters Current { get; }


        private string _messageFontName;
        private double _messageFontSize;
        private double _menuFontSize;
        private bool _isHighContrast;
        private SystemThemeType _theme;
        private Color _accentColor = Colors.RoyalBlue;


        private SystemVisualParameters()
        {
            // NOTE: nullable 警告回避
            _messageFontName = SystemFonts.MessageFontFamily.Source;

            UpdateFonts();
            UpdateColors();

            SystemDeviceWatcher.Current.SettingChanged += WindowMessage_SettingChanged;
        }


        public string MessageFontName
        {
            get { return _messageFontName; }
            set { SetProperty(ref _messageFontName, value); }
        }

        public double MessageFontSize
        {
            get { return _messageFontSize; }
            set { SetProperty(ref _messageFontSize, value); }
        }

        public double MenuFontSize
        {
            get { return _menuFontSize; }
            set { SetProperty(ref _menuFontSize, value); }
        }

        public bool IsHighContrast
        {
            get { return _isHighContrast; }
            set { SetProperty(ref _isHighContrast, value); }
        }

        public SystemThemeType Theme
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }

        public Color AccentColor
        {
            get { return _accentColor; }
            set { SetProperty(ref _accentColor, value); }
        }


        private void WindowMessage_SettingChanged(object? sender, SettingChangedEventArgs e)
        {
            switch (e.Message)
            {
                case "WindowsThemeElement":
                    AppDispatcher.BeginInvoke(() => { UpdateFonts(); UpdateColors(); });
                    break;

                case "ImmersiveColorSet":
                    AppDispatcher.BeginInvoke(() => UpdateColors());
                    break;
            }
        }

        private void UpdateFonts()
        {
            MessageFontName = SystemFonts.MessageFontFamily.Source;
            MessageFontSize = SystemFonts.MessageFontSize;
            MenuFontSize = SystemFonts.MenuFontSize;
        }

        private void UpdateColors()
        {
            Theme = GetSystemAppTheme();
            AccentColor = GetAccentColor();
            IsHighContrast = SystemParameters.HighContrast;
        }

        private static SystemThemeType GetSystemAppTheme()
        {
            try
            {
                var registryKeyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                using (var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryKeyName))
                {
                    if (registryKey is null) return SystemThemeType.Dark;

                    var value = (int?)registryKey.GetValue("AppsUseLightTheme");
                    return (value == 1) ? SystemThemeType.Light : SystemThemeType.Dark;
                }
            }
            catch
            {
                return SystemThemeType.Dark;
            }
        }

        private static Color GetAccentColor()
        {
            try
            {
                NativeMethods.DwmGetColorizationColor(out uint colorizationColor, out bool colorizationOpaqueBlend);
                return Color.FromRgb((byte)(colorizationColor >> 16), (byte)(colorizationColor >> 8), (byte)colorizationColor);
            }
            catch
            {
                return Color.FromRgb(0x00, 0x78, 0xD7);
            }
        }
    }
}
