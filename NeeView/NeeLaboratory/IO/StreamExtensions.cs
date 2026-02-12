using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// entire stream to byte array (common)
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <returns>byte array</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        // TODO: [Obsolete] 廃止予定。ToMemory() に移行
        public static byte[] ToArray(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (!stream.CanSeek)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            var length = stream.Length;

            stream.Seek(0, SeekOrigin.Begin);

            if (length > int.MaxValue)
            {
                throw new IOException("The data is too large to be stored in a byte array.");
            }

            var buffer = new byte[(int)length];

            var totalRead = SafeRead(stream, buffer, 0, (int)length);
            Debug.Assert(totalRead == (int)length);

            return buffer;
        }

        /// <summary>
        /// Stream 全体を読み込む
        /// </summary>
        public static Memory<byte> ToMemory(this Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return ms.MemoryStreamToMemory();
            }
            else
            {
                return stream.StreamToMemory();
            }
        }

        /// <summary>
        /// サイズを指定して Stream を読み込む
        /// </summary>
        public static Memory<byte> ReadToMemory(this Stream stream, int readSize)
        {
            if (stream is MemoryStream ms)
            {
                return ms.MemoryStreamReadToMemory(readSize);
            }
            else
            {
                return stream.StreamReadToMemory(readSize);
            }
        }

        /// <summary>
        /// Stream 全体を読み込む
        /// </summary>
        /// <remarks>
        /// 先頭から。シークできない場合は残りをすべて読み込む
        /// </remarks>
        private static Memory<byte> StreamToMemory(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (!stream.CanSeek)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToMemory();
                }
            }

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var length = stream.Length;
                var buffer = new byte[length];
                stream.ReadExactly(buffer);
                return buffer.AsMemory();
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// サイズを指定して stream を読み込む
        /// </summary>
        /// <remarks>
        /// 現在位置から。
        /// </remarks>
        private static Memory<byte> StreamReadToMemory(this Stream stream, int readSize)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[readSize];
            var readCount = stream.ReadAtLeast(buffer, readSize, false);
            return buffer.AsMemory(0, readCount);
        }

        /// <summary>
        /// MemoryStream 全体を読み込む
        /// </summary>
        /// <remarks>
        /// 可能であればバッファをそのまま返す
        /// </remarks>
        private static Memory<byte> MemoryStreamToMemory(this MemoryStream ms)
        {
            if (ms.TryGetBuffer(out ArraySegment<byte> seg))
            {
                return seg.AsMemory();
            }
            else
            {
                return seg.ToArray().AsMemory();
            }
        }

        /// <summary>
        /// サイズを指定して MemoryStream を読み込む
        /// </summary>
        /// <remarks>
        /// 現在位置から。可能であればバッファのセグメントを返す。
        /// </remarks>
        private static Memory<byte> MemoryStreamReadToMemory(this MemoryStream ms, int readSize)
        {
            var position = ms.Position;
            var rest = (int)(ms.Length - position);
            int length = Math.Min(readSize, rest);
            if (ms.TryGetBuffer(out ArraySegment<byte> seg))
            {
                return seg.AsMemory((int)position, length);
            }
            else
            {
                return ms.StreamReadToMemory(length);
            }
        }

        /// <summary>
        /// 読み込みサイズを保証する Read()
        /// </summary>
        public static int SafeRead(this Stream stream, byte[] array, int offset, int length)
        {
            int totalRead = 0;
            while (totalRead < length)
            {
                int bytesRead = stream.Read(array, offset + totalRead, length - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }
            return totalRead;
        }

        /// <summary>
        /// 読み込みサイズを保証する ReadAsync()
        /// </summary>
        public static async ValueTask<int> SafeReadAsync(this Stream stream, byte[] array, int offset, int length, CancellationToken token = default)
        {
            int totalRead = 0;
            while (totalRead < length)
            {
                int bytesRead = await stream.ReadAsync(array, offset + totalRead, length - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }
            return totalRead;
        }

    }
}
