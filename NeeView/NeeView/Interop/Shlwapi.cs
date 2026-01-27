using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int StrCmpLogicalW(string? psz1, string? psz2);
    }
}
