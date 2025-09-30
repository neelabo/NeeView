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
        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _gif89aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        private static readonly byte[] _gif87aSignature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };

        private static readonly byte[] _pngChunkACTL = Encoding.ASCII.GetBytes("acTL");
        private static readonly byte[] _pngChunkIEND = Encoding.ASCII.GetBytes("IEND");

        private static readonly byte[] _webpHead = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // RIFF
        private static readonly byte[] _webpHead2 = new byte[] { 0x57, 0x45, 0x42, 0x50 }; // WEBP


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
            stream.Seek(0, SeekOrigin.Begin);
            var signature = new byte[12];
            stream.ReadExactly(signature);
            stream.Position = 0;

            if (signature.Length < 12)
                return false;

            // WebP signature
            // "RIFF" [4 bytes chunksize] "WEBP"

            for (var i = 0; i < _webpHead.Length; ++i)
                if (signature[i] != _webpHead[i])
                    return false;

            for (var i = 8; i < _webpHead2.Length; ++i)
                if (signature[i] != _webpHead2[i])
                    return false;

            return true;
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

        public static bool IsAnimatedWebp(Stream stream)
        {
            if (!IsWebp(stream)) return false;

            stream.Seek(0, SeekOrigin.Begin);

            const int kMaxBufferLength = 64;
            byte[] buff = new byte[kMaxBufferLength];
            int readBytesLength = stream.Read(buff, 0, buff.Length);

            // WEBP
            if (System.Text.Encoding.ASCII.GetString(buff, 0, 4).Equals("RIFF"))
            {
                for (int i = 12; i < readBytesLength - 8; i += 8)
                {
                    string chname = System.Text.Encoding.ASCII.GetString(buff, i, 4);
                    int chsize = BitConverter.ToInt32(buff, i + 4);

                    if (chname.Equals("ANMF") || chname.Equals("ANIM"))
                    {
                        // ANMF chunk found
                        return true;
                    }
                    i += chsize;
                }
            }

            return false;
        }
    }
}
