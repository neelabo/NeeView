using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class AnimatedPageThumbnail : PageThumbnail
    {
        private readonly AnimatedPageContent _content;

        public AnimatedPageThumbnail(AnimatedPageContent content) : base(content)
        {
            _content = content;
        }


        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
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
                    using (var stream = await CreateEntryStreamAsync(token))
                    {
                        thumbnailRaw = ThumbnailTools.CreateThumbnailImage(stream, _content.PictureInfo, ThumbnailProfile.Current, token);
                    }
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEmptyなサムネイル画像を適用する
                }
            }

            await Task.CompletedTask;
            return new ThumbnailSource(thumbnailRaw);
        }

        /// <summary>
        /// データソースのストリーム取得
        /// </summary>
        /// <remarks>
        /// ロード済の場合はそのメモリから、そうでない場合は ArchiveEntry から。
        /// </remarks>
        /// <returns></returns>
        private async Task<Stream> CreateEntryStreamAsync(CancellationToken token)
        {
            if (_content.Data is byte[] bytes)
            {
                return new MemoryStream(bytes);
            }
            else
            {
                return await _content.ArchiveEntry.OpenEntryAsync(_content.Decrypt, token);
            }
        }
    }
}
