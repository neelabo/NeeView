using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Default BitmapFactory
    /// </summary>
    /// <remarks>
    /// WebPの半透明問題を回避するため、デコーダを順番に試す方式にしている。
    /// </remarks>
    public static class DefaultBitmapFactory
    {
        private static readonly List<IBitmapDecoder> _decoders = [
            new WebPBitmapDecoder(),
            new DefaultBitmapDecoder()
        ];

        public static BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
        {
            foreach (var decoder in _decoders)
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (decoder.CheckFormat(stream))
                {
                    //Debug.WriteLine($"Decoder: {decoder.DecoderName}");
                    stream.Seek(0, SeekOrigin.Begin);
                    return decoder.Create(stream, info, size, token);
                }
            }
            throw new NotSupportedException("Unsupported image format.");
        }

        public static void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            foreach (var decoder in _decoders)
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (decoder.CheckFormat(stream))
                {
                    //Debug.WriteLine($"Decoder: {decoder.DecoderName}");
                    stream.Seek(0, SeekOrigin.Begin);
                    decoder.CreateImage(stream, info, outStream, size, format, quality, token);
                    return;
                }
            }
            throw new NotSupportedException("Unsupported image format.");
        }
    }
}
