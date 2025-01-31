using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public static class PageFileIO
    {
        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public static bool CanDeletePage(List<Page> pages)
        {
            return pages.All(e => e.CanDelete());
        }

        /// <summary>
        /// 確認ダイアログ作成
        /// </summary>
        public static async Task<MessageDialog> CreateDeleteConfirmDialog(List<Page> pages, bool isCompletely)
        {
            var thumbnail = (pages.Count == 1) ? await pages.First().CreatePageVisualAsync() : null;
            var entries = pages.Select(e => e.ArchiveEntry).ToList();
            return ConfirmFileIO.CreateDeleteConfirmDialog(entries, Properties.TextResources.GetString("FileDeletePageDialog.Title"), thumbnail, isCompletely);
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public static async Task<bool> DeletePageAsync(List<Page> pages)
        {
            if (!pages.Any()) return false;

            bool anyFileModified = false;

            foreach (var group in pages.Select(e => e.ArchiveEntry).GroupBy(e => e.Archive))
            {
                var archiver = group.Key;
                archiver.ClearEntryCache();
                var result = await archiver.DeleteAsync(group.ToList());
                if (result == DeleteResult.Success && archiver is not FolderArchive)
                {
                    anyFileModified = true;
                }
            }

            return anyFileModified;
        }
    }
}
