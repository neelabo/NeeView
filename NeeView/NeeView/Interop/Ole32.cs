using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
    }

}
