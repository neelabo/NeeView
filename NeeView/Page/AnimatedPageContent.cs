using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class AnimatedPageContent : PageContent
    {
        enum PageContentType
        {
            None,
            Bitmap,
            Animated,
        }

        private readonly AnimatedImageType _imageType;
        private PageContentType _contentType = PageContentType.None;

        public AnimatedPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService, AnimatedImageType imageType) : base(archiveEntry, bookMemoryService)
        {
            _imageType = imageType;
        }

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            var streamSource = new ArchiveEntryStreamSource(ArchiveEntry, Decrypt);
            using (var stream = await streamSource.OpenStreamAsync(token))
            {
                var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                var pictureInfo = PictureInfo.Create(bitmapInfo, "AnimatedImage");
                return await Task.FromResult(pictureInfo);
            }
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                var streamSource = new ArchiveEntryStreamSource(ArchiveEntry, Decrypt);
                await streamSource.CreateCacheAsync(Decrypt, token);

                // 初回アニメーション判定
                if (_contentType == PageContentType.None)
                {
                    using var stream = await streamSource.OpenStreamAsync(token);
                    _contentType = AnimatedImageChecker.IsAnimatedImage(stream, _imageType) ? PageContentType.Animated : PageContentType.Bitmap;
                }

                // アニメーション画像
                if (_contentType == PageContentType.Animated)
                {
                    // pictureInfo
                    using var stream = await streamSource.OpenStreamAsync(token);
                    var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                    var pictureInfo = PictureInfo.Create(bitmapInfo, "AnimatedImage");
                    return new AnimatedPageSource(new AnimatedPageData(new MediaSource(streamSource)), null, pictureInfo);
                }
                // 通常画像
                else
                {
                    var loader = new BitmapPageContentLoader();
                    var createPictureInfo = PictureInfo is null;
                    var imageData = await loader.LoadAsync(streamSource, createPictureInfo, true, token);
                    return imageData;
                }

            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }
    }


    /// <summary>
    /// AnimationPage 用データソース
    /// </summary>
    /// TODO: キャッシュサイズ取得だけなので汎用化できそう
    public class AnimatedPageSource : PageSource
    {
        public AnimatedPageSource(object? data, string? errorMessage, PictureInfo? pictureInfo) : base(data, errorMessage, pictureInfo)
        {
        }

        public override long DataSize => (Data as IHasCache)?.CacheSize ?? 0;
    }

}
