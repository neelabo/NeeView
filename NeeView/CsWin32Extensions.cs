
global using Win32Rect = Windows.Win32.Foundation.RECT;

using System;
using System.Runtime.InteropServices;


namespace NeeView.Win32.Extensions
{
    internal static partial class WindowMessages
    {
        internal const uint WM_SHNOTIFY = global::Windows.Win32.PInvoke.WM_USER + 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SHNOTIFYSTRUCT
    {
        public IntPtr dwItem1;
        public IntPtr dwItem2;
    }

    internal static class Win32RectExtensions
    {
        public static string ToDispString(this Win32Rect rect)
        {
            return $"{{X={rect.X}, Y={rect.Y}, Width={rect.Width}, Height={rect.Height}}}";
        }
    }
}

