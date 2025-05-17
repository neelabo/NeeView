﻿using NeeLaboratory;
using NeeLaboratory.Collection;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO;
using NeeLaboratory.Linq;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class Playlist : BindableBase
    {
        private ObservableCollection<PlaylistItem> _items = new();
        private MultiMap<string, PlaylistItem> _itemsMap = new();
        private string _playlistPath;
        private readonly Lock _lock = new();
        private bool _isDirty;
        private bool _isEditable;
        private bool _isNew;
        private readonly DelayAction _delaySave;


        public Playlist(string path)
        {
            _playlistPath = path;
            _delaySave = new DelayAction(() => Save(false), TimeSpan.FromSeconds(1.0));
        }

        public Playlist(string path, PlaylistSource playlistFile, bool isNew) : this(path)
        {
            _isNew = false;
            this.Items = new ObservableCollection<PlaylistItem>(playlistFile.Items.Select(e => new PlaylistItem(e)));
            this.IsEditable = true;
            this.IsNew = isNew;
        }



        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event EventHandler<PlaylistItemRenamedEventArgs>? ItemRenamed;


        public string Path
        {
            get { return _playlistPath; }
            set { SetProperty(ref _playlistPath, value); }
        }

        public bool IsEditable
        {
            get { return _isEditable; } //&& this.Items != null; }
            set { SetProperty(ref _isEditable, value); }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    lock (_lock)
                    {
                        _isDirty = value;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsNew
        {
            get { return _isNew; }
            private set { SetProperty(ref _isNew, value); }
        }

        public ObservableCollection<PlaylistItem> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged -= OnCollectionChanged;
                    }

                    _items = value;

                    if (_items != null)
                    {
                        _itemsMap = _items.ToMultiMap(x => x.Path, x => x);
                        _items.CollectionChanged += OnCollectionChanged;
                    }
                    else
                    {
                        _itemsMap = new MultiMap<string, PlaylistItem>();
                    }

                    RaisePropertyChanged();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }


        public bool IsThumbnailVisible
        {
            get
            {
                return Config.Current.Playlist.PanelListItemStyle switch
                {
                    PanelListItemStyle.Thumbnail => true,
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0,
                    _ => false,
                };
            }
        }

        public bool IsCurrentPlaylistBookOpened => this.Path is not null && this.Path == BookOperation.Current.Address;

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.Playlist.PanelListItemStyle;
            set => Config.Current.Playlist.PanelListItemStyle = value;
        }

        public string? ErrorMessage { get; private set; }


        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems?.Cast<PlaylistItem>();
            var newItems = e.NewItems?.Cast<PlaylistItem>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _itemsMap = _items.ToMultiMap(x => x.Path, x => x);
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (newItems is null) throw new InvalidOperationException("newItems must not be null when Add");
                    foreach (PlaylistItem item in newItems)
                    {
                        _itemsMap.Add(item.Path, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (oldItems is null) throw new InvalidOperationException("oldItems must not be null when Remove");
                    foreach (PlaylistItem item in oldItems)
                    {
                        _itemsMap.Remove(item.Path, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (newItems is null) throw new InvalidOperationException("newItems must not be null when Replace");
                    if (oldItems is null) throw new InvalidOperationException("oldItems must not be null when Replace");
                    foreach (PlaylistItem item in oldItems.Except(newItems))
                    {
                        _itemsMap.Remove(item.Path, item);
                    }
                    foreach (PlaylistItem item in newItems.Except(oldItems))
                    {
                        _itemsMap.Add(item.Path, item);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            Debug.Assert(_items.Count == _itemsMap.Count);

            CollectionChanged?.Invoke(this, e);
        }

        public PlaylistSource CreatePlaylistSource()
        {
            lock (_lock)
            {
                return new PlaylistSource(_items.Select(e => e.ToPlaylistItem()));
            }
        }

        public PlaylistItem? Find(string path)
        {
            if (_items is null) return null;

            lock (_lock)
            {
                if (_itemsMap.TryGetValue(path, out var item))
                {
                    return item;
                }
            }

            return null;
        }

        public bool TryGetValue(string path, [MaybeNullWhen(false)] out PlaylistItem? item)
        {
            lock (_lock)
            {
                return _itemsMap.TryGetValue(path, out item);
            }
        }

        public List<PlaylistItem> Collect(IEnumerable<string> paths)
        {
            if (paths is null) return new List<PlaylistItem>();

            lock (_lock)
            {
                return paths.Select(e => _itemsMap.TryGetValue(e, out var item) ? item : null)
                    .WhereNotNull()
                    .ToList();
            }
        }

        public List<PlaylistItem>? Add(IEnumerable<string> paths)
        {
            var items = paths.Select(e => new PlaylistItem(e)).ToList();
            return Add(items);
        }

        public PlaylistItem? Add(string path)
        {
            var item = new PlaylistItem(path);
            return Add(item);
        }

        public List<PlaylistItem>? Add(IEnumerable<PlaylistItem> items)
        {
            if (!IsEditable) return null;
            if (items is null) return null;

            lock (_lock)
            {
                var results = new List<PlaylistItem>();
                foreach (var item in items)
                {
                    var result = Add(item);
                    if (result is not null)
                    {
                        results.Add(result);
                    }
                }
                return results;
            }
        }

        public PlaylistItem? Add(PlaylistItem item)
        {
            if (!IsEditable) return null;
            if (item is null) return null;

            lock (_lock)
            {
                // already exists
                if (TryGetValue(item.Path, out var exists))
                {
                    return exists;
                }

                _items.Add(item);
                _isDirty = true;
                return item;
            }
        }

        public List<PlaylistItem>? Insert(PlaylistItem? targetItem, IEnumerable<string> paths)
        {
            var items = paths.Select(e => new PlaylistItem(e)).ToList();
            return Insert(targetItem, items);
        }

        public PlaylistItem? Insert(PlaylistItem? targetItem, string path)
        {
            var item = new PlaylistItem(path);
            return Insert(targetItem, item);
        }

        public List<PlaylistItem>? Insert(PlaylistItem? targetItem, IEnumerable<PlaylistItem> items)
        {
            lock (_lock)
            {
                var index = targetItem != null ? _items.IndexOf(targetItem) : _items.Count;
                if (index < 0) return null;
                return Insert(index, items);
            }
        }

        public PlaylistItem? Insert(PlaylistItem? targetItem, PlaylistItem item)
        {
            lock (_lock)
            {
                var index = targetItem != null ? _items.IndexOf(targetItem) : _items.Count;
                if (index < 0) return null;
                return Insert(index, item);
            }
        }

        public List<PlaylistItem>? Insert(int index, IEnumerable<PlaylistItem> items)
        {
            if (!IsEditable) return null;
            if (items is null) return null;

            lock (_lock)
            {
                var current = Math.Clamp(index, 0, _items.Count);
                var results = new List<PlaylistItem>();
                foreach (var item in items)
                {
                    var result = Insert(current, item);
                    if (result is not null)
                    {
                        results.Add(result);
                        current++;
                    }
                }
                return results;
            }
        }

        public PlaylistItem? Insert(int index, PlaylistItem item)
        {
            if (!IsEditable) return null;
            if (item is null) return null;

            lock (_lock)
            {
                // already exists
                if (TryGetValue(item.Path, out var exists))
                {
                    return exists;
                }

                index = Math.Clamp(index, 0, _items.Count);
                _items.Insert(index, item);
                _isDirty = true;
                return item;
            }
        }

        public void Remove(IEnumerable<PlaylistItem> items)
        {
            if (!IsEditable) return;
            if (items is null) return;

            lock (_lock)
            {
                foreach (var item in items)
                {
                    _items.Remove(item);
                }
                _isDirty = true;
            }
        }

        public void Remove(PlaylistItem item)
        {
            if (!IsEditable) return;
            if (item is null) return;

            lock (_lock)
            {
                _items.Remove(item);
                _isDirty = true;
            }
        }

        public async ValueTask DeleteInvalidItemsAsync(CancellationToken token)
        {
            if (!IsEditable) return;

            // 削除項目収集
            var unlinked = new List<PlaylistItem>();
            foreach (var node in _items)
            {
                if (!await ArchiveEntryUtility.ExistsAsync(node.Path, false, token))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            Remove(unlinked);
            ToastService.Current.Show(new Toast(Properties.TextResources.GetFormatString("Playlist.DeleteItemsMessage", unlinked.Count)));
        }

        public bool Drop(int index, PlaylistItem item)
        {
            if (!IsEditable) return false;
            if (item is null) return false;

            lock (_lock)
            {
                if (index < 0)
                {
                    // Add
                    _items.Add(item);
                    _isDirty = true;
                    return true;
                }
                var newIndex = Math.Clamp(index, 0, _items.Count);
                var oldIndex = _items.IndexOf(item);
                if (oldIndex < 0)
                {
                    // Insert
                    _items.Insert(newIndex, item);
                    _isDirty = true;
                    return true;
                }
                else
                {
                    // Move
                    _items.Move(oldIndex, newIndex);
                    _isDirty = true;
                    return true;
                }
            }
        }

        public void Move(PlaylistItem item, PlaylistItem? targetItem)
        {
            if (!IsEditable) return;
            if (item is null) return;
            if (item == targetItem) return;

            lock (_lock)
            {
                var oldIndex = _items.IndexOf(item);
                if (oldIndex < 0) return;
                var newIndex = targetItem is null ? _items.Count - 1 : _items.IndexOf(targetItem);
                _items.Move(oldIndex, newIndex);
                _isDirty = true;
            }
        }

        // TODO: ソートオーダーの導入で不要になるかも？
        public void Sort()
        {
            if (!IsEditable) return;

            lock (_lock)
            {
                var sorted = _items.OrderBy(e => e.Path, NaturalSort.Comparer);
                this.Items = new ObservableCollection<PlaylistItem>(sorted);

                _isDirty = true;
            }
        }

        public bool Rename(PlaylistItem item, string newName)
        {
            if (!IsEditable) return false;
            if (item is null) return false;
            if (item.Name == newName) return false;

            lock (_lock)
            {
                var oldName = item.Name;
                item.Name = newName;
                ItemRenamed?.Invoke(this, new PlaylistItemRenamedEventArgs(item, oldName));

                _isDirty = true;
            }

            return true;
        }

        public void Open(PlaylistItem item)
        {
            if (item is null) return;

            if (IsCurrentPlaylistBookOpened)
            {
                // try jump in current book.
                var isSuccess = BookOperation.Current.JumpPageWithTarget(this, item.Path);
                if (!isSuccess)
                {
                    ToastService.Current.Show(new Toast(ResourceService.GetString("@Notice.CannotOpenThisItem"), null, ToastIcon.Warning));
                }
            }
            else
            {
                // try open page at new book.
                OpenSource(item);
            }
        }

        public void OpenSource(PlaylistItem item)
        {
            if (item is null) return;

            var options = BookLoadOption.None;
            BookHub.Current.RequestLoad(this, item.Path, null, options, true);
        }

        private List<PlaylistItem> CreateTargetItems(List<PlaylistItem> viewItems, PlaylistItem sample)
        {
            return Config.Current.Playlist.IsGroupBy ? viewItems.Where(e => e.Place == sample.Place).ToList() : viewItems;
        }

        public bool CanMoveUp(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return false;
            if (items is null || items.Count == 0) return false;

            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            if (Config.Current.Playlist.IsGroupBy)
            {
                var place = items.First().Place;
                if (items.Any(e => e.Place != place)) return false;
            }

            var targetItems = CreateTargetItems(viewItems, items.First());
            return items.Count < targetItems.Count && targetItems[items.Count - 1] != items.Last();
        }

        public void MoveUp(IEnumerable<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            var targetItems = CreateTargetItems(viewItems, items.First());

            int top = 0;
            foreach (var item in items)
            {
                var index = _items.IndexOf(item);
                var viewIndex = targetItems.IndexOf(item);
                if (index > top && viewIndex > 0)
                {
                    var target = targetItems[viewIndex - 1];
                    Move(item, target);
                    top = _items.IndexOf(item) + 1;
                }
                else
                {
                    top = index + 1;
                }
            }
        }

        public bool CanMoveDown(List<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return false;
            if (items is null || items.Count == 0) return false;

            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            if (Config.Current.Playlist.IsGroupBy)
            {
                var place = items.First().Place;
                if (items.Any(e => e.Place != place)) return false;
            }

            var targetItems = CreateTargetItems(viewItems, items.First());
            return items.Count < targetItems.Count && targetItems[targetItems.Count - items.Count] != items.First();
        }

        public void MoveDown(IEnumerable<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            var targetItems = CreateTargetItems(viewItems, items.First());

            int bottom = _items.Count - 1;
            foreach (var item in items.Reverse())
            {
                var index = _items.IndexOf(item);
                var viewIndex = targetItems.IndexOf(item);
                if (index < bottom && viewIndex >= 0 && viewIndex < targetItems.Count - 1)
                {
                    var target = targetItems[viewIndex + 1];
                    Move(item, target);
                    bottom = _items.IndexOf(item) - 1;
                }
                else
                {
                    bottom = index - 1;
                }
            }
        }

        public void MoveTop(IEnumerable<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            var targetItems = CreateTargetItems(viewItems, items.First());

            var target = targetItems.FirstOrDefault();
            if (target is null) return;
            foreach (var item in items.Reverse())
            {
                Move(item, target);
                target = item;
            }
        }

        public void MoveBottom(IEnumerable<PlaylistItem> items, List<PlaylistItem> viewItems)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            Debug.Assert(items.SequenceEqual(items.OrderBy(e => _items.IndexOf(e))));

            var targetItems = CreateTargetItems(viewItems, items.First());

            var target = targetItems.LastOrDefault();
            if (target is null) return;

            foreach (var item in items)
            {
                Move(item, target);
                target = item;
            }
        }


        #region Save

        public void DelaySave()
        {
            _delaySave.Request();
        }

        public void Flush()
        {
            _delaySave.Flush();
        }

        public bool Save(bool isForce)
        {
            if (!this.IsEditable) return false;
            if (this.Path is null) return false;

            PlaylistSource source;
            lock (_lock)
            {
                if (!_isDirty && !isForce) return false;
                source = CreatePlaylistSource();
                _isDirty = false;
            }

            using (ProcessLock.Lock())
            {
                if (_isNew)
                {
                    if (source.Items.Count == 0 && !isForce)
                    {
                        return false;
                    }
                    _isNew = false;

                    this.Path = FileIO.CreateUniquePath(this.Path);
                }

                var newFileName = this.Path + ".new.tmp";

                try
                {
                    if (File.Exists(this.Path))
                    {
                        try
                        {

                            File.Delete(newFileName);
                            SaveCore(newFileName);
                            File.Replace(newFileName, this.Path, null);
                        }
                        catch
                        {
                            File.Delete(newFileName);
                            throw;
                        }
                    }
                    else
                    {
                        SaveCore(this.Path);
                    }

                    RemoteCommandService.Current.Send(new RemoteCommand("LoadPlaylist", this.Path), RemoteCommandDelivery.All);
                    return true;
                }
                catch (Exception ex)
                {
                    if (this.IsEditable)
                    {
                        this.IsEditable = false; // 以後編集不可
                        ToastService.Current.Show(new Toast(ex.Message, Properties.TextResources.GetString("Playlist.FailedToSave"), ToastIcon.Error));
                    }
                    return false;
                }
            }

            void SaveCore(string path)
            {
                source.Save(path, true, IsDefaultPlaylistsFolder(path));
            }
        }

        #endregion Save

        #region Load

        private static bool IsDefaultPlaylistsFolder(string path)
        {
            return System.IO.Path.GetDirectoryName(path) == SaveDataProfile.DefaultPlaylistsFolder;
        }

        private static bool IsDefaultPlaylistsFolder(FileInfo fileInfo)
        {
            return fileInfo.Directory?.FullName == SaveDataProfile.DefaultPlaylistsFolder;
        }

        public static Playlist Load(string path, bool createNewFile)
        {
            var file = new FileInfo(path);
            if (file.Exists)
            {
                using (ProcessLock.Lock())
                {
                    try
                    {
                        var playlistFile = PlaylistSourceTools.Load(path);
                        var playlist = new Playlist(path, playlistFile, false);
                        playlist.IsEditable = !file.Attributes.HasFlag(FileAttributes.ReadOnly);
                        return playlist;
                    }
                    catch (Exception ex)
                    {
                        ToastService.Current.Show(new Toast(ex.Message, Properties.TextResources.GetString("Playlist.FailedToLoad"), ToastIcon.Error));
                        return new Playlist(path) { ErrorMessage = ex.Message };
                    }
                }
            }
            else if (file.Directory?.Exists == true || IsDefaultPlaylistsFolder(file))
            {
                var playlist = new Playlist(path, new PlaylistSource(), true);
                if (createNewFile)
                {
                    playlist.Save(true);
                }
                return playlist;
            }
            else
            {
                return new Playlist(path) { ErrorMessage = $"Playlist folder does not exists: '{file.Directory?.FullName}'" };
            }
        }

        #endregion Load

        #region Move to another playlist

        public List<string> CollectAnotherPlaylists()
        {
            return PlaylistHub.GetPlaylistFiles(true)
                .Where(e => e != _playlistPath)
                .ToList();
        }

        public void MoveToAnotherPlaylist(string path, IEnumerable<PlaylistItem> items)
        {
            if (path is null) return;
            if (items is null || !items.Any()) return;
            if (path == _playlistPath) return;

            var playlist = Load(path, true);
            if (!playlist.IsEditable) return;

            var newItems = playlist.Add(items.Select(e => e.Path).ToArray());
            if (newItems is null) throw new InvalidOperationException("Playlist.Add must be succeed");

            var map = items.Where(e => e.IsNameChanged).ToDictionary(e => e.Path, e => e);
            foreach (var item in newItems)
            {
                if (map.TryGetValue(item.Path, out var mapItem))
                {
                    item.Name = mapItem.Name;
                }
            }

            if (playlist.Save(true))
            {
                Remove(items);
            }
        }

        #endregion Move to another playlist

        #region Rename

        public bool CanRename()
        {
            return IsEditable;
        }

        public bool Rename(string newName, bool useErrorDialog = true)
        {
            if (!_isEditable) return false;
            if (string.IsNullOrWhiteSpace(newName)) return false;

            Flush();

            try
            {
                if (FileIO.ContainsInvalidFileNameChars(newName))
                {
                    throw new IOException(Properties.TextResources.GetString("FileRenameInvalidDialog.Message"));
                }

                var newPath = FileIO.CreateUniquePath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path) ?? ".", newName.TrimStart() + System.IO.Path.GetExtension(Path)));
                var oldPath = Path;
                File.Move(oldPath, newPath);
                Path = newPath;
                RemoteCommandService.Current.Send(new RemoteCommand("RenamePlaylist", oldPath, newPath), RemoteCommandDelivery.All);
                return true;
            }
            catch (Exception ex) when (useErrorDialog)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.TextResources.GetString("Playlist.ErrorDialog.Title"), ToastIcon.Error));
                return false;
            }
        }

        #endregion Rename
    }
}
