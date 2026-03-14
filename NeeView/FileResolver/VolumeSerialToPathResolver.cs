using NeeLaboratory.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;


namespace NeeView
{
    public static class VolumeSerialToPathResolver
    {
        private static Dictionary<ulong, string> _map = new();

        public static string? GetVolumePathFromSerial(ulong volumeSerial)
        {
            string? volumePath;
            if (_map.TryGetValue(volumeSerial, out volumePath))
            {
                return volumePath;
            }

            // 見つからなければ再構築して再検索
            // シリアルから生成する場合はこれでかならず見つかるはず
            _map = BuildVolumeSerialToPathTable();

            if (_map.TryGetValue(volumeSerial, out volumePath))
            {
                return volumePath;
            }

            return null;
        }


        private static Dictionary<ulong, string> BuildVolumeSerialToPathTable()
        {
            var table = new Dictionary<ulong, string>();

            var buffer = ArrayPool<char>.Shared.Rent(1024);
            try
            {
                Span<char> volumeName = buffer;

                using var findHandle = PInvoke.FindFirstVolume(volumeName);
                if (findHandle.IsInvalid)
                {
                    return table;
                }

                do
                {
                    // "\\?\Volume{GUID}\"
                    string guidPath = volumeName.ToNullTerminatedString();

                    if (TryGetVolumeSerial(guidPath, out ulong serial))
                    {
                        // VolumeSerial → VolumePath
                        table[serial] = guidPath.TrimEnd('\\');
                    }

                    var len = volumeName.Length;

                } while (PInvoke.FindNextVolume((HANDLE)findHandle.DangerousGetHandle(), volumeName));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            return table;
        }

        private static bool TryGetVolumeSerial(string volumeGuid, out ulong volumeSerial)
        {
            volumeSerial = 0;

            try
            {
                using var h = PInvoke.CreateFile(
                    volumeGuid,
                    0,
                    FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_DELETE,
                    null,
                    FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                    FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS,
                    null);

                if (h.IsInvalid)
                {
                    return false;
                }

                Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<FILE_ID_INFO>()];

                if (!PInvoke.GetFileInformationByHandleEx(
                    h,
                    FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                    buffer))
                {
                    return false;
                }

                if (buffer.Length < Unsafe.SizeOf<FILE_ID_INFO>())
                {
                    throw new InvalidOperationException("Buffer too small for FILE_ID_INFO");
                }

                var info = MemoryMarshal.Cast<byte, FILE_ID_INFO>(buffer)[0];

                volumeSerial = info.VolumeSerialNumber;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}