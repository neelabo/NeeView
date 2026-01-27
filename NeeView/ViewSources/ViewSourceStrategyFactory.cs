using System;

namespace NeeView
{
    public static class ViewSourceStrategyFactory
    {
        public static IViewSourceStrategy? Create(PageContent pageContent, PageDataSource data)
        {
            if (!data.IsLoaded) return null;

            // NOTE: エラーデータは事前に弾いておくこと
            if (data.IsFailed) return null;

            var archiveEntry = pageContent.ArchiveEntry;
            var pictureInfo = data.PictureInfo;

            return data.Data switch
            {
                BitmapPageData => new BitmapViewSourceStrategy(archiveEntry, pictureInfo),
                AnimatedPageData => new AnimatedViewSourceStrategy(),
                MediaPageData => new MediaViewSourceStrategy(),
                PdfPageData => new PdfViewSourceStrategy(archiveEntry, pictureInfo),
                SvgPageData => new SvgViewSourceStrategy(archiveEntry, pictureInfo),
                ArchivePageData => new ArchiveViewSourceStrategy(),
                FilePageData => new FileViewSourceStrategy(),
                EmptyPageData => new EmptyViewSourceStrategy(),
                _ => throw new NotSupportedException(),
            };
        }
    }

}
