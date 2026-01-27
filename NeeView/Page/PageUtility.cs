using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページユーティリティ
    /// </summary>
    public static class PageUtility
    {
        /// <summary>
        /// ページ群の実ファイルリストに変換可能か
        /// </summary>
        public static bool CanCreateRealizedFilePathList(IEnumerable<Page> pages)
        {
            return pages.Any() && pages.All(e => e.ArchiveEntry.CanRealize());
        }

        /// <summary>
        /// ページ群の実ファイルリストを取得
        /// </summary>
        public static async ValueTask<List<string>> CreateRealizedFilePathListAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            return await CreateRealizedFilePathListAsync(pages, ArchivePolicy.SendExtractFile, token);
        }

        /// <summary>
        /// ページ群の実ファイルリストを取得
        /// </summary>
        public static async ValueTask<List<string>> CreateRealizedFilePathListAsync(IEnumerable<Page> pages, ArchivePolicy archivePolicy, CancellationToken token)
        {
            return await ArchiveEntryUtility.RealizeArchiveEntry(pages.Select(e => e.ArchiveEntry), archivePolicy, token);
        }
    }
}
