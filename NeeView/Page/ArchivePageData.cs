namespace NeeView
{
    public class ArchivePageData
    {
        public ArchivePageData(ArchiveEntry archiveEntry, ThumbnailType thumbnailType, PageContent? pageContent, PageDataSource? dataSource)
        {
            ArchiveEntry = archiveEntry;
            ThumbnailType = thumbnailType;
            PageContent = pageContent;
            DataSource = dataSource;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public ThumbnailType ThumbnailType { get; }
        public PageContent? PageContent { get; }
        public PageDataSource? DataSource { get; }
    }

}
