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
