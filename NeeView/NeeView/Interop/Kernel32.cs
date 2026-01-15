using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

        [DllImport("kernel32")]
        internal static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr GlobalSize(IntPtr hMem);

        [DllImport("kernel32")]
        internal static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr Arguments);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFile(string lpExistingFileName, string lpNewFileName);
    }
}
