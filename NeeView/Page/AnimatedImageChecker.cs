using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace NeeView
{
    public static class AnimatedImageChecker
    {
        private static readonly byte[] _gif89aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        private static readonly byte[] _gif87aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly int _pngChunkACTL = BitConverter.ToInt32("acTL"u8);
        private static readonly int _pngChunkIDAT = BitConverter.ToInt32("IDAT"u8);

        private static readonly int _webpHeaderRIFF = BitConverter.ToInt32("RIFF"u8);
        private static readonly int _webpHeaderWEBP = BitConverter.ToInt32("WEBP"u8);
        private static readonly int _webpChunkVP8X = BitConverter.ToInt32("VP8X"u8);


        public static void InitializeLibrary()
        {
            AnimatedImage.Config.SetNativeLibraryPath(Environment.LibrariesPlatformPath);
        }

        public static bool IsAnimatedImage(Stream stream, AnimatedImageType imageType)
        {
            return imageType switch
            {
                AnimatedImageType.Gif => IsAnimatedGif(stream),
                AnimatedImageType.Png => IsAnimatedPng(stream),
                AnimatedImageType.Webp => IsAnimatedWebp(stream),
                _ => IsAnimatedGif(stream) || IsAnimatedPng(stream) || IsAnimatedWebp(stream),
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

        public static bool IsWebp(Stream stream)
        {
            // WebP signature
            // "RIFF" [4 bytes chunksize] "WEBP"

            stream.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(stream);
            if (reader.ReadInt32() != _webpHeaderRIFF)
            {
                return false;
            }
            _ = reader.ReadInt32(); // skip file size
            if (reader.ReadInt32() != _webpHeaderWEBP)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Animated PNG 判定
        /// </summary>
        /// <remarks>
        /// "IDAT" チャンクの前に "acTL" チャンクが存在したら Animated PNG であると判定。
        /// <para>
        /// https://www.w3.org/TR/png-3/#apng-frame-based-animation<br/>
        /// > To be recognized as an APNG, an acTL chunk must appear in the stream before any IDAT chunks.
        /// </para>
        /// </remarks>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool IsAnimatedPng(Stream stream)
        {
            try
            {
                if (!IsPng(stream)) return false;

                using var reader = new BinaryReader(stream);
                while (stream.Position < stream.Length)
                {
                    var buff = reader.ReadBytes(8);
                    var length = BinaryPrimitives.ReadInt32BigEndian(new Span<byte>(buff, 0, 4));
                    var chunk = new Span<byte>(buff, 4, 4);
                    var chunkId = BitConverter.ToInt32(chunk);

                    //var s = new string(Encoding.ASCII.GetString(chunk));
                    //Debug.WriteLine($"PNG.Chunk: {s}, {length}");

                    if (chunkId == _pngChunkIDAT)
                        return false;
                    if (chunkId == _pngChunkACTL)
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
            try
            {
                if (!IsGif(stream)) return false;

                stream.Seek(0, SeekOrigin.Begin);
                using var image = Image.FromStream(stream);
                return ImageAnimator.CanAnimate(image);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // nop.
            }

            return false;
        }

        public static bool IsAnimatedWebp(Stream stream)
        {
            try
            {
                if (!IsWebp(stream)) return false;

                // skip RIFF header
                stream.Seek(12, SeekOrigin.Begin);

                // check VP8X chunk
                using var reader = new BinaryReader(stream);
                var firstChunkID = reader.ReadInt32();
                var firstChunkSize = reader.ReadInt32();
                if (firstChunkID == _webpChunkVP8X)
                {
                    // Animation flag is 2nd bit of flags
                    var flags = reader.ReadByte();
                    return (flags & 0b0000_0010) != 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // nop.
            }

            return false;
        }
    }
}
