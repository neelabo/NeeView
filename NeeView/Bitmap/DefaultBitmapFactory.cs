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
    public class DefaultBitmapFactory : IBitmapFactory
    {
        private readonly List<IBitmapDecoder> _decoders = [];

        public DefaultBitmapFactory()
        {
            _decoders.Add(new WebPBitmapDecoder());
            _decoders.Add(new DefaultBitmapDecoder());
        }

        public BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
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

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
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
