using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        public static async ValueTask<PageContent> GetSelectedPageContentAsync(ArchiveEntry archiveEntry, bool decrypt, CancellationToken token)
        {
            var entry = await CreateRegularEntryAsync(archiveEntry, decrypt, token);
            var selectedEntry = await SelectAlternativeEntry(entry, decrypt, token);
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
        private static async ValueTask<ArchiveEntry> SelectAlternativeEntry(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                try
                {
                    var target = FolderConfigTools.GetThumbnailTarget(entry.EntryFullName);
                    if (target is not null)
                    {
                        return await ArchiveEntryUtility.CreateAsync(target, ArchiveHint.None, false, token);
                    }
                }
                catch (Exception ex)
                {
                    // 取得時の例外は無視
                    Debug.WriteLine(ex.Message);
                }

                var fileName = Config.Current.Book.BookThumbnailFileName;
                var depth = Config.Current.Book.BookThumbnailDepth;
                return await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, fileName, depth, decrypt, token) ?? entry;
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
        private static async ValueTask<ArchiveEntry> CreateRegularEntryAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            if (!entry.IsTemporary) return entry;

            var query = new QueryPath(entry.SystemPath);
            query = query.ResolvePath();
            try
            {
                return await ArchiveEntryUtility.CreateAsync(query.SimplePath, ArchiveHint.None, decrypt, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                return entry;
            }
        }
    }

}
