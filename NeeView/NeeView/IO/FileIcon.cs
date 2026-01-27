using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Imaging;

namespace NeeView.IO
{
    public enum FileIconType
    {
        File,
        Directory,
        Drive,
        FileType,
        DirectoryType,
    }


    // from https://www.ipentec.com/document/csharp-shell-namespace-get-big-icon-from-file-path
    public class FileIcon
    {
        public enum IconSize
        {
            Large = SHImageLists.SHIL_LARGE,
            Small = SHImageLists.SHIL_SMALL,
            ExtraLarge = SHImageLists.SHIL_EXTRALARGE,
            Jumbo = SHImageLists.SHIL_JUMBO,
        };


        private static readonly System.Threading.Lock _lock = new();


        public static List<BitmapSource> CreateIconCollection(string filename, FileIconType iconType, bool allowJumbo)
        {
            return iconType switch
            {
                FileIconType.DirectoryType => CreateDirectoryTypeIconCollection(filename, allowJumbo),
                FileIconType.FileType => CreateFileTypeIconCollection(filename, allowJumbo),
                FileIconType.Drive => CreateDriveIconCollection(filename, allowJumbo),
                FileIconType.Directory => CreateDirectoryIconCollection(filename, allowJumbo),
                FileIconType.File => CreateFileIconCollection(filename, allowJumbo),
                _ => throw new ArgumentOutOfRangeException(nameof(iconType)),
            };
        }

        public static List<BitmapSource> CreateDirectoryTypeIconCollection(string filename, bool allowJumbo)
        {
            return CreateFileIconCollection(filename, Interop.FileAttributes.FILE_ATTRIBUTE_DIRECTORY, SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES, allowJumbo);
        }

        public static List<BitmapSource> CreateFileTypeIconCollection(string filename, bool allowJumbo)
        {
            return CreateFileIconCollection(System.IO.Path.GetExtension(filename), 0, SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES, allowJumbo);
        }

        public static List<BitmapSource> CreateDriveIconCollection(string filename, bool allowJumbo)
        {
            var flags = SHGetFileInfoFlags.SHGFI_ICONLOCATION;
            return CreateFileIconCollection(filename, Interop.FileAttributes.FILE_ATTRIBUTE_DIRECTORY, flags, allowJumbo);
        }

        public static List<BitmapSource> CreateDirectoryIconCollection(string filename, bool allowJumbo)
        {
            var flags = System.IO.Directory.Exists(filename) ? 0 : SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES;
            return CreateFileIconCollection(filename, Interop.FileAttributes.FILE_ATTRIBUTE_DIRECTORY, flags, allowJumbo);
        }

        public static List<BitmapSource> CreateFileIconCollection(string filename, bool allowJumbo)
        {
            return CreateFileIconCollection(filename, 0, 0, allowJumbo);
        }

        private static List<BitmapSource> CreateFileIconCollection(string filename, Interop.FileAttributes attribute, SHGetFileInfoFlags flags, bool allowJumbo)
        {
            if (allowJumbo)
            {
                return CreateFileIconCollectionExtra(filename, attribute, flags);
            }
            else
            {
                return CreateFileIconCollection(filename, attribute, flags);
            }
        }

        private static List<BitmapSource> CreateFileIconCollection(string filename, Interop.FileAttributes attribute, SHGetFileInfoFlags flags)
        {
            var bitmaps = new List<BitmapSource>
            {
                CreateFileIcon(filename, attribute, flags, IconSize.Small),
                CreateFileIcon(filename, attribute, flags, IconSize.Large)
            };
            return bitmaps.Where(e => e != null).ToList();
        }

        private static List<BitmapSource> CreateFileIconCollectionFromIconFile(string filename)
        {
            using (var imageFileStrm = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var decoder = BitmapDecoder.Create(imageFileStrm, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var bitmaps = decoder.Frames.Cast<BitmapSource>().ToList();
                bitmaps.ForEach(e => e.Freeze());
                return bitmaps;
            }
        }

        private static List<BitmapSource> CreateFileIconCollectionExtra(string filename, Interop.FileAttributes attribute, SHGetFileInfoFlags flags)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                var shinfo = new SHFILEINFO();
                shinfo.szDisplayName = "";
                shinfo.szTypeName = "";
                IntPtr hImg = NativeMethods.SHGetFileInfo(filename, (uint)attribute, ref shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), (uint)(SHGetFileInfoFlags.SHGFI_SYSICONINDEX | flags));

                try
                {
                    if ((flags & SHGetFileInfoFlags.SHGFI_ICONLOCATION) != 0 && Path.GetExtension(shinfo.szDisplayName).ToLowerInvariant() == ".ico")
                    {
                        return CreateFileIconCollectionFromIconFile(shinfo.szDisplayName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                var bitmaps = new List<BitmapSource>();

                var shils = Enum.GetValues(typeof(SHImageLists)).Cast<SHImageLists>();
                foreach (var shil in shils)
                {
                    try
                    {
                        int hResult = NativeMethods.SHGetImageList(shil, ref ImageListIDs.IID_IImageList, out IImageList imglist);
                        if (hResult == HResult.S_OK)
                        {
                            IntPtr hicon = IntPtr.Zero;
                            imglist.GetIcon(shinfo.iIcon, (int)ImageListDrawItemConstants.ILD_TRANSPARENT, ref hicon);
                            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hicon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            bitmapSource?.Freeze();
                            NativeMethods.DestroyIcon(hicon);
                            ////Debug.WriteLine($"Icon: {filename} - {shil}: {sw.ElapsedMilliseconds}ms");
                            if (bitmapSource is not null)
                            {
                                bitmaps.Add(bitmapSource);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Icon: {filename} - {shil}: {ex.Message}");
                        throw;
                    }
                }
                return bitmaps;
            }
        }

        private static BitmapSource CreateFileIcon(string filename, Interop.FileAttributes attribute, SHGetFileInfoFlags flags, IconSize iconSize)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                var shinfo = new SHFILEINFO();
                IntPtr hSuccess = NativeMethods.SHGetFileInfo(filename, (uint)attribute, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(flags | SHGetFileInfoFlags.SHGFI_ICON | (iconSize == IconSize.Small ? SHGetFileInfoFlags.SHGFI_SMALLICON : SHGetFileInfoFlags.SHGFI_LARGEICON)));
                if (hSuccess != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
                {
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    NativeMethods.DestroyIcon(shinfo.hIcon);
                    ////Debug.WriteLine($"Icon: {filename} - {iconSize}: {sw.ElapsedMilliseconds}ms");
                    return bitmapSource;
                }
                else
                {
                    Debug.WriteLine($"Icon: {filename} - {iconSize}: Cannot created!!");
                    throw new ApplicationException("Cannot create file icon.");
                }
            }
        }
    }
}
