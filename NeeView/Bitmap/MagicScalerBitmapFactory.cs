using PhotoSauce.MagicScaler;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// MagicScaler BitmapFactory
    /// </summary>
    public class MagicScalerBitmapFactory : IBitmapFactory
    {
        static MagicScalerBitmapFactory()
        {
            CodecManager.Configure(codecs => {
                codecs.Clear();
                codecs.UseWicCodecs(WicCodecPolicy.Microsoft);
            });
        }


        // 注意: sourceは上書きされます
        private static ProcessImageSettings CreateSetting(Size size, string mimeType, ProcessImageSettings? source)
        {
            var setting = source ?? new ProcessImageSettings();

            // width か height を0にすると、アスペクト比を維持したサイズで変換する
            setting.Width = size.IsEmpty ? 0 : Convert.ToInt32(size.Width);
            setting.Height = size.IsEmpty ? 0 : Convert.ToInt32(size.Height);
            setting.ResizeMode = (setting.Width == 0 || setting.Height == 0) ? CropScaleMode.Crop : CropScaleMode.Stretch;
            setting.Anchor = (setting.ResizeMode == CropScaleMode.Crop) ? CropAnchor.Left | CropAnchor.Top : CropAnchor.Center;
            
            setting.TrySetEncoderFormat(mimeType) ;

            return setting;
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
        {
            return Create(stream, info, size, null);
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo? info, Size size, ProcessImageSettings? setting)
        {
            ////Debug.WriteLine($"MagicScalerImage: {size.Truncate()}");
            
            stream.Seek(0, SeekOrigin.Begin);

            using (var ms = new MemoryStream())
            {
                setting = CreateSetting(size, ImageMimeTypes.Bmp, setting);
                MagicImageProcessor.ProcessImage(stream, ms, setting);

                ms.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        //
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            CreateImage(stream, info, outStream, size, format, quality, null);
        }

        //
        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, ProcessImageSettings? setting)
        {
            ////Debug.WriteLine($"MagicScalerImage: {size.Truncate()}");

            stream.Seek(0, SeekOrigin.Begin);

            setting = CreateSetting(size, CreateFormat(format), setting);

            if (format == BitmapImageFormat.Jpeg)
            {
                setting.EncoderOptions = new JpegEncoderOptions(quality, JpegEncoderOptions.Default.Subsample, JpegEncoderOptions.Default.SuppressApp0);
            }

            MagicImageProcessor.ProcessImage(stream, outStream, setting);
        }

        //
        private static string CreateFormat(BitmapImageFormat format)
        {
            return format switch
            {
                BitmapImageFormat.Png => ImageMimeTypes.Png,
                _ => ImageMimeTypes.Jpeg,
            };
        }
    }




}
