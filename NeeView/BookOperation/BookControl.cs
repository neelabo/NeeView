using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.IO;

namespace NeeView
{
    public class BookControl : BindableBase, IBookControl, IDisposable
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public BookControl(PageFrameBox box)
        {
            _box = box;
            _book = box.Book;

            _disposables.Add(_box.SubscribePropertyChanged(nameof(_box.IsBusy), (s, e) => RaisePropertyChanged(nameof(IsBusy))));
        }




        // ブックマーク判定
        public bool IsBookmark => BookmarkCollection.Current.Contains(_book.Path);

        public bool IsBusy => _box.IsBusy;

        public PageSortModeClass PageSortModeClass => _book.PageSortModeClass;

        public string? Path => _book.Path;

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

        // 現在表示しているブックの削除可能？
        public bool CanDeleteBook()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeletable) == 0 && (File.Exists(_book.SourcePath) || Directory.Exists(_book.SourcePath));
        }

        // 現在表示しているブックを削除する
        public async void DeleteBook()
        {
            if (CanDeleteBook())
            {
                var bookAddress = _book?.SourcePath;
                if (bookAddress is null) return;

                var item = BookshelfFolderList.Current.FindFolderItem(bookAddress);
                if (item != null)
                {
                    await BookshelfFolderList.Current.RemoveAsync(item);
                }
                else if (FileIO.ExistsPath(bookAddress))
                {
                    var entry = StaticFolderArchive.Default.CreateArchiveEntry(bookAddress, ArchiveHint.None);
                    await ConfirmFileIO.DeleteAsync(entry, TextResources.GetString("FileDeleteBookDialog.Title"), null);
                }
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




}
