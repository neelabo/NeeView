using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct FILE_ID_DESCRIPTOR_UNION
        {
            [FieldOffset(0)]
            public long FileId64;

            [FieldOffset(0)]
            public FILE_ID_128 FileId128;
        }

    }
}
