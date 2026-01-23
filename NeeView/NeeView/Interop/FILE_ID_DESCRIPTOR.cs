using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_ID_DESCRIPTOR
        {
            public uint dwSize;
            public FILE_ID_TYPE Type;
            public FILE_ID_DESCRIPTOR_UNION Id;
        }

    }
}
