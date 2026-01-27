using System.Runtime.InteropServices;
using System.Text;

namespace NeeView.Native
{
    /// <summary>
    /// for NeeView.Interop.dll
    /// </summary>
    public class NVInterop
    {
        private static class NativeMethods
        {
#if X64
            private const string DllName = @"Libraries\x64\NeeView.Interop.dll";
#else
            private const string DllName = @"Libraries\x86\NeeView.Interop.dll";
#endif

            [DllImport(DllName, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal static extern bool NVGetImageCodecInfo(uint index, StringBuilder friendlyName, StringBuilder fileExtensions);

            [DllImport(DllName)]
            internal static extern void NVCloseImageCodecInfo();

            [DllImport(DllName)]
            internal static extern void NVFpReset();

            [DllImport(DllName, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal static extern bool NVGetFullPathFromShortcut([MarshalAs(UnmanagedType.LPWStr)] string shortcut, StringBuilder fullPath);

            // NOTE: FPU設定のテスト用
#if false 
            [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
            public extern static uint _controlfp(uint newcw, uint mask);

            public const uint _MCW_EM = 0x0008001f;
            public const uint EM_INVALID = 0x00000010;
            public const uint EM_DENORMAL = 0x00080000;
            public const uint EM_ZERODIVIDE = 0x00000008;
            public const uint EM_OVERFLOW = 0x00000004;
            public const uint EM_UNDERFLOW = 0x00000002;
            public const uint EM_INEXACT = 0x00000001;

            // FPUのリセット
            public static void FixFPU()
            {
                //_controlfp(_MCW_EM, EM_INVALID | EM_ZERODIVIDE);
                _controlfp(EM_INEXACT | EM_ZERODIVIDE | EM_OVERFLOW | EM_OVERFLOW | EM_DENORMAL | EM_INVALID, _MCW_EM);
            }
#endif
        }

        public static bool NVGetImageCodecInfo(uint index, StringBuilder friendlyName, StringBuilder fileExtensions)
        {
            return NativeMethods.NVGetImageCodecInfo(index, friendlyName, fileExtensions);
        }

        public static void NVCloseImageCodecInfo()
        {
            NativeMethods.NVCloseImageCodecInfo();
        }

        public static void NVFpReset()
        {
            NativeMethods.NVFpReset();
        }

        public static bool NVGetFullPathFromShortcut(string shortcut, StringBuilder fullPath)
        {
            return NativeMethods.NVGetFullPathFromShortcut(shortcut, fullPath);
        }

    }
}
