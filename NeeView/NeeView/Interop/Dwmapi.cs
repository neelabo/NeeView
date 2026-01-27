using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmGetColorizationColor(out uint colorizationColor, [MarshalAs(UnmanagedType.Bool)] out bool colorizationOpaqueBlend);
    }
}
