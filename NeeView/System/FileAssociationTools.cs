using NeeView.Interop;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
            foreach(var item in collection)
            {
                item.IsEnabled = false;
            }
        }

        /// <summary>
        /// ファイル関連付け変更をシェルに通知してアイコン表示を更新する
        /// </summary>
        public static void RefreshShellIcons()
        {
            Debug.WriteLine($"FileAssociate: Refresh shell icons.");
            NativeMethods.SHChangeNotify(SHChangeNotifyEvents.SHCNE_ASSOCCHANGED, SHChangeNotifyFlags.SHCNF_IDLIST, 0, 0);
        }

        /// <summary>
        /// アイコン選択ダイアログを表示する
        /// </summary>
        /// <param name="icon">開始時のアイコン情報</param>
        /// <returns></returns>
        public static FileAssociationIcon? ShowIconDialog(FileAssociationIcon icon)
        {
            var iconPath = new StringBuilder(icon.FilePath, 1024);
            int iconIndex = icon.Index;

            if (NativeMethods.PickIconDlg(IntPtr.Zero, iconPath, iconPath.Capacity, ref iconIndex) != 0)
            {
                return new FileAssociationIcon(iconPath.ToString(), iconIndex);
            }
            return null;
        }

        /// <summary>
        /// Icon の Bitmap を取得する
        /// </summary>
        /// <param name="icon">アイコン情報</param>
        /// <returns></returns>
        public static BitmapSource? GetBitmapSource(FileAssociationIcon icon)
        {
            IntPtr hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, icon.FilePath, icon.Index);
            if (hIcon == IntPtr.Zero) return null;

            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return bitmapSource;
            }
            finally
            {
                NativeMethods.DestroyIcon(hIcon);
            }
        }
    }
}