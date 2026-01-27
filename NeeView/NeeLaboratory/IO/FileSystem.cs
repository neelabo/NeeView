using NeeView;
using NeeView.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// ファイル情報静的メソッド
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// アイコンサイズ
        /// </summary>
        public enum IconSize
        {
            Small,
            Normal,
        };


        /// <summary>
        /// ファイルの種類名を取得(Win32版)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetTypeName(string path)
        {
            var shfi = new SHFILEINFO();
            shfi.szDisplayName = "";
            shfi.szTypeName = "";

            IntPtr hSuccess = NativeMethods.SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), (uint)SHGetFileInfoFlags.SHGFI_TYPENAME);
            if (hSuccess != IntPtr.Zero)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(shfi.szTypeName))
            {
                return shfi.szTypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイルの種類名を取得(Win32版)(USEFILEATTRIBUTES)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetTypeNameWithAttribute(string path, uint attribute)
        {
            var shfi = new SHFILEINFO();
            shfi.szDisplayName = "";
            shfi.szTypeName = "";

            IntPtr hSuccess = NativeMethods.SHGetFileInfo(path, attribute, ref shfi, (uint)Marshal.SizeOf(shfi), (uint)(SHGetFileInfoFlags.SHGFI_TYPENAME | SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES));
            if (hSuccess != IntPtr.Zero)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(shfi.szTypeName))
            {
                return shfi.szTypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// アプリケーション・アイコンを取得(Win32版)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        public static BitmapSource? GetTypeIconSource(string path, IconSize iconSize)
        {
            var shinfo = new SHFILEINFO();
            IntPtr hSuccess = NativeMethods.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGetFileInfoFlags.SHGFI_ICON | (iconSize == IconSize.Small ? SHGetFileInfoFlags.SHGFI_SMALLICON : SHGetFileInfoFlags.SHGFI_LARGEICON)));
            if (hSuccess != IntPtr.Zero)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                NativeMethods.DestroyIcon(shinfo.hIcon);
                return bitmapSource;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        // アプリケーション・アイコンを取得(Win32版)(USEFILEATTRIBUTES)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        public static BitmapSource? GetTypeIconSourceWithAttribute(string path, IconSize iconSize, uint attribute)
        {
            var shinfo = new SHFILEINFO();
            IntPtr hSuccess = NativeMethods.SHGetFileInfo(path, attribute, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGetFileInfoFlags.SHGFI_ICON | (iconSize == IconSize.Small ? SHGetFileInfoFlags.SHGFI_SMALLICON : SHGetFileInfoFlags.SHGFI_LARGEICON) | SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES));
            if (hSuccess != IntPtr.Zero)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                NativeMethods.DestroyIcon(shinfo.hIcon);
                return bitmapSource;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイルサイズ取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetSize(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            if ((fileInfo.Attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
            {
                return -1;
            }
            else
            {
                return fileInfo.Length;
            }
        }

        /// <summary>
        /// ファイル更新日取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetLastWriteTime(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            return fileInfo.GetSafeLastWriteTime();
        }

        /// <summary>
        /// プロパティウィンドウを開く
        /// </summary>
        /// <param name="path"></param>
        public static void OpenProperty(System.Windows.Window window, string path)
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            if (!NativeMethods.SHObjectProperties(handle, (uint)ShopObjectTypes.SHOP_FILEPATH, path, ""))
            {
                throw new ApplicationException($"Cannot open file property window. {path}");
            }
        }
    }
}
