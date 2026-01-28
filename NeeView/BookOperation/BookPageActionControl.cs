//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public BookPageActionControl(PageFrameBox box, IBookControl bookControl)
        {
            _box = box;
            _book = _box.Book;
            _bookControl = bookControl;

            var archives = _book.Pages.SourceArchives;
            archives.ForEach(e => AttachArchive(e));
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void AttachArchive(Archive archive)
        {
            _disposables.Add(archive.SubscribeDeleted(Archive_Deleted));
        }

        /// <summary>
        /// 削除予約されているページのみ、実際に削除されたらページも削除する
        /// </summary>
        private async void Archive_Deleted(object? sender, FileSystemEventArgs e)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));

            // TODO: e.FullPath が RawEntryName なのは FolderArchive だけでは？
            // TODO: アーカイブによらずインスタンスを特定する識別子が必要か
            var archive = sender as Archive;
            var page = _book.Pages.FirstOrDefault(x => x.PendingCount > 0 && x.ArchiveEntry.Archive == archive && string.Equals(x.ArchiveEntry.RawEntryName, e.Name, StringComparison.OrdinalIgnoreCase));
            if (page is null) return;

            await RemovePagesAsync([page]);
        }

        /// <summary>
        /// ページ登録解除
        /// </summary>
        /// <param name="pages"></param>
        /// <returns></returns>
        public async ValueTask RemovePagesAsync(List<Page> pages)
        {
            await RemovePagesCoreAsync(pages);

            if (_book.Pages.Count == 0 || pages.Any(e => e.ArchiveEntry.Archive is not FolderArchive))
            {
                ReloadBook();
            }
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
        public async ValueTask DeleteFileAsync()
        {
            var page = _book?.CurrentPage;
            if (page is null) return;
            await DeleteFileAsync(new List<Page>() { page });
        }

        // 指定ページのファル削除可能？
        public bool CanDeleteFile(List<Page> pages)
        {
            if (!pages.Any()) return false;

            return Config.Current.System.IsFileWriteAccessEnabled && PageFileIO.CanDeletePage(pages, true);
        }

        // 指定ページのファイルを削除する
        public async ValueTask DeleteFileAsync(List<Page> pages)
        {
            var entryType = ArchiveEntryUtility.GetDeleteEntryType(pages.Select(e => e.ArchiveEntry));
            if (entryType.IsVarious()) return;

            if (Config.Current.System.IsRemoveConfirmed || entryType.IsIrreversible())
            {
                var dialog = await PageFileIO.CreateDeleteConfirmDialog(pages, entryType);
                if (!dialog.ShowDialog().IsPossible)
                {
                    return;
                }
            }

            await RemovePagesCoreAsync(pages);

            try
            {
                await PageFileIO.DeletePageAsync(pages);
                if (_book.Pages.Count == 0)
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
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {ex.Message}", TextResources.GetString("FileDeleteErrorDialog.Title")).ShowDialog();
                ReloadBook();
            }
        }

        private async ValueTask RemovePagesCoreAsync(List<Page> pages)
        {
            if (pages.Count == 0) return;

            // ページ削除予約
            pages.ForEach(e => e.IsDeleted = true);

            // 次の現在位置を取得
            var next = _book.Pages.GetValidPage(_book.CurrentPage);

            // ブックからページを削除
            _book.Pages.Remove(pages);

            // ビューのページ表示を停止
            DisposeViewContent(pages);
            await Task.Delay(16);

            // 次のページ位置に移動
            _box.MoveTo(new PagePosition(next?.Index ?? 0, 0), ComponentModel.LinkedListDirection.Next);
        }

        /// <summary>
        /// ページ表示の停止
        /// </summary>
        public void DisposeViewContent(IEnumerable<Page> pages)
        {
            _box.DisposeViewContent(pages);
        }

        /// <summary>
        /// ブックの再読み込み
        /// </summary>
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

        public async ValueTask CopyToFolderAsync(DestinationFolder parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
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
                new MessageDialog(ex.Message, TextResources.GetString("Bookshelf.CopyToFolderFailed")).ShowDialog();
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

        public async ValueTask MoveToFolderAsync(DestinationFolder parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
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
                new MessageDialog(ex.Message, TextResources.GetString("PageList.Message.MoveToFolderFailed")).ShowDialog();
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
        public async ValueTask OpenApplicationAsync(IExternalApp parameter, MultiPagePolicy multiPagePolicy, CancellationToken token)
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

        // クリップボードに切り取り
        public bool CanCutToClipboard(CopyFileCommandParameter parameter)
        {
            var pages = CollectPages(_book, parameter.MultiPagePolicy);
            return Config.Current.System.IsFileWriteAccessEnabled && pages.Any() && pages.All(e => e.ArchiveEntry.IsFileSystem);
        }

        // クリップボードに切り取り
        public void CutToClipboard(CopyFileCommandParameter parameter)
        {
            _ = CutToClipboardAsync(parameter, CancellationToken.None);
        }

        // クリップボードに切り取り
        public async ValueTask CutToClipboardAsync(CopyFileCommandParameter parameter, CancellationToken token)
        {
            try
            {
                var pages = CollectPages(_book, parameter.MultiPagePolicy);
                await ClipboardUtility.CutAsync(pages, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {e.Message}", TextResources.GetString("CopyErrorDialog.Title")).ShowDialog();
            }
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
        public async ValueTask CopyToClipboardAsync(CopyFileCommandParameter parameter, CancellationToken token)
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
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {e.Message}", TextResources.GetString("CopyErrorDialog.Title")).ShowDialog();
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
                    new MessageDialog($"{TextResources.GetString("ImageExportErrorDialog.Message")}\n{TextResources.GetString("Word.Cause")}: {e.Message}", TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
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
                    new MessageDialog($"{TextResources.GetString("ImageExportErrorDialog.Message")}\n{TextResources.GetString("Word.Cause")}: {e.Message}", TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
            }
        }

        #endregion ページ出力

    }
}
