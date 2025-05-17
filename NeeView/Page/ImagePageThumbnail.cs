using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ImagePageThumbnail : PageThumbnail
    {
        private readonly PageContent _content;

        public ImagePageThumbnail(PageContent content) : base(content)
        {
            _content = content;
        }

        public override async ValueTask<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            byte[]? thumbnailRaw = null;

            if (_content.IsFailed)
            {
                thumbnailRaw = null;
            }
            else
            {
                try
                {
                    await _content.LoadAsync(token);
                    var data = (_content.Data as IHasRawData)?.RawData;
                    if (data != null)
                    {
                        var pictureSource = PictureSourceFactory.Create(_content);
                        thumbnailRaw = await MemoryControl.Current.RetryFuncWithMemoryCleanupAsync(pictureSource.CreateThumbnailAsync(data, ThumbnailProfile.Current, token));
                    }
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEmptyなサムネイル画像を適用する
                }
            }

            token.ThrowIfCancellationRequested();
            return new ThumbnailSource(thumbnailRaw);
        }
    }
}
