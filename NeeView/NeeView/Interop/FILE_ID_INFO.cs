using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_ID_INFO
        {
            public ulong VolumeSerialNumber;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] FileId; // 128bit
        }

    }
}
