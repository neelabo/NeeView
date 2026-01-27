using System.Runtime.InteropServices;


namespace NeeView.Interop
{
    // NOTE: fgd[] が不定サイズなので Marshal.PtrToStructure() では直接取得できない
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct FILEGROUPDESCRIPTORW
    {
        public int cItems;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] 
        public FILEDESCRIPTORW[] fgd;
    }
}
