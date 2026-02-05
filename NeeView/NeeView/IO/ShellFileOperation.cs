using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace NeeView.IO
{
    public static class ShellFileOperation
    {
        private static void SHFileOperation(ref SHFILEOPSTRUCT lpFileOp)
        {
            var result = NativeMethods.SHFileOperation(ref lpFileOp);

            Debug.WriteLine($"SHFileOperation: Code=0x{result:x4}");

            switch ((FileOperationErrors)result)
            {
                case 0:
                    return;
                case FileOperationErrors.DE_OPCANCELLED:
                case FileOperationErrors.ERROR_CANCELLED:
                    throw new OperationCanceledException();

                case FileOperationErrors.DE_SAMEFILE:
                case FileOperationErrors.DE_MANYSRC1DEST:
                case FileOperationErrors.DE_DIFFDIR:
                case FileOperationErrors.DE_ROOTDIR:
                case FileOperationErrors.DE_DESTSUBTREE:
                case FileOperationErrors.DE_ACCESSDENIEDSRC:
                case FileOperationErrors.DE_PATHTOODEEP:
                case FileOperationErrors.DE_MANYDEST:
                case FileOperationErrors.DE_INVALIDFILES:
                case FileOperationErrors.DE_DESTSAMETREE:
                case FileOperationErrors.DE_FLDDESTISFILE:
                case FileOperationErrors.DE_FILEDESTISFLD:
                case FileOperationErrors.DE_FILENAMETOOLONG:
                case FileOperationErrors.DE_DEST_IS_CDROM:
                case FileOperationErrors.DE_DEST_IS_DVD:
                case FileOperationErrors.DE_DEST_IS_CDRECORD:
                case FileOperationErrors.DE_FILE_TOO_LARGE:
                case FileOperationErrors.DE_SRC_IS_CDROM:
                case FileOperationErrors.DE_SRC_IS_DVD:
                case FileOperationErrors.DE_SRC_IS_CDRECORD:
                case FileOperationErrors.DE_ERROR_MAX:
                case FileOperationErrors.DE_ERROR_UNKNOWN:
                case FileOperationErrors.ERRORONDEST:
                case FileOperationErrors.DE_DESTROOTDIR:
                    throw new IOException($"{(FileOperationErrors)result} (0x{result:x4})");

                default:
                    var message = new StringBuilder(1024);
                    var length = NativeMethods.FormatMessage((uint)FormatMessages.FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, (uint)result, 0, message, message.Capacity, IntPtr.Zero);
                    if (length > 0)
                    {
                        throw new IOException(message.ToString());
                    }
                    else
                    {
                        throw new IOException($"Error code: 0x{result:x4}");
                    }
            }
        }

        private static IntPtr GetHWnd(Window window)
        {
            return window != null
              ? new System.Windows.Interop.WindowInteropHelper(window).Handle
              : IntPtr.Zero;
        }


        public static void Copy(Window owner, IEnumerable<string> paths, string dst)
        {
            Copy(GetHWnd(owner), paths, dst);
        }

        public static void Copy(IntPtr hwnd, IEnumerable<string> paths, string dst)
        {
            if (paths == null || !paths.Any()) throw new ArgumentException("Empty paths");
            if (dst == null) throw new ArgumentNullException(nameof(dst));

            SHFILEOPSTRUCT shfos;
            shfos.hwnd = hwnd;
            shfos.wFunc = FileFuncFlags.FO_COPY;
            shfos.pFrom = string.Join("\0", paths) + "\0\0";
            shfos.pTo = dst + "\0\0";
            shfos.fFlags = FileOperationFlags.FOF_ALLOWUNDO;
            shfos.fAnyOperationsAborted = true;
            shfos.hNameMappings = IntPtr.Zero;
            shfos.lpszProgressTitle = null;

            SHFileOperation(ref shfos);
        }

        public static void Move(Window owner, IEnumerable<string> paths, string dst)
        {
            Move(GetHWnd(owner), paths, dst);
        }

        public static void Move(IntPtr hwnd, IEnumerable<string> paths, string dst)
        {
            if (paths == null || !paths.Any()) throw new ArgumentException("Empty paths");
            if (dst == null) throw new ArgumentNullException(nameof(dst));

            SHFILEOPSTRUCT shfos;
            shfos.hwnd = hwnd;
            shfos.wFunc = FileFuncFlags.FO_MOVE;
            shfos.pFrom = string.Join("\0", paths) + "\0\0";
            shfos.pTo = dst + "\0\0";
            shfos.fFlags = FileOperationFlags.FOF_ALLOWUNDO;
            shfos.fAnyOperationsAborted = true;
            shfos.hNameMappings = IntPtr.Zero;
            shfos.lpszProgressTitle = null;

            SHFileOperation(ref shfos);
        }

        public static void Delete(Window owner, IEnumerable<string> paths, bool wantNukeWarning)
        {
            Delete(GetHWnd(owner), paths, wantNukeWarning);
        }

        public static void Delete(IntPtr hwnd, IEnumerable<string> paths, bool wantNukeWarning)
        {
            if (paths == null || !paths.Any()) throw new ArgumentException("Empty paths");

            var flags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION;
            if (wantNukeWarning)
            {
                flags |= FileOperationFlags.FOF_WANTNUKEWARNING;
            }

            SHFILEOPSTRUCT shfos;
            shfos.hwnd = hwnd;
            shfos.wFunc = FileFuncFlags.FO_DELETE;
            shfos.pFrom = string.Join("\0", paths) + "\0\0";
            shfos.pTo = null;
            shfos.fFlags = flags;
            shfos.fAnyOperationsAborted = true;
            shfos.hNameMappings = IntPtr.Zero;
            shfos.lpszProgressTitle = null;

            SHFileOperation(ref shfos);
        }
    }

}
