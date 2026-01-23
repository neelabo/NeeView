using Microsoft.Win32.SafeHandles;
using NeeView.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace NeeView
{
    /// <summary>
    /// FileID <=> パス
    /// </summary>
    public static class FileIdResolver
    {
        /// <summary>
        /// パス -> FileId
        /// </summary>
        public static FileId GetFileIdFromPath(string path)
        {
            using var handle = NativeMethods.CreateFile(
                path,
                NativeMethods.FILE_READ_ATTRIBUTES,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

            if (handle.IsInvalid)
                ThrowLastError("CreateFile failed");

            if (!NativeMethods.GetFileInformationByHandleEx(
                handle,
                NativeMethods.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                out NativeMethods.FILE_ID_INFO info,
                (uint)Marshal.SizeOf<NativeMethods.FILE_ID_INFO>()))
            {
                ThrowLastError("GetFileInformationByHandleEx failed");
            }

            var volumeGuid = VolumeSerialToPathResolver.GetVolumePathFromSerial(info.VolumeSerialNumber);
            if (volumeGuid == null)
            {
                throw new IOException("Volume GUID not found for serial: " + info.VolumeSerialNumber);
            }

            return new(volumeGuid, info.FileId);
        }

        /// <summary>
        /// FileId -> パス
        /// </summary>
        public static string? ResolvePathFromFileId(FileId fileId)
        {
            try
            {
                // FileID → ハンドルを開く
                using var handle = OpenByFileId(fileId.VolumePath, fileId.FileId128);

                //  ハンドル → パス復元
                var resolvedPath = GetPathFromHandle(handle);

                return resolvedPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ResolvePathFromFileId failed: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// FileID -> ハンドル
        /// </summary>
        private static SafeFileHandle OpenByFileId(string volumePath, byte[] fileId128)
        {
            Debug.Assert(!volumePath.StartsWith(@"'\\"));
            Debug.Assert(!volumePath.EndsWith(@"\"));

            var volumeHandle = NativeMethods.CreateFile(
                volumePath,
                NativeMethods.FILE_READ_ATTRIBUTES,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (volumeHandle.IsInvalid)
                ThrowLastError("Failed to open volume: " + volumePath);

            var desc = new NativeMethods.FILE_ID_DESCRIPTOR
            {
                dwSize = (uint)Marshal.SizeOf<NativeMethods.FILE_ID_DESCRIPTOR>(),
                Type = NativeMethods.FILE_ID_TYPE.ExtendedFileIdType,
                Id = new NativeMethods.FILE_ID_DESCRIPTOR_UNION { FileId128 = new NativeMethods.FILE_ID_128(fileId128) }
            };

            var handle = NativeMethods.OpenFileById(
                volumeHandle,
                ref desc,
                NativeMethods.FILE_READ_ATTRIBUTES,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
                IntPtr.Zero,
                NativeMethods.FILE_FLAG_BACKUP_SEMANTICS);

            if (handle.IsInvalid)
                ThrowLastError("OpenFileById failed");

            return handle;
        }

        /// <summary>
        /// ハンドル -> パス
        /// </summary>
        private static string? GetPathFromHandle(SafeFileHandle handle)
        {
            var buffer = new char[1024];
            var result = NativeMethods.GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Length, NativeMethods.FILE_NAME_NORMALIZED);

            if (result == 0 || result >= buffer.Length)
                return null;

            var path = new string(buffer, 0, (int)result);

            if (path.StartsWith(@"\\?\"))
                path = path.Substring(4);

            return path;
        }

        /// <summary>
        /// WIN32エラーを取得してIOExceptionをスロー
        /// </summary>
        private static void ThrowLastError(string message)
        {
            var err = Marshal.GetLastWin32Error();
            throw new IOException($"{message} (CODE: {err})");
        }
    }
}