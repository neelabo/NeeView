using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // ファイルページ用コンテンツのアイコン
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alert,
    }

    /// <summary>
    /// ファイルページ用コンテンツ
    /// FilePageControl のパラメータとして使用される
    /// </summary>
    public class FilePageContent : PageContent
    {
        private readonly FilePageData _source;

        public FilePageContent(ArchiveEntry archiveEntry, FilePageIcon icon, string? message, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
            _source = new FilePageData(archiveEntry, icon, message);
        }

        public override bool IsFileContent => true;

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            return await Task.FromResult(new PictureInfo(DefaultSize));
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            return await Task.FromResult(new PageSource(_source, null, new PictureInfo(DefaultSize)));
        }
    }

}
