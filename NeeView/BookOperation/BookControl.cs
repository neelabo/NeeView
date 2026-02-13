using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.IO;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookControl : BindableBase, IBookControl, IDisposable, IPendingBook
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private CancellationTokenSource? _realizeTokenSource;
        private readonly SingleFileWatcher _watcher = new();
        private int _pendingCount;
        private NextFolderListBookLoader? _nextBookLoader;


        public BookControl(PageFrameBox box)
        {
            _box = box;
            _book = box.Book;

            _disposables.Add(_box.SubscribePropertyChanged(nameof(_box.IsBusy), (s, e) => RaisePropertyChanged(nameof(IsBusy))));
            _disposables.Add(_watcher.SubscribeDeleted(FileSystemWatcher_Deleted));
        }


        // ブックマーク判定
        public bool IsBookmark => BookmarkCollection.Current.Contains(_book.Path);

        public bool IsBusy => _box.IsBusy;

        public PageSortModeClass PageSortModeClass => _book.PageSortModeClass;

        public string Path => _book.Path;

        public int PendingCount => _pendingCount;


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

        /// <summary>
        /// ブックの再読み込み
        /// </summary>
        public void ReLoad()
        {
            var book = _book;
            if (book is null) return;

            var viewPage = book.CurrentPage;
            var page = book.Pages.GetValidPage(viewPage);
            BookHub.Current.RequestReLoad(this, page?.EntryName);
        }

        /// <summary>
        /// エクスプローラーで開く 実行可能判定
        /// </summary>
        /// <returns></returns>
        public bool CanOpenBookPlace()
        {
            return true;
        }

        /// <summary>
        /// エクスプローラーで開く
        /// </summary>
        public void OpenBookPlace()
        {
            var archivePath = ArchivePath.Create(_book.Path);
            var place = archivePath.SystemPath;

            if (!string.IsNullOrWhiteSpace(place))
            {
                ExternalProcess.OpenWithFileManager(place);
            }
        }

        /// <summary>
        /// 外部アプリで開く 実行判定
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanOpenExternalApp(IExternalApp parameter)
        {
            return true;
        }

        /// <summary>
        /// 外部アプリで開く
        /// </summary>
        /// <param name="parameter"></param>
        public async void OpenExternalApp(IExternalApp parameter)
        {
            _realizeTokenSource?.Cancel();
            _realizeTokenSource = new();
            await OpenExternalAppAsync(parameter, _realizeTokenSource.Token);
        }

        public async ValueTask OpenExternalAppAsync(IExternalApp parameter, CancellationToken token)
        {
            var entry = await ArchiveEntryUtility.CreateAsync(_book.Path, ArchiveHint.None, true, token);
            await ExternalAppUtility.TryOpenExternalAppAsync([entry], parameter, token);
        }

        /// <summary>
        /// コピーコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool CanCopyBookToClipboard()
        {
            return true;
        }

        /// <summary>
        /// コピーコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void CopyBookToClipboard()
        {
            _realizeTokenSource?.Cancel();
            _realizeTokenSource = new();
            await CopyBookAsync(_realizeTokenSource.Token);
        }

        public async Task CopyBookAsync(CancellationToken token)
        { 
            if (!CanCopyBookToClipboard()) return;

            var entry = await ArchiveEntryUtility.CreateAsync(_book.Path, ArchiveHint.None, true, token);
            await ClipboardUtility.TryCopyAsync([entry], token);
        }

        /// <summary>
        /// 切り取りコマンド実行可能判定
        /// </summary>
        public bool CanCutBookToClipboard()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeletable) == 0 && (File.Exists(_book.Path) || Directory.Exists(_book.Path));
        }

        /// <summary>
        /// 切り取りコマンド実行
        /// </summary>
        public async void CutBookToClipboard()
        {
            await CutBookToClipboardAsync(CancellationToken.None);
        }
          
        public async ValueTask CutBookToClipboardAsync(CancellationToken token)
        {
            if (!CanCutBookToClipboard()) return;

            await ClipboardUtility.TryCutAsync(this, token);
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _nextBookLoader?.OpenNextBook();
        }

        public void IncrementPendingCount()
        {
            var count = Interlocked.Increment(ref _pendingCount);
            if (count == 1 && Path is not null)
            {
                _nextBookLoader = new NextFolderListBookLoader(BookshelfFolderList.Current).Ready(Path);
                _watcher.Start(Path);
            }
            RaisePropertyChanged(nameof(PendingCount));
        }

        public void DecrementPendingCount()
        {
            var count = Interlocked.Decrement(ref _pendingCount);
            if (count == 0)
            {
                _watcher.Stop();
            }
            RaisePropertyChanged(nameof(PendingCount));
        }

        /// <summary>
        /// フォルダーへコピー実行可能判定
        /// </summary>
        public bool CanCopyBookToFolder(DestinationFolder parameter)
        {
            return true;
        }

        /// <summary>
        /// フォルダーへコピー
        /// </summary>
        public async void CopyBookToFolder(DestinationFolder parameter)
        {
            _realizeTokenSource?.Cancel();
            _realizeTokenSource = new();
            await CopyBookToFolderAsync(parameter, _realizeTokenSource.Token);
        }

        public async ValueTask CopyBookToFolderAsync(DestinationFolder parameter, CancellationToken token)
        {
            if (!CanCopyBookToFolder(parameter)) return;

            await parameter.TryCopyAsync([_book.Path], token);
        }

        /// <summary>
        /// フォルダーへ移動実行可能判定
        /// </summary>
        public bool CanMoveBookToFolder(DestinationFolder parameter)
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeletable) == 0 && (File.Exists(_book.Path) || Directory.Exists(_book.Path));
        }

        /// <summary>
        /// フォルダーへ移動
        /// </summary>
        public async void MoveBookToFolder(DestinationFolder parameter)
        {
            await MoveBookToFolderAsync(parameter, CancellationToken.None);
        }

        public async ValueTask MoveBookToFolderAsync(DestinationFolder parameter, CancellationToken token)
        {
            if (!CanMoveBookToFolder(parameter)) return;

            await parameter.TryMoveAsync([_book.Path], token);
        }

        /// <summary>
        /// 現在表示しているブックの名前変更可能？
        /// </summary>
        public bool CanRenameBook()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeletable) == 0 && (File.Exists(_book.Path) || Directory.Exists(_book.Path));
        }

        /// <summary>
        /// 名前変更を実行
        /// </summary>
        public async void RenameBook()
        {
            if (!CanRenameBook()) return;

            var result = RenameFileDialog.ShowDialog(_book.Path, TextResources.GetString("RenameBookCommand"));
            if (result.IsPossible)
            {
                var src = result.OldPath;
                var dst = result.NewPath;

                dst = FileIO.CheckInvalidFilename(src, dst, true);
                if (dst is null) return;

                dst = FileIO.CheckChangeExtension(src, dst, true);
                if (dst is null) return;

                dst = FileIO.CheckDuplicateFilename(src, dst, true);
                if (dst is null) return;

                await FileIO.RenameAsync(src, dst, restoreBook: true);
            }
        }

        // 現在表示しているブックの削除可能？
        public bool CanDeleteBook()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeletable) == 0 && (File.Exists(_book.Path) || Directory.Exists(_book.Path));
        }

        // 現在表示しているブックを削除する
        public async void DeleteBook()
        {
            if (!CanDeleteBook()) return;

            try
            {
                var loader = new NextFolderListBookLoader(BookshelfFolderList.Current).Ready(_book.Path);
                var entry = StaticFolderArchive.Default.CreateArchiveEntry(_book.Path, ArchiveHint.None);
                await ConfirmFileIO.DeleteAsync(entry, TextResources.GetString("FileDeleteBookDialog.Title"), null);
                loader.OpenNextBook();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, TextResources.GetString("Message.DeleteFailed")).ShowDialog();
            }
        }


        #region BookCommand : ブックマーク

        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return BookTools.CanBookmark(_book.Path);
        }

        // ブックマーク設定
        public void SetBookmark(bool isBookmark, string? parent)
        {
            if (CanBookmark())
            {
                var query = new QueryPath(_book.Path);

                TreeListNode<IBookmarkEntry>? parentNode = null;
                if (!string.IsNullOrEmpty(parent))
                {
                    var parentQuery = new QueryPath(parent);
                    parentNode = BookmarkCollectionService.FindBookmarkFolder(parentQuery);
                    if (parentNode is null)
                    {
                        ToastService.Current.Show(new Toast($"{TextResources.GetString("Bookmark.Message.FolderNotFoundError")}\n{parentQuery.GetSimplePath(QueryScheme.Bookmark)}", "", ToastIcon.Error));
                        return;
                    }
                }

                if (isBookmark)
                {
                    // ignore temporary directory
                    if (_book.Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal))
                    {
                        ToastService.Current.Show(new Toast(TextResources.GetString("Bookmark.Message.TemporaryNotSupportedError"), "", ToastIcon.Error));
                        return;
                    }

                    BookmarkCollectionService.Add(query, parentNode, null, false);
                }
                else
                {
                    BookmarkCollectionService.Remove(query, parentNode);
                }

                RaisePropertyChanged(nameof(IsBookmark));
            }
        }

        // ブックマーク切り替え
        public void ToggleBookmark(string? parent)
        {
            if (CanBookmark())
            {
                SetBookmark(!IsBookmarkOn(parent), parent);
            }
        }

        public bool IsBookmarkOn(string? parent)
        {
            if (!string.IsNullOrEmpty(parent))
            {
                var query = new QueryPath(_book.Path);
                var parentNode = BookmarkCollectionService.FindBookmarkFolder(new QueryPath(parent));
                if (parentNode is null) return false;
                return BookmarkCollectionService.FindChildBookmark(query, parentNode) != null;
            }
            else
            {
                return IsBookmark;
            }
        }

        #endregion
    }


    public interface IPendingBook : IPendingItem
    {
        string Path { get; }
    }
}
