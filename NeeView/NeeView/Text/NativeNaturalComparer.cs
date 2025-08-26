using NeeView.Interop;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NeeView.Text
{
    /// <summary>
    /// Win32API での自然ソート
    /// </summary>
    public class NativeNaturalComparer : IComparer<string>, IComparer
    {
        public int Compare(string? x, string? y)
        {
            if (x is null) return (y is null) ? 0 : -1;
            if (y is null) return 1;

            return NativeMethods.StrCmpLogicalW(x, y);
        }

        public int Compare(object? x, object? y)
        {
            return Compare(x as string, y as string);
        }
    }

}
