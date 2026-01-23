using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

            const int bufferSize = 1024;
            var volumeName = new StringBuilder(bufferSize);

            IntPtr findHandle = NativeMethods.FindFirstVolume(volumeName, bufferSize);
            if (findHandle == IntPtr.Zero)
                return table;

            try
            {
                do
                {
                    // "\\?\Volume{GUID}\"
                    string guidPath = volumeName.ToString();

                    if (TryGetVolumeSerial(guidPath, out ulong serial))
                    {
                        // VolumeSerial → VolumePath
                        table[serial] = guidPath.TrimEnd('\\');
                    }

                } while (NativeMethods.FindNextVolume(findHandle, volumeName, bufferSize));
            }
            finally
            {
                NativeMethods.FindVolumeClose(findHandle);
            }

            return table;
        }

        private static bool TryGetVolumeSerial(string volumeGuid, out ulong volumeSerial)
        {
            volumeSerial = 0;

            try
            {
                using var h = NativeMethods.CreateFile(
                    volumeGuid,
                    0,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    NativeMethods.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero);

                if (h.IsInvalid)
                    return false;

                if (!NativeMethods.GetFileInformationByHandleEx(
                    h,
                    NativeMethods.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                    out NativeMethods.FILE_ID_INFO info,
                    (uint)Marshal.SizeOf<NativeMethods.FILE_ID_INFO>()))
                {
                    return false;
                }

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