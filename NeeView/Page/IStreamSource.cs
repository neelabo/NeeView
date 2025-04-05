using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Stream 生成可能な要素
    /// </summary>
    public interface IStreamSource
    {
        public bool Decrypt { get; }

        // TODO: PageContent保持メモリサイズ用。本来の用途ではないのでどうにかする
        long Length => throw new NotSupportedException();

        Task<Stream> OpenStreamAsync(CancellationToken token);

        // TODO: PageContent保持メモリサイズ用。本来の用途ではないのでどうにかする
        long GetMemorySize(); //=> 0L;
    }


    public interface IHasStreamSource
    {
        IStreamSource StreamSource { get; }
    }

}
