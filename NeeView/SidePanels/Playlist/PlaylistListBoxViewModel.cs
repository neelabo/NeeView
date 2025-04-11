using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class PlaylistListBoxViewModel : BindableBase
    {
        private Playlist? _model;
        private ObservableCollection<PlaylistItem>? _items;
        private PlaylistItem? _selectedItem;
        private Visibility _visibility = Visibility.Hidden;


        public PlaylistListBoxViewModel()
        {
            this.CollectionViewSource = new CollectionViewSource();
            this.CollectionViewSource.Filter += CollectionViewSourceFilter;

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsGroupBy),
                (s, e) => UpdateGroupBy());

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsCurrentBookFilterEnabled),
                (s, e) => UpdateFilter(true));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsDecoratePlace),
                (s, e) => UpdateDisplayPlace());

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsFirstIn),
                (s, e) => UpdateIsFirstIn());

            BookOperation.Current.BookChanged +=
                (s, e) => UpdateFilter(false);

            PageFrameBoxPresenter.Current.ViewPageChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsAddButtonEnabled));
        }


        public bool IsThumbnailVisible => _model is not null && _model.IsThumbnailVisible;

        public bool IsCurrentPlaylistBookOpened => _model is not null && _model.IsCurrentPlaylistBookOpened;

        public CollectionViewSource CollectionViewSource { get; private set; }



        public ObservableCollection<PlaylistItem>? Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        public PlaylistItem? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }


        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }

        public bool IsEditable
        {
            get { return _model is not null && _model.IsEditable; }
        }

        public bool IsAddButtonEnabled
        {
            get { return IsCurrentPageEnabled(); }
        }

        public bool IsGroupBy
        {
            get { return Config.Current.Playlist.IsGroupBy; }
        }

        public bool IsFirstIn
        {
            get { return Config.Current.Playlist.IsFirstIn; }
            set
            {
                if (Config.Current.Playlist.IsFirstIn != value)
                {
                    Config.Current.Playlist.IsFirstIn = value;
                    UpdateIsFirstIn();
                }
            }
        }

        public bool IsLastIn
        {
            get { return !IsFirstIn; }
            set { IsFirstIn = !value; }
        }

        public string? ErrorMessage => _model?.ErrorMessage;


        private void UpdateIsFirstIn()
        {
            RaisePropertyChanged(nameof(IsFirstIn));
            RaisePropertyChanged(nameof(IsLastIn));
        }

        public void SetModel(Playlist model)
        {
            // TODO: 購読の解除。今の所Modelのほうが寿命が短いので問題ないが、安全のため。

            _model = model;

            _model.AddPropertyChanged(nameof(_model.Items),
                (s, e) => AppDispatcher.Invoke(() => UpdateItems()));

            _model.AddPropertyChanged(nameof(_model.IsEditable),
                (s, e) => RaisePropertyChanged(nameof(IsEditable)));

            _model.AddPropertyChanged(nameof(_model.ErrorMessage),
                (s, e) => RaisePropertyChanged(nameof(ErrorMessage)));

            UpdateItems();
        }

        private void CollectionViewSourceFilter(object? sender, FilterEventArgs e)
        {
            var book = BookOperation.Current.Book;

            if (e.Item is null)
            {
                e.Accepted = false;
            }
            else if (Config.Current.Playlist.IsCurrentBookFilterEnabled && book is not null)
            {
                var item = (PlaylistItem)e.Item;
                if (book.IsPlaylist)
                {
                    e.Accepted = book.Path == _model?.Path && book.Pages.PageTargetMap.ContainsKey(item.Path);
                }
                else
                {
                    e.Accepted = item.Path.StartsWith(book.Path, StringComparison.Ordinal) && book.Pages.PageMap.ContainsKey(item.Path);
                }
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void UpdateDisplayPlace()
        {
            if (_items is null) return;

            foreach (var item in _items)
            {
                item.UpdateDisplayPlace();
            }

            UpdateGroupBy();
        }

        private void UpdateGroupBy()
        {
            RaisePropertyChanged(nameof(IsGroupBy));

            this.CollectionViewSource.GroupDescriptions.Clear();
            if (Config.Current.Playlist.IsGroupBy)
            {
                this.CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PlaylistItem.DisplayPlace)));
            }
        }

        private void UpdateFilter(bool isForce)
        {
            if (_model is null) return;
            if (this.CollectionViewSource.View is null) return;

            if (isForce || Config.Current.Playlist.IsCurrentBookFilterEnabled)
            {
                this.CollectionViewSource.View.Refresh();
            }
        }

        private void UpdateItems()
        {
            if (_model is null) return;

            if (this.Items != _model.Items)
            {
                this.Items = _model.Items;
                this.CollectionViewSource.Source = this.Items;
                UpdateGroupBy();
                this.CollectionViewSource.View.CollectionChanged += CollectionView_CollectionChanged;
                EnsureSelectedItem();
            }
        }

        private void CollectionView_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            EnsureSelectedItem();
        }

        private void EnsureSelectedItem()
        {
            if (_model is null) return;
            if (this.CollectionViewSource.View is null) return;

            var items = this.CollectionViewSource.View.Cast<PlaylistItem>();
            if (this.SelectedItem is null || !items.Contains(this.SelectedItem))
            {
                this.SelectedItem = items.FirstOrDefault();
            }
        }

        public bool IsLRKeyEnabled()
        {
            if (_model is null) return false;

            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }

        private int GetSelectedIndex()
        {
            if (_items is null) return -1;

            return this.SelectedItem is null ? -1 : _items.IndexOf(this.SelectedItem);
        }

        private void SetSelectedIndex(int index)
        {
            if (_items is null) return;

            if (_items.Count > 0)
            {
                index = MathUtility.Clamp(index, 0, _items.Count - 1);
                this.SelectedItem = _items[index];
            }
        }

        public bool IsCurrentPageEnabled()
        {
            if (_items is null) return false;

            var book = BookOperation.Current.Book;
            if (book is null) return false;

            var page = BookOperation.Current.Control.SelectedPages.FirstOrDefault();
            if (page is null) return false;

            var bookPlaylist = new BookPlaylist(book, PlaylistHub.Current.Playlist);
            return bookPlaylist.CanRegister(page);
        }

        public PlaylistItem? AddCurrentPage()
        {
            if (_items is null) return null;

            var book = BookOperation.Current.Book;
            if (book is null) return null;

            var page = BookOperation.Current.Control.SelectedPages.FirstOrDefault();
            if (page is null) return null;

            var bookPlaylist = new BookPlaylist(book, PlaylistHub.Current.Playlist);
            if (!bookPlaylist.CanRegister(page)) return null;

            var item = bookPlaylist.Add(page);
            this.SelectedItem = item ?? this.SelectedItem;

            return item;
        }

        public bool CanMoveUp(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (_model is null) return false;

            return _model.CanMoveUp(items, viewItems);
        }

        public void MoveUp(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (_model is null) return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                _model.MoveTop(items, viewItems);
            }
            else
            {
                _model.MoveUp(items, viewItems);
            }
        }

        public bool CanMoveDown(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (_model is null) return false;

            return _model.CanMoveDown(items, viewItems);
        }

        public void MoveDown(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (_model is null) return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                _model.MoveBottom(items, viewItems);
            }
            else
            {
                _model.MoveDown(items, viewItems);
            }
        }

        public async Task<List<PlaylistItem>?> InsertAsync(IEnumerable<string> paths, PlaylistItem? targetItem, CancellationToken token)
        {
            if (_model is null) return null;
            if (!_model.IsEditable) return null;

            var fixedPaths = await ValidatePlaylistItemPath(paths, token);
            var items = _model.Insert(fixedPaths, targetItem);
            return items;
        }

        public async Task<List<string>> ValidatePlaylistItemPath(IEnumerable<string> paths, CancellationToken token)
        {
            var list = new List<string>();
            foreach (var path in paths)
            {
                var entry = await ArchiveEntryUtility.CreateAsync(path, token);
                list.Add(entry.SystemPath);
            }
            return list;
        }

        public void Remove(IEnumerable<PlaylistItem> items)
        {
            if (_model is null) return;
            if (!_model.IsEditable) return;

            var index = GetSelectedIndex();
            this.SelectedItem = null;

            _model.Remove(items);

            SetSelectedIndex(index);
        }

        public void Move(IEnumerable<PlaylistItem> items, PlaylistItem? targetItem)
        {
            if (_model is null) return;
            if (!_model.IsEditable) return;

            _model.Move(items, targetItem);
        }

        public List<string> CollectAnotherPlaylists()
        {
            if (_model is null) return new List<string>();

            return _model.CollectAnotherPlaylists();
        }


        public void MoveToAnotherPlaylist(string path, List<PlaylistItem> items)
        {
            if (_model is null) return;
            if (!_model.IsEditable) return;

            _model.MoveToAnotherPlaylist(path, items);
        }


        public bool Rename(PlaylistItem item, string newName)
        {
            if (_model is null) return false;

            return _model.Rename(item, newName);
        }

        public void Open(PlaylistItem item)
        {
            if (_model is null) return;

            _model.Open(item);
        }

        public void OpenSource(PlaylistItem item)
        {
            if (_model is null) return;

            _model.OpenSource(item);
        }

        public bool CanMovePrevious()
        {
            return _items != null;
        }

        public bool MovePrevious()
        {
            if (_model is null) return false;
            if (this.CollectionViewSource.View is null) return false;

            this.CollectionViewSource.View.MoveCurrentTo(this.SelectedItem);
            this.CollectionViewSource.View.MoveCurrentToPrevious();
            if (this.CollectionViewSource.View.CurrentItem is PlaylistItem item)
            {
                this.SelectedItem = item;
                _model.Open(this.SelectedItem);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanMoveNext()
        {
            return _items != null;
        }

        public bool MoveNext()
        {
            if (_model is null) return false;
            if (this.CollectionViewSource.View is null) return false;

            this.CollectionViewSource.View.MoveCurrentTo(this.SelectedItem);
            this.CollectionViewSource.View.MoveCurrentToNext();
            if (this.CollectionViewSource.View.CurrentItem is PlaylistItem item)
            {
                this.SelectedItem = item;
                _model.Open(this.SelectedItem);
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PlaylistItem> GetViewItems()
        {
            if (_model is null) return [];
            if (this.CollectionViewSource.View is null) return [];

            var collectionView = (CollectionView)CollectionViewSource.View;
            if (collectionView.NeedsRefresh)
            {
                collectionView.Refresh();
            }
            return collectionView.Cast<PlaylistItem>().ToList();
        }
    }
}
