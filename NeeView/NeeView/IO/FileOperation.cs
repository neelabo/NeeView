using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    /// <summary>
    /// IFileOperation wrapper
    /// </summary>
    public static class FileOperation
    {
        /// <summary>
        /// Moves the specified files or folders to the destination folder.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sourcePaths"></param>
        /// <param name="destFolderPath"></param>
        /// <returns>Execution results</returns>
        public static FolderOperatonResult MoveToFolder(IntPtr owner, IEnumerable<string> sourcePaths, string destFolderPath)
        {
            var fileOp = CreateFileOperation(owner);

            var sink = new MoveItemsSink();
            fileOp.Advise(sink, out uint cookie);

            try
            {
                var destFolder = CreateShellItem(destFolderPath);

                foreach (var item in sourcePaths.Select(e => CreateShellItem(e)))
                {
                    fileOp.MoveItem(item, destFolder, null, null);
                }

                fileOp.PerformOperations();

                return new FolderOperatonResult(sink.Results);
            }
            finally
            {
                fileOp.Unadvise(cookie);
            }
        }

        /// <summary>
        /// Moves the specified file or folder to the secified path.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static FolderOperatonResult Move(IntPtr owner, string sourcePath, string destPath)
        {
            var destSplitPath = SplitPath(destPath);

            var fileOp = CreateFileOperation(owner);

            var sink = new MoveItemsSink();
            fileOp.Advise(sink, out uint cookie);

            try
            {
                var destFolder = CreateShellItem(destSplitPath.Directory);
                var item = CreateShellItem(sourcePath);

                fileOp.MoveItem(item, destFolder, destSplitPath.FileName, null);

                fileOp.PerformOperations();

                return new FolderOperatonResult(sink.Results);
            }
            finally
            {
                fileOp.Unadvise(cookie);
            }
        }

        /// <summary>
        /// Copy the specified files or folders to the destination folder.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sourcePaths"></param>
        /// <param name="destFolderPath"></param>
        /// <returns>Execution results</returns>
        public static FolderOperatonResult CopyToFolder(IntPtr owner, IEnumerable<string> sourcePaths, string destFolderPath)
        {
            var fileOp = CreateFileOperation(owner);

            var sink = new CopyItemsSink();
            fileOp.Advise(sink, out uint cookie);

            try
            {
                var destFolder = CreateShellItem(destFolderPath);

                foreach (var item in sourcePaths.Select(e => CreateShellItem(e)))
                {
                    fileOp.CopyItem(item, destFolder, null, null);
                }

                fileOp.PerformOperations();

                return new FolderOperatonResult(sink.Results);
            }
            finally
            {
                fileOp.Unadvise(cookie);
            }
        }

        /// <summary>
        /// Copy the specified file or folder to the secified path.
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static FolderOperatonResult Copy(IntPtr owner, string sourcePath, string destPath)
        {
            var destSplitPath = SplitPath(destPath);

            var fileOp = CreateFileOperation(owner);

            var sink = new CopyItemsSink();
            fileOp.Advise(sink, out uint cookie);

            try
            {
                var destFolder = CreateShellItem(destSplitPath.Directory);
                var item = CreateShellItem(sourcePath);

                fileOp.CopyItem(item, destFolder, destSplitPath.FileName, null);

                fileOp.PerformOperations();

                return new FolderOperatonResult(sink.Results);
            }
            finally
            {
                fileOp.Unadvise(cookie);
            }
        }

        /// <summary>
        /// Delete the specified files.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sourcePaths"></param>
        /// <returns>Execution results</returns>
        public static FolderOperatonResult Delete(IntPtr owner, IEnumerable<string> sourcePaths, bool wantNukeWarning)
        {
            var fileOp = CreateFileOperation(owner);

            if (wantNukeWarning)
            {
                fileOp.SetOperationFlags(FILEOPERATION_FLAGS.FOF_ALLOWUNDO | FILEOPERATION_FLAGS.FOF_WANTNUKEWARNING);
            }

            var sink = new DeleteItemsSink();
            fileOp.Advise(sink, out uint cookie);

            try
            {
                foreach (var item in sourcePaths.Select(e => CreateShellItem(e)))
                {
                    fileOp.DeleteItem(item, null);
                }

                fileOp.PerformOperations();

                return new FolderOperatonResult(sink.Results);
            }
            finally
            {
                fileOp.Unadvise(cookie);
            }
        }

        /// <summary>
        /// Split the path into directory and filename
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private static (string Directory, string FileName) SplitPath(string path)
        {
            var directory = System.IO.Path.GetDirectoryName(path) ?? throw new IOException($"Illegal path: {path}");
            var filename = System.IO.Path.GetFileName(path) ?? throw new IOException($"Illegal path: {path}");
            return (directory, filename);
        }

        /// <summary>
        /// Create IFileOperation instance
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        private static IFileOperation CreateFileOperation(IntPtr hwnd)
        {
            PInvoke.CoCreateInstance(typeof(global::Windows.Win32.UI.Shell.FileOperation).GUID, null, CLSCTX.CLSCTX_ALL, out IFileOperation fileOp).ThrowOnFailure();

            if (hwnd != IntPtr.Zero)
            {
                fileOp.SetOwnerWindow(new global::Windows.Win32.Foundation.HWND(hwnd));
            }

            return fileOp;
        }

        /// <summary>
        /// Create an IShellItem from a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static IShellItem CreateShellItem(string path, bool isDirectory = false)
        {
            try
            {
                PInvoke.SHCreateItemFromParsingName(path, null, typeof(IShellItem).GUID, out var itemPtr).ThrowOnFailure();
                if (itemPtr is null)
                {
                    throw new COMException("Failed to create shell item");
                }
                return (IShellItem)itemPtr;
            }
            catch (Exception ex)
            {
                throw new COMException($"{ex.Message} '{path}'", ex);
            }
        }
    }
}

