using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public static class DriveIconUtility
    {
        private static readonly Dictionary<string, ImageSource> _iconCache = new();

        /// <summary>
        /// 非同期のドライブアイコン画像生成
        /// </summary>
        /// <param name="path">ドライブパス</param>
        /// <param name="callback">画像生成後のコールバック</param>
        public static async ValueTask CreateDriveIconAsync(string path, Action<BitmapSourceCollection> callback)
        {
            await AppDispatcher.BeginInvoke(async () => await CreateDriveIcon());

            async ValueTask CreateDriveIcon()
            {
                for (int i = 0; i < 2; ++i) // retry 2 time.
                {
                    try
                    {
                        var bitmapSource = FileIconCollection.Current.CreateFileIcon(path, IO.FileIconType.Drive, true, false);
                        if (bitmapSource != null)
                        {
                            bitmapSource.Freeze();
                            callback?.Invoke(bitmapSource);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"CreateDriveIcon({path}): " + ex.Message);
                    }
                    await Task.Delay(500);
                }
            }
        }


        public static void SetDriveIconCache(string path, ImageSource? bitmapSource)
        {
            if (bitmapSource is null) return;

            _iconCache[path] = bitmapSource;
        }

        public static ImageSource? GetDriveIconCache(string path)
        {
            if (_iconCache.TryGetValue(path, out ImageSource? bitmapSource))
            {
                return bitmapSource;
            }
            else
            {
                return FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);
            }
        }
    }
}
