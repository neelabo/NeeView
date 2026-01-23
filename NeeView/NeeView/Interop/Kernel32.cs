using Microsoft.Win32.SafeHandles;
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

        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool GetFileInformationByHandleEx(SafeFileHandle hFile, FILE_INFO_BY_HANDLE_CLASS FileInformationClass, out FILE_ID_INFO lpFileInformation, uint dwBufferSize);

        internal enum FILE_INFO_BY_HANDLE_CLASS
        {
            FileIdInfo = 18
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        internal const uint FILE_READ_ATTRIBUTES = 0x80;
        internal const uint FILE_SHARE_READ = 1;
        internal const uint FILE_SHARE_WRITE = 2;
        internal const uint FILE_SHARE_DELETE = 4;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        internal const uint FILE_NAME_NORMALIZED = 0x0;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle OpenFileById(SafeFileHandle hVolume, ref FILE_ID_DESCRIPTOR fileId, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwFlags);

        internal enum FILE_ID_TYPE : uint
        {
            FileIdType = 0,
            ExtendedFileIdType = 2
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetFinalPathNameByHandle(SafeFileHandle hFile, char[] lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstVolume([Out] StringBuilder lpszVolumeName, int cchBufferLength);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern bool FindNextVolume(IntPtr hFindVolume, [Out] StringBuilder lpszVolumeName, int cchBufferLength);

        [DllImport("kernel32")]
        internal static extern bool FindVolumeClose(IntPtr hFindVolume);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern bool GetVolumePathNamesForVolumeNameW(string lpszVolumeName, [Out] StringBuilder lpszVolumePathNames, int cchBufferLength, out int lpcchReturnLength);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern bool GetVolumeInformation(string lpRootPathName, StringBuilder? lpVolumeNameBuffer, int nVolumeNameSize, out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength, out uint lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, int nFileSystemNameSize);

    }
}
