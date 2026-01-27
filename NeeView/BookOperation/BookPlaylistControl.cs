using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public partial class BookPlaylistControl : BindableBase, IDisposable, IBookPlaylistControl
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private readonly IBookPageControl _pageControl;
        private bool _disposedValue;


        // TODO: book なのに PageFrameBoxPresenter なのが疑問。 PageFrameBox では？
        public BookPlaylistControl(PageFrameBox box, IBookPageControl pageControl)
        {
            _box = box;
            _book = _box.Book;
            Debug.Assert(!_book.IsMedia);
            _pageControl = pageControl;

            PlaylistHub.Current.PlaylistCollectionChanged += Playlist_CollectionChanged;
            _box.ViewContentChanged += Box_ViewContentChanged;
            _book.Pages.PagesSorted += Book_PagesSorted;
            _book.Pages.PageRemoved += Book_PageRemoved;
        }


        // プレイリストに追加、削除された
        [Subscribable]
        public event EventHandler? MarkersChanged;


        // 表示ページのマーク判定
        public bool IsMarked
        {
            get
            {
                var page = _book.CurrentPage;
                if (page is null) return false;
                return _book.Marker.IsMarked(page);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    PlaylistHub.Current.PlaylistCollectionChanged -= Playlist_CollectionChanged;
                    _box.ViewContentChanged -= Box_ViewContentChanged;
                    _book.Pages.PagesSorted -= Book_PagesSorted;
                    _book.Pages.PageRemoved -= Book_PageRemoved;
                    MarkersChanged = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        private void Book_PageRemoved(object? sender, PageRemovedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                // プレイリストから削除
                var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
                bookPlaylist.Remove(e.Pages);

                RaisePropertyChanged(nameof(IsMarked));
            });
        }

        private void Book_PagesSorted(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsMarked)));
        }

        private void Box_ViewContentChanged(object? sender, PageFrames.FrameViewContentChangedEventArgs e)
        {
            if (e.Action < PageFrames.ViewContentChangedAction.Selection) return;
            AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsMarked)));
        }

        private void Playlist_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //NvDebug.AssertSTA();

            var newItems = e.NewItems?.Cast<PlaylistItem>();
            var oldItems = e.OldItems?.Cast<PlaylistItem>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    UpdateMarkers();
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (newItems is null) throw new InvalidOperationException();
                    if (newItems.Any(x => x.Path.StartsWith(_book.Path, StringComparison.Ordinal)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (oldItems is null) throw new InvalidOperationException();
                    if (oldItems.Any(x => x.Path.StartsWith(_book.Path, StringComparison.Ordinal)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (newItems is null) throw new InvalidOperationException();
                    if (oldItems is null) throw new InvalidOperationException();
                    if (!oldItems.SequenceEqual(newItems) && oldItems.Union(newItems).Any(x => x.Path.StartsWith(_book.Path, StringComparison.Ordinal)))
                    {
                        UpdateMarkers();
                    }
                    break;
            }
        }


        // ページマーク登録可能？
        public bool CanMark()
        {
            var page = _book.CurrentPage;
            if (page is null) return false;
            return CanMark(page);
        }

        public bool CanMark(Page page)
        {
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.CanRegister(page);
        }

        // マーカー追加/削除
        public PlaylistItem? SetMark(bool isMark)
        {
            var page = _book.CurrentPage;
            if (page is null) return null;

            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Set(page, isMark);
        }

        // マーカー切り替え
        public PlaylistItem? ToggleMark()
        {
            var page = _book.CurrentPage;
            if (page is null) return null;

            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Toggle(page);
        }

        #region 開発用

        /// <summary>
        /// (開発用) たくさんのページマーク作成
        /// </summary>
        [Conditional("DEBUG")]
        public void Test_MakeManyMarkers()
        {
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);

            for (int index = 0; index < _book.Pages.Count; index += 100)
            {
                var page = _book.Pages[index];
                bookPlaylist.Add(page);
            }
        }

        #endregion

        // マーカー表示更新
        public void UpdateMarkers()
        {
            // 本にマーカを設定
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            var pages = bookPlaylist.Collect();

            _book.Marker.SetMarkers(pages);

            // 表示更新
            MarkersChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(IsMarked));
        }

        public bool CanPrevMarkInPlace(MovePlaylistItemInBookCommandParameter param)
        {
            return (_book.Marker.Markers != null && _book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public bool CanNextMarkInPlace(MovePlaylistItemInBookCommandParameter param)
        {
            return (_book.Marker.Markers != null && _book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public void PrevMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param)
        {
            MoveMarkInPlace(-1, param.IsLoop, param.IsIncludeTerminal);
        }

        public void NextMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param)
        {
            MoveMarkInPlace(+1, param.IsLoop, param.IsIncludeTerminal);
        }

        private void MoveMarkInPlace(int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);
            if (_disposedValue) return;

            var index = _book.CurrentPage?.Index ?? 0;
            var target = _book.Marker.GetNearMarkedPage(index, direction, isLoop, isIncludeTerminal);
            if (target == null) return;

            _pageControl.MoveTo(this, target.Index);
        }

    }

}
