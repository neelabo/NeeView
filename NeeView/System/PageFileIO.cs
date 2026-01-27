using NeeView.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NeeView
{
    public static class PageFileIO
    {
        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public static bool CanDeletePage(List<Page> pages, bool strict)
        {
            return pages.All(e => e.CanDelete(strict));
        }

        /// <summary>
        /// 確認ダイアログ作成
        /// </summary>
        public static async ValueTask<MessageDialog> CreateDeleteConfirmDialog(List<Page> pages, bool isCompletely)
        {
            var thumbnail = (pages.Count == 1) ? await pages.First().CreatePageVisualAsync() : null;
            var entries = pages.Select(e => e.ArchiveEntry).ToList();
            return ConfirmFileIO.CreateDeleteConfirmDialog(entries, TextResources.GetString("FileDeletePageDialog.Title"), thumbnail, isCompletely);
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public static async ValueTask<bool> DeletePageAsync(List<Page> pages)
        {
            if (pages.Count == 0) return false;

            bool success = false;
            foreach (var group in pages.Select(e => e.ArchiveEntry).GroupBy(e => e.Archive))
            {
                var archiver = group.Key;
                var result = await archiver.DeleteAsync(group.ToList());
                if (result == DeleteResult.Success)
                {
                    success = true;
                }
            }

            return success;
        }
    }
}
