using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;


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
        public enum IconSize : uint
        {
            Large = PInvoke.SHIL_LARGE,
            Small = PInvoke.SHIL_SMALL,
            ExtraLarge = PInvoke.SHIL_EXTRALARGE,
            Jumbo = PInvoke.SHIL_JUMBO,
        };

        //IMAGE LIST
        public static readonly Guid IID_IImageList = new("46EB5926-582E-4017-9FDF-E8998DAA0950");
        public static readonly Guid IID_IImageList2 = new("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");

        private static readonly Lock _lock = new();


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
            return CreateFileIconCollection(filename, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY, SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES, allowJumbo);
        }

        public static List<BitmapSource> CreateFileTypeIconCollection(string filename, bool allowJumbo)
        {
            return CreateFileIconCollection(System.IO.Path.GetExtension(filename), 0, SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES, allowJumbo);
        }

        public static List<BitmapSource> CreateDriveIconCollection(string filename, bool allowJumbo)
        {
            var flags = SHGFI_FLAGS.SHGFI_ICONLOCATION;
            return CreateFileIconCollection(filename, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY, flags, allowJumbo);
        }

        public static List<BitmapSource> CreateDirectoryIconCollection(string filename, bool allowJumbo)
        {
            var flags = FileIO.DirectoryExists(filename) ? 0 : SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES;
            return CreateFileIconCollection(filename, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY, flags, allowJumbo);
        }

        public static List<BitmapSource> CreateFileIconCollection(string filename, bool allowJumbo)
        {
            return CreateFileIconCollection(filename, 0, 0, allowJumbo);
        }

        private static List<BitmapSource> CreateFileIconCollection(string filename, FILE_FLAGS_AND_ATTRIBUTES attribute, SHGFI_FLAGS flags, bool allowJumbo)
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

        private static List<BitmapSource> CreateFileIconCollection(string filename, FILE_FLAGS_AND_ATTRIBUTES attribute, SHGFI_FLAGS flags)
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

        private static unsafe List<BitmapSource> CreateFileIconCollectionExtra(string filename, FILE_FLAGS_AND_ATTRIBUTES attribute, SHGFI_FLAGS flags)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                var shinfo = new SHFILEINFOW();
                var result = PInvoke.SHGetFileInfo(filename, attribute, ref shinfo, (SHGFI_FLAGS.SHGFI_SYSICONINDEX | flags));
                try
                {
                    var displayName = shinfo.szDisplayName.ToString();
                    if ((flags & SHGFI_FLAGS.SHGFI_ICONLOCATION) != 0 && Path.GetExtension(displayName).ToLowerInvariant() == ".ico")
                    {
                        return CreateFileIconCollectionFromIconFile(displayName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                var bitmaps = new List<BitmapSource>();

                var shils = Enum.GetValues(typeof(IconSize)).Cast<IconSize>();
                foreach (var shil in shils)
                {
                    try
                    {
                        HRESULT hResult = PInvoke.SHGetImageList((int)shil, IID_IImageList, out var obj);
                        if (hResult.Succeeded)
                        {
                            IImageList imglist = (IImageList)obj;
                            HICON hicon = default;
                            imglist.GetIcon(shinfo.iIcon, (int)IMAGE_LIST_DRAW_STYLE.ILD_TRANSPARENT, &hicon);
                            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hicon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            bitmapSource?.Freeze();
                            PInvoke.DestroyIcon(hicon);
                            if (bitmapSource is not null)
                            {
                                ////Debug.WriteLine($"Icon: {filename} - {shil}: {bitmapSource.PixelWidth}x{bitmapSource.PixelHeight}");
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

        private static BitmapSource CreateFileIcon(string filename, FILE_FLAGS_AND_ATTRIBUTES attribute, SHGFI_FLAGS flags, IconSize iconSize)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                var shinfo = new SHFILEINFOW();
                var result = PInvoke.SHGetFileInfo(filename, attribute, ref shinfo, (flags | SHGFI_FLAGS.SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_FLAGS.SHGFI_SMALLICON : SHGFI_FLAGS.SHGFI_LARGEICON)));
                if (result != 0 && shinfo.hIcon != IntPtr.Zero)
                {
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    PInvoke.DestroyIcon(shinfo.hIcon);
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
