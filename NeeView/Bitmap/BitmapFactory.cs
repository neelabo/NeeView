using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{

    /// <summary>
    /// Bitmap生成
    /// </summary>
    public class BitmapFactory
    {
        private readonly DefaultBitmapFactory _default = new();
        private readonly MagicScalerBitmapFactory _magicScaler = new();


        public BitmapSource CreateBitmapSource(Stream stream, BitmapInfo? info, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    var pos = stream.Position;
                    try
                    {
                        return _magicScaler.Create(stream, info, size, setting.ProcessImageSettings);
                    }
                    catch (InvalidDataException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                        stream.Position = pos;
                        return CreateBitmapSourceWithMagicScaler(stream, info, size, setting, token);
                    }
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
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            return _default.Create(stream, info, size, token);
        }

        /// <summary>
        /// Decode the image using standard functions, then process it with MagicScaler.
        /// </summary>
        /// <remarks>
        /// This is the processing for WIC that MagicScaler does not support.
        /// </remarks>
        private BitmapSource CreateBitmapSourceWithMagicScaler(Stream stream, BitmapInfo? info, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            // convert stream to raw size BitmapImage using default decoder
            BitmapSource bitmap = _default.Create(stream, info, Size.Empty, token);

            // convert bitmap to TIFF stream
            var encoder = new TiffBitmapEncoder() { Compression = TiffCompressOption.None };
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);

            // convert TIFF stream to BitmapImage using MagicScaler
            return _magicScaler.Create(ms, info, size, setting.ProcessImageSettings);
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    _magicScaler.CreateImage(stream, info, outStream, size, format, quality, setting.ProcessImageSettings);
                    return;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            _default.CreateImage(stream, info, outStream, size, format, quality, token);
        }
    }

}
