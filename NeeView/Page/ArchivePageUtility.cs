using NeeLaboratory.Text;
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
        /// <param name="entry">基準となるエントリ</param>
        /// <param name="token"></param>
        /// <returns>代表エントリの PageContent. 代表エントリが入力と同じである場合は null</returns>
        public static async Task<PageContent> GetSelectedPageContentAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
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
        private static async Task<ArchiveEntry> SelectAlternativeEntry(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            if (FileIO.DirectoryExists(entry.SystemPath) || entry.IsBook())
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

                var depth = Config.Current.Book.BookThumbnailDepth;
                var regex = Config.Current.Book.GetBookThumbnailRegex();
                var match = regex is not null ? new RegexStringMatch(regex) : null;

                return await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, match, depth, decrypt, token) ?? entry;
            }
            else
            {
                return entry;
            }
        }

    }
}
