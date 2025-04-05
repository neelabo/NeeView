using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// メモリバッファを StreamSource にする
    /// </summary>
    public class MemoryStreamSource : IStreamSource
    {
        public MemoryStreamSource(byte[] bytes)
        {
            Bytes = bytes;
        }

        public long Length => Bytes.LongLength;

        public byte[] Bytes { get; }

        public bool Decrypt => false;

        public async Task<Stream> OpenStreamAsync(CancellationToken token)
        {
            return await Task.FromResult(new MemoryStream(Bytes));
        }

        public long GetMemorySize() => Length;
    }

}
