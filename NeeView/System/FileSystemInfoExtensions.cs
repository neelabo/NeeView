using System;
using System.IO;

namespace NeeView
{
    internal static class FileSystemInfoExtensions
    {
        internal static DateTime GetSafeLastAccessTime(this FileSystemInfo info)
        {
            try
            {
                return info.LastAccessTime;
            }
            catch
            {
                return default;
            }
        }

        internal static DateTime GetSafeCreationTime(this FileSystemInfo info)
        {
            try
            {
                return info.CreationTime;
            }
            catch
            {
                return default;
            }
        }

        internal static DateTime GetSafeLastWriteTime(this FileSystemInfo info)
        {
            try
            {
                // [DEV]
                // Raise an exception => Not a valid Win32 FileTime. (Parameter 'fileTime')
                // _ = DateTimeOffset.FromFileTime(DateTimeOffset.MaxValue.Ticks + 1);

                return info.LastWriteTime;
            }
            catch
            {
                return default;
            }
        }
    }
}
