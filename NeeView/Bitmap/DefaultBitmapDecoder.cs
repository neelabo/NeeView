using NeeView.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DefaultBitmapDecoder : IBitmapDecoder
    {
        public string Name => ".NET BitmapImage";

        public bool CheckFormat(Stream stream)
        {
            // NOTE: すべての形式を受け入れる
            return true;
        }

        public BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
        {
            info = info ?? BitmapInfo.Create(stream);

            try
            {
                return Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, size, info, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                token.ThrowIfCancellationRequested();

                // カラープロファイルを無効にして再生成
                Debug.WriteLine($"BitmapImage Failed: {ex.Message}\nTry IgnoreColorProfile ...");
                return Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, size, info, token);
            }
        }

        private static BitmapImage Create(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, Size size, BitmapInfo info, CancellationToken token)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = createOption;
            bitmap.CacheOption = cacheOption;
            bitmap.StreamSource = stream;

            if (size != Size.Empty)
            {
                bitmap.DecodePixelHeight = info.IsTranspose ? (int)size.Width : (int)size.Height;
                bitmap.DecodePixelWidth = info.IsTranspose ? (int)size.Height : (int)size.Width;
            }

            if (info.IsMirrorHorizontal || info.IsMirrorVertical || info.Rotation != Rotation.Rotate0)
            {
                bitmap.DecodePixelWidth = (bitmap.DecodePixelWidth == 0 ? info.PixelWidth : bitmap.DecodePixelWidth) * (info.IsMirrorHorizontal ? -1 : 1);
                bitmap.DecodePixelHeight = (bitmap.DecodePixelHeight == 0 ? info.PixelHeight : bitmap.DecodePixelHeight) * (info.IsMirrorVertical ? -1 : 1);
                bitmap.Rotation = info.Rotation;
            }

            bitmap.EndInit();

            token.ThrowIfCancellationRequested();
            bitmap.Freeze();

            // out of memory, maybe.
            if (stream.Length > 100 * 1024 && bitmap.PixelHeight == 1 && bitmap.PixelWidth == 1)
            {
                Debug.WriteLine("1x1!?");
                throw new ApplicationException(TextResources.GetString("Notice.ImageDecodeFailed"));
            }

            return bitmap;
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            Debug.WriteLine($"DefaultImage: {size.Truncate()}");

            BitmapSource bitmap = Create(stream, info, size, token);
            BitmapFactoryTools.OutputImage(bitmap, outStream, format, quality);
        }

    }
}
