using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NeeView
{
    public static class ArchivePageUtility
    {
        /// <summary>
        /// アーカイブの代表となるエントリの PageContent を取得
        /// </summary>
        /// <param name="archiveEntry">基準となるエントリ</param>
        /// <param name="token"></param>
        /// <returns>代表エントリの PageContent. 代表エントリが入力と同じである場合は null</returns>
        public static async Task<PageContent> GetSelectedPageContentAsync(ArchiveEntry archiveEntry, CancellationToken token)
        {
            var entry = await CreateRegularEntryAsync(archiveEntry, token);
            var selectedEntry = await SelectAlternativeEntry(entry, token);
            var factory = new PageContentFactory(null, false);
            var selectedContent = factory.CreatePageContent(selectedEntry, token);
            return selectedContent;
        }

        /// <summary>
        /// アーカイブの代表となるエントリを取得
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<ArchiveEntry> SelectAlternativeEntry(ArchiveEntry entry, CancellationToken token)
        {
            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiveManager.Current.GetSupportedType(entry.SystemPath) == ArchiveType.MediaArchive)
                {
                    return entry;
                }

                return await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, 2, token) ?? entry;
            }
            else
            {
                return entry;
            }
        }

        /// <summary>
        /// 簡易 ArchiveEntry を 正規 ArchiveEntry に変換
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<ArchiveEntry> CreateRegularEntryAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (!entry.IsTemporary) return entry;

            var query = new QueryPath(entry.SystemPath);
            query = query.ResolvePath();
            try
            {
                return await ArchiveEntryUtility.CreateAsync(query.SimplePath, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                return entry;
            }
        }
    }

}
