using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace NeeLaboratory.IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// stream to byte array (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream stream, int start, int length)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = stream.Read(array.AsSpan());
            Debug.Assert(readSize == length);
            return array;
        }

        /// <summary>
        /// stream to byte array (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <param name="token">cancellation token</param>
        /// <returns></returns>
        public static async ValueTask<byte[]> ToArrayAsync(this Stream stream, int start, int length, CancellationToken token)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = await stream.ReadAsync(array.AsMemory(), token);
            Debug.Assert(readSize == length);
            return array;
        }

        /// <summary>
        /// stream to byte span (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <returns></returns>
        public static ReadOnlySpan<byte> ToSpan(this Stream stream, int start, int length)
        {
            return new ReadOnlySpan<byte>(stream.ToArray(start, length));
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
