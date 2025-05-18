using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class MediaSource : IHasCache
    {
        public MediaSource(IStreamSource? streamSource)
        {
            StreamSource = streamSource;
        }

        public MediaSource(string? path, AudioInfo? audioInfo)
        {
            Path = path;
            AudioInfo = audioInfo;
        }

        public IStreamSource? StreamSource { get; }

        public string? Path { get; }

        public AudioInfo? AudioInfo { get; }

        public long CacheSize => (StreamSource as IHasCache)?.CacheSize ?? 0;

        public bool IsValid() => StreamSource != null || Path != null;

        public override string ToString()
        {
            return Path ?? "StreamSource";
        }

        public async ValueTask<Stream> OpenStreamAsync(CancellationToken token)
        {
            if (StreamSource is not null)
            {
                return await StreamSource.OpenStreamAsync(token);
            }
            else if (Path is not null)
            {
                return File.OpenRead(Path);
            }

            throw new InvalidOperationException();
        }

        public void ClearCache()
        {
            (StreamSource as IHasCache)?.ClearCache();
        }
    }
}
