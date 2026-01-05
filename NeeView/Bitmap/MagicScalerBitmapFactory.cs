using PhotoSauce.MagicScaler;
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
        private MagicScalerBitmapDecoder _decoder = new MagicScalerBitmapDecoder();

        public BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
        {
            return _decoder.Create(stream, info, size, token);
        }

        public BitmapSource Create(Stream stream, BitmapInfo? info, Size size, ProcessImageSettings? setting)
        {
            return _decoder.Create(stream, info, size, setting);
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            _decoder.CreateImage(stream, info, outStream, size, format, quality, token);
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, ProcessImageSettings? setting)
        {
            _decoder.CreateImage(stream, info, outStream, size, format, quality, setting);
        }
    }
}
