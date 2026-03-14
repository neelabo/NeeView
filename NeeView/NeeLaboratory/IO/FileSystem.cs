using NeeView;
using System;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Shell;

using SHFILEINFO = Windows.Win32.UI.Shell.SHFILEINFOW;

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

            var result = PInvoke.SHGetFileInfo(path, 0, ref shfi, SHGFI_FLAGS.SHGFI_TYPENAME);
            if (result == 0)
            {
                return null;
            }

            var typeName = shfi.szTypeName.ToString();
            if (!string.IsNullOrEmpty(typeName))
            {
                return typeName;
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
        internal static string? GetTypeNameWithAttribute(string path, FILE_FLAGS_AND_ATTRIBUTES attribute)
        {
            var shfi = new SHFILEINFO();

            var result = PInvoke.SHGetFileInfo(path, attribute, ref shfi, (SHGFI_FLAGS.SHGFI_TYPENAME | SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES));
            if (result == 0)
            {
                return null;
            }

            var typeName = shfi.szTypeName.ToString();
            if (!string.IsNullOrEmpty(typeName))
            {
                return typeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイル拡張子から種類名を取得(Win32版)
        /// </summary>
        public static string? GetExtensionTypeName(string extension)
        {
            var ext = string.IsNullOrEmpty(extension) ? "dummy" : extension;
            return GetTypeNameWithAttribute(ext, 0);
        }

        /// <summary>
        /// ディレクトリの種類名を取得(Win32)
        /// </summary>
        /// <returns></returns>
        public static string? GetDirectoryTypeName()
        {
            return GetTypeNameWithAttribute("dummy", FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY);
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
            var result= PInvoke.SHGetFileInfo(path, 0, ref shinfo, (SHGFI_FLAGS.SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_FLAGS.SHGFI_SMALLICON : SHGFI_FLAGS.SHGFI_LARGEICON)));
            if (result != 0)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                PInvoke.DestroyIcon(shinfo.hIcon);
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
        internal static BitmapSource? GetTypeIconSourceWithAttribute(string path, IconSize iconSize, FILE_FLAGS_AND_ATTRIBUTES attribute)
        {
            var shinfo = new SHFILEINFO();
            var result = PInvoke.SHGetFileInfo(path, attribute, ref shinfo, (SHGFI_FLAGS.SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_FLAGS.SHGFI_SMALLICON : SHGFI_FLAGS.SHGFI_LARGEICON) | SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES));
            if (result != 0)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                PInvoke.DestroyIcon(shinfo.hIcon);
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

            if (!PInvoke.SHObjectProperties((HWND)handle, SHOP_TYPE.SHOP_FILEPATH, path, ""))
            {
                throw new ApplicationException($"Cannot open file property window. {path}");
            }
        }
    }
}
