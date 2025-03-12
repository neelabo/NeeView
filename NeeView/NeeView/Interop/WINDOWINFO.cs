using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public int cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public int dwStyle;
        public int dwExStyle;
        public int dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public short atomWindowType;
        public short wCreatorVersion;
    }
}
