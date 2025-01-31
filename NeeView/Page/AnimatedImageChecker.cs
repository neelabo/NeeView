using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Text;

namespace NeeView
{
    public static class AnimatedImageChecker
    {
        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _gif89aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        private static readonly byte[] _gif87aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };

        private static readonly byte[] _pngChunkACTL = Encoding.ASCII.GetBytes("acTL");
        private static readonly byte[] _pngChunkIEND = Encoding.ASCII.GetBytes("IEND");

        public static bool IsAnimatedImage(Stream stream, AnimatedImageType imageType)
        {
            return imageType switch
            {
                AnimatedImageType.Gif => IsAnimatedGif(stream),
                AnimatedImageType.Png => IsAnimatedPng(stream),
                _ => IsAnimatedGif(stream) || IsAnimatedPng(stream),
            };
        }

        public static bool IsGif(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var signature = new byte[6].AsSpan();
            _ = stream.Read(signature);
            if (signature.SequenceEqual(_gif89aSignature)) return true;
            if (signature.SequenceEqual(_gif87aSignature)) return true;
            return false;
        }

        public static bool IsPng(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var signature = new byte[8].AsSpan();
            _ = stream.Read(signature);
            return signature.SequenceEqual(_pngSignature);
        }

        /// <summary>
        /// Animated PNG 判定
        /// </summary>
        /// <remarks>
        /// "acTL" チャンクが存在したら Animated PNG であると判定
        /// </remarks>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool IsAnimatedPng(Stream stream)
        {
            if (!IsPng(stream)) return false;

            try
            {
                using var reader = new BinaryReader(stream);
                while (stream.Position < stream.Length)
                {
                    var buff = reader.ReadBytes(8);
                    var length = BinaryPrimitives.ReadInt32BigEndian(new Span<byte>(buff, 0, 4));
                    var chunk = new Span<byte>(buff, 4, 4);

                    //var s = new string(Encoding.ASCII.GetString(chunk));
                    //Debug.WriteLine($"PNG.Chunk: {s}, {length}");

                    if (chunk.SequenceEqual(_pngChunkIEND))
                        return false;
                    if (chunk.SequenceEqual(_pngChunkACTL))
                        return true;
                    stream.Position += length + 4; // 4 is chunk check sum data
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // nop.
            }

            return false;
        }

        public static bool IsAnimatedGif(Stream stream)
        {
            if (!IsGif(stream)) return false;

            stream.Seek(0, SeekOrigin.Begin);
            using var image = Image.FromStream(stream);
            return ImageAnimator.CanAnimate(image);
        }
    }

}
