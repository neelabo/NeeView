using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageThumbnail
    {
        private readonly PageContent _content;

        public PageThumbnail(PageContent content)
        {
            _content = content;
            Thumbnail = new Thumbnail(_content.ArchiveEntry);
        }

        public Thumbnail Thumbnail { get; }


        public async ValueTask LoadAsync(CancellationToken token)
        {
            //Debug.WriteLine($"LoadThumbnail({Thumbnail.SerialNumber}): {_content.ArchiveEntry}");
            NVDebug.AssertMTA();
            if (Thumbnail.IsValid) return;

            await Thumbnail.InitializeFromCacheAsync(token);
            if (Thumbnail.IsValid) return;

            var source = await LoadThumbnailAsync(token);
            Thumbnail.Initialize(source);
        }

        public virtual async ValueTask<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            // ダミーサムネイル
            return await Task.FromResult(new ThumbnailSource(null));
        }
    }



}
