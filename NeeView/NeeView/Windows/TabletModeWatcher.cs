using Microsoft.Win32;
using NeeView.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace NeeView.Windows
{
    /// <summary>
    /// タブレットモード判定 (Windows10)
    /// </summary>
    public class TabletModeWatcher
    {
        private bool _isTabletMode = false;
        private int _dirtyValue = 1;


        public TabletModeWatcher(Window window)
        {
            if (!AddHook(window))
            {
                window.SourceInitialized += (s, e) => AddHook(window);
            }
        }


        public bool IsTabletMode
        {
            get
            {
                if (_dirtyValue != 0)
                {
                    UpdateTabletMode();
                }

                return _isTabletMode;
            }
        }

        private void UpdateTabletMode()
        {
            Interlocked.Exchange(ref _dirtyValue, 0);

            RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell");
            if (regKey != null)
            {
                _isTabletMode = Convert.ToBoolean(regKey.GetValue("TabletMode", 0), CultureInfo.InvariantCulture);
                regKey.Close();
            }

            Debug.WriteLine($"TabletMode: {_isTabletMode}");
        }

        private bool AddHook(Window window)
        {
            var hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hWnd).AddHook(new HwndSourceHook(WndProc));
                return true;
            }
            else
            {
                return false;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WindowMessages)msg)
            {
                case WindowMessages.WM_SETTINGCHANGE:
                    OnSettingChange(wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private void OnSettingChange(IntPtr wParam, IntPtr lParam)
        {
            string? str = Marshal.PtrToStringAuto(lParam);
            if (str == "UserInteractionMode")
            {
                Interlocked.Exchange(ref _dirtyValue, 1);
            }
        }
    }
}
