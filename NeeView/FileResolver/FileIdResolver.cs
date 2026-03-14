using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;

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
            using var handle = PInvoke.CreateFile(
                path,
                (uint)FILE_ACCESS_RIGHTS.FILE_READ_ATTRIBUTES,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_DELETE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS,
                null);

            if (handle.IsInvalid)
                ThrowLastError("CreateFile failed");

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<FILE_ID_INFO>()];

            if (!PInvoke.GetFileInformationByHandleEx(
                handle,
                FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                buffer))
            {
                ThrowLastError("GetFileInformationByHandleEx failed");
            }

            if (buffer.Length < Unsafe.SizeOf<FILE_ID_INFO>())
            {
                throw new InvalidOperationException("Buffer too small for FILE_ID_INFO");
            }

            var info = MemoryMarshal.Cast<byte, FILE_ID_INFO>(buffer)[0];

            var volumeGuid = VolumeSerialToPathResolver.GetVolumePathFromSerial(info.VolumeSerialNumber);
            if (volumeGuid == null)
            {
                throw new IOException("Volume GUID not found for serial: " + info.VolumeSerialNumber);
            }

            byte[] bytes = info.FileId.Identifier.AsReadOnlySpan().ToArray();
            return new(volumeGuid, bytes);
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

            using var volumeHandle = PInvoke.CreateFile(
                volumePath,
                (uint)FILE_ACCESS_RIGHTS.FILE_READ_ATTRIBUTES,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_DELETE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0,
                null);

            if (volumeHandle.IsInvalid)
                ThrowLastError("Failed to open volume: " + volumePath);

            var desc = new FILE_ID_DESCRIPTOR
            {
                dwSize = (uint)Marshal.SizeOf<FILE_ID_DESCRIPTOR>(),
                Type = FILE_ID_TYPE.ExtendedFileIdType,
            };
            desc.Anonymous.ExtendedFileId = new() { Identifier = fileId128 };

            var handle = PInvoke.OpenFileById(
                volumeHandle,
                desc,
                (uint)FILE_ACCESS_RIGHTS.FILE_READ_ATTRIBUTES,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_DELETE,
                null,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS);

            if (handle.IsInvalid)
                ThrowLastError("OpenFileById failed");

            return handle;
        }

        /// <summary>
        /// ハンドル -> パス
        /// </summary>
        private static string? GetPathFromHandle(SafeFileHandle handle)
        {
            var bufferSource = ArrayPool<char>.Shared.Rent(1024);
            try
            {
                Span<char> buffer = bufferSource;
                var length = PInvoke.GetFinalPathNameByHandle(handle, buffer, GETFINALPATHNAMEBYHANDLE_FLAGS.FILE_NAME_NORMALIZED);

                if (length == 0 || length >= buffer.Length)
                    return null;

                var path = buffer[..(int)length].ToString();

                if (path.StartsWith(@"\\?\"))
                    path = path.Substring(4);

                return path;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(bufferSource);
            }
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