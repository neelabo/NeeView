//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using NeeView.PageFrames;
using NeeLaboratory.Linq;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// 現在ページに対する操作
    /// </summary>
    public class BookPageActionControl : IBookPageActionControl
    {
        private readonly Book _book;
        private readonly IBookControl _bookControl;
        private readonly PageFrameBox _box;

        public BookPageActionControl(PageFrameBox box, IBookControl bookControl)
        {
            _box = box;
            _book = _box.Book;
            _bookControl = bookControl;
        }

        #region ページ削除

        // 現在表示しているページのファイル削除可能？
        public bool CanDeleteFile()
        {
            var page = _book?.CurrentPage;
            if (page is null) return false;
            return CanDeleteFile(new List<Page>() { page });
        }

        // 現在表示しているページのファイルを削除する
        public async Task DeleteFileAsync()
        {
            var page = _book?.CurrentPage;
            if (page is null) return;
            await DeleteFileAsync(new List<Page>() { page });
        }

        // 指定ページのファル削除可能？
        public bool CanDeleteFile(List<Page> pages)
        {
            if (!pages.Any()) return false;

            return Config.Current.System.IsFileWriteAccessEnabled && PageFileIO.CanDeletePage(pages);
        }

        // 指定ページのファイルを削除する
        public async Task DeleteFileAsync(List<Page> pages)
        {
            var isCompletely = pages.Any(e => !e.ArchiveEntry.IsFileSystem);
            if (Config.Current.System.IsRemoveConfirmed || isCompletely)
            {
                var dialog = await PageFileIO.CreateDeleteConfirmDialog(pages, isCompletely);
                if (!dialog.ShowDialog().IsPossible)
                {
                    return;
                }
            }

            // ページ削除予約
            pages.ForEach(e => e.IsDeleted = true);

            // 次の現在位置を取得
            var next = _book.Pages.GetValidPage(_book.CurrentPage);

            // ブックからページを削除
            _book.Pages.Remove(pages);

            // ビューのページ表示を停止
            _bookControl.DisposeViewContent(pages);
            await Task.Delay(16);

            // 次のページ位置に移動
            _box.MoveTo(new PagePosition(next?.Index ?? 0, 0), ComponentModel.LinkedListDirection.Next);

            try
            {
                var anyFileModified = await PageFileIO.DeletePageAsync(pages);
                if (anyFileModified)
                {
                    ReloadBook();
                }
            }
            catch (OperationCanceledException)
            {
                ReloadBook();
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Properties.TextResources.GetString("Word.Cause")}: {ex.Message}", Properties.TextResources.GetString("FileDeleteErrorDialog.Title")).ShowDialog();
                ReloadBook();
            }
        }

        private void ReloadBook()
        {
            if (_book.Path == _bookControl.Path)
            {
                _bookControl.ReLoad();
            }
        }

        #endregion ページ削除

        #region ページ出力

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return _book.CurrentPage != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            string? place = _book.CurrentPage?.GetFolderOpenPlace();
            if (!string.IsNullOrWhiteSpace(place))
            {
                ExternalProcess.OpenWithFileManager(place);
            }
        }

        public bool CanCopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            var pages = CollectPages(_book, multiPagePolicy);
            return PageUtility.CanCreateRealizedFilePathList(pages);
        }

        public void CopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            _ = CopyToFolderAsync(parameter, multiPagePolicy, CancellationToken.None);
        }

        public async Task CopyToFolderAsync(DestinationFolder parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
        {
            try
            {
                var pages = CollectPages(_book, multiPagePolicy);
                var paths = await PageUtility.CreateRealizedFilePathListAsync(pages, Config.Current.System.ArchiveCopyPolicy.LimitedRealization(), token);
                await parameter.CopyAsync(paths, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.TextResources.GetString("Bookshelf.CopyToFolderFailed")).ShowDialog();
            }
        }

        // NOTE: parameter は未使用
        public bool CanMoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            var items = CollectPages(_book, multiPagePolicy);
            return Config.Current.System.IsFileWriteAccessEnabled
                && items != null
                && items.Any()
                && items.All(e => e.ArchiveEntry.IsFileSystem && e.ArchiveEntry.Archive is not PlaylistArchive);
        }

        public void MoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            _ = MoveToFolderAsync(parameter, multiPagePolicy, CancellationToken.None);
        }

        public async Task MoveToFolderAsync(DestinationFolder parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
        {
            try
            {
                var pages = CollectPages(_book, multiPagePolicy).Where(e => e.ArchiveEntry.IsFileSystem);
                var paths = pages.Select(e => e.GetFilePlace()).WhereNotNull().ToList();
                await parameter.MoveAsync(paths, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.TextResources.GetString("PageList.Message.MoveToFolderFailed")).ShowDialog();
            }
        }

        public bool CanOpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy)
        {
            return _book.CurrentPage != null;
        }

        // 外部アプリで開く
        public void OpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy)
        {
            _ = OpenApplicationAsync(parameter, multiPagePolicy, CancellationToken.None);
        }

        // 外部アプリで開く
        public async Task OpenApplicationAsync(IExternalApp parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
        {
            var pages = CollectPages(_book, multiPagePolicy);
            var external = new ExternalAppUtility();
            await external.CallAsync(pages, parameter, token);
        }

        private static List<Page> CollectPages(Book book, MultiPagePolicy policy)
        {
            if (book is null)
            {
                return new List<Page>();
            }

            var pages = book.CurrentPages.Distinct();

            switch (policy)
            {
                case MultiPagePolicy.Once:
                    pages = pages.Take(1);
                    break;

                case MultiPagePolicy.AllLeftToRight:
                    if (book.Setting.BookReadOrder == PageReadOrder.RightToLeft)
                    {
                        pages = pages.Reverse();
                    }
                    break;
            }

            return pages.ToList();
        }

        // クリップボードにコピー
        public bool CanCopyToClipboard(CopyFileCommandParameter parameter)
        {
            var pages = CollectPages(_book, parameter.MultiPagePolicy);
            return PageUtility.CanCreateRealizedFilePathList(pages);
        }

        // クリップボードにコピー
        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _ = CopyToClipboardAsync(parameter, CancellationToken.None);
        }

        // クリップボードにコピー
        public async Task CopyToClipboardAsync(CopyFileCommandParameter parameter, CancellationToken token)
        {
            try
            {
                var pages = CollectPages(_book, parameter.MultiPagePolicy);
                await ClipboardUtility.CopyAsync(pages, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                new MessageDialog($"{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("CopyErrorDialog.Title")).ShowDialog();
            }
        }

        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            return _box.GetSelectedPageFrameContent()?.ViewContents.FirstOrDefault() is IHasImageSource;
        }

        // ファイルに保存する (ダイアログ)
        // TODO: OutOfMemory対策
        // TODO: ダイアログにリソースを直接渡すようにする
        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    var exportImageProceduralDialog = new ExportImageProceduralDialog();
                    exportImageProceduralDialog.Owner = MainViewComponent.Current.GetWindow();
                    exportImageProceduralDialog.Show(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Properties.TextResources.GetString("ImageExportErrorDialog.Message")}\n{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
            }
        }

        // ファイルに保存する
        public void Export(ExportImageCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    ExportImageProcedure.Execute(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Properties.TextResources.GetString("ImageExportErrorDialog.Message")}\n{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
            }
        }

        #endregion ページ出力


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }
    }
}
