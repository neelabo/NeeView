using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ArchivePageContent : PageContent
    {
        public ArchivePageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
        }

        public override PageType PageType => ArchiveEntry.IsDirectory ? PageType.Folder : PageType.Archive;

        public override bool IsFileContent => true;


        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            return await Task.FromResult(new PictureInfo(DefaultSize));
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                var pictureInfo = new PictureInfo(DefaultSize);

                var data = await LoadArchivePageData(token);
                if (data is null)
                {
                    return PageSource.CreateEmpty();
                }
                else if (data.PageContent?.IsFailed == true)
                {
                    return new ArchivePageSource(new ArchivePageData(ArchiveEntry, ThumbnailType.Empty, null, null), null, pictureInfo);
                }
                else
                {
                    return new ArchivePageSource(data, null, pictureInfo);
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

        private async ValueTask<ArchivePageData?> LoadArchivePageData(CancellationToken token)
        {
            var pageContent = await ArchivePageUtility.GetSelectedPageContentAsync(ArchiveEntry, false, token);
            pageContent.Decrypt = false;
            if (pageContent is ArchivePageContent)
            {
                if (pageContent.ArchiveEntry.IsMedia())
                {
                    return new ArchivePageData(ArchiveEntry, ThumbnailType.Media, null, null);
                }
                else
                {
                    return new ArchivePageData(ArchiveEntry, ThumbnailType.Empty, null, null);
                }
            }
            else
            {
                Debug.Assert(pageContent is not ArchivePageContent);
                await pageContent.LoadAsync(token);
                token.ThrowIfCancellationRequested();
                var dataSource = pageContent.PageDataSource;
                if (dataSource.DataState == DataState.None) return null;
                return new ArchivePageData(ArchiveEntry, ThumbnailType.Unique, pageContent, dataSource);
            }
        }
    }

    public class ArchivePageSource : PageSource
    {
        public ArchivePageSource(ArchivePageData? data, string? errorMessage, PictureInfo? pictureInfo) : base(data, errorMessage, pictureInfo)
        {
        }

        public override long DataSize => (Data as ArchivePageData)?.PageContent?.DataSize ?? 0;
    }
}
