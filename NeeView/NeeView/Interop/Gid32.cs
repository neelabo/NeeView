using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateSolidBrush(int color);
    }
}
