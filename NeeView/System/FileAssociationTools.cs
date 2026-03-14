using System;
using System.Buffers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView
{
    public static class FileAssociationTools
    {
        /// <summary>
        /// すべての関連づけを解除する
        /// </summary>
        public static void UnassociateAll()
        {
            var collection = FileAssociationCollectionFactory.Create(FileAssociationCollectionCreateOptions.OnlyRegistered);
            foreach (var item in collection)
            {
                item.IsEnabled = false;
            }
        }

        /// <summary>
        /// ファイル関連付け変更をシェルに通知してアイコン表示を更新する
        /// </summary>
        public static unsafe void RefreshShellIcons()
        {
            Debug.WriteLine($"FileAssociate: Refresh shell icons.");
            PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST, null, null);
        }

        /// <summary>
        /// アイコン選択ダイアログを表示する
        /// </summary>
        /// <param name="icon">開始時のアイコン情報</param>
        /// <returns></returns>
        public static FileAssociationIcon? ShowIconDialog(IntPtr hwnd, FileAssociationIcon icon)
        {
            var buffer = ArrayPool<char>.Shared.Rent(1024);
            try
            {
                Span<char> iconPath = buffer;
                int iconIndex = icon.Index;

                if (PInvoke.PickIconDlg((HWND)hwnd, ref iconPath, (uint)iconPath.Length, ref iconIndex) != 0)
                {
                    return new FileAssociationIcon(iconPath.ToString(), iconIndex);
                }
                return null;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Icon の Bitmap を取得する
        /// </summary>
        /// <param name="icon">アイコン情報</param>
        /// <returns></returns>
        public static BitmapSource? GetBitmapSource(FileAssociationIcon icon)
        {
            using var hIcon = PInvoke.ExtractIcon(icon.FilePath, (uint)icon.Index);
            if (hIcon.IsInvalid) return null;

            var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(hIcon.DangerousGetHandle(), Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }
    }
}