//#define LOCAL_DEBUG

using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{

    [LocalDebug]
    public partial class PlaylistHub : BindableBase
    {
        static PlaylistHub() => Current = new PlaylistHub();
        public static PlaylistHub Current { get; }


        private List<object> _playlistCollection;
        private Playlist _playlist;
        private int _playlistLockCount;
        private CancellationTokenSource? _deleteInvalidItemsCancellationToken;
        private bool _isPlaylistDirty;

        private PlaylistHub()
        {
            if (SelectedItem != Config.Current.Playlist.DefaultPlaylist)
            {
                if (!File.Exists(SelectedItem))
                {
                    SelectedItem = Config.Current.Playlist.DefaultPlaylist;
                }
            }

            UpdatePlaylistCollection();

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.PlaylistFolder),
                PlaylistFolder_Changed);

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsCurrentBookFilterEnabled),
                (s, e) => RaisePropertyChanged(nameof(FilterMessage)));

            BookOperation.Current.BookChanged +=
                (s, e) => RaisePropertyChanged(nameof(FilterMessage));

            // NOTE: 応急処置
            //BookOperation.Current.LinkPlaylistHub(this);

            this.AddPropertyChanged(nameof(SelectedItem),
                (s, e) => SelectedItemChanged());

            // initialize first playlist
            _playlist = LoadPlaylist(this.SelectedItem);
            AttachPlaylistEvents(_playlist);
        }


        [Subscribable]
        public event NotifyCollectionChangedEventHandler? PlaylistCollectionChanged;

        [Subscribable]
        public event EventHandler<PlaylistSavedEventArgs>? PlaylistSaved;

        [Subscribable]
        public event EventHandler? Refreshed;


        public string DefaultPlaylist => Config.Current.Playlist.DefaultPlaylist;
        public string NewPlaylist => string.IsNullOrEmpty(Config.Current.Playlist.PlaylistFolder) ? "" : Path.Combine(Config.Current.Playlist.PlaylistFolder, "NewPlaylist.nvpls");

        // NOTE: Separatorを含むことがあるので、List<object>にしている
        public List<object> PlaylistFiles
        {
            get
            {
                if (_playlistCollection is null)
                {
                    UpdatePlaylistCollection();
                }
                return _playlistCollection;
            }
            set { SetProperty(ref _playlistCollection, value); }
        }

        public string SelectedItem
        {
            get
            { return Config.Current.Playlist.CurrentPlaylist; }
            set
            {
                if (Config.Current.Playlist.CurrentPlaylist != value)
                {
                    Config.Current.Playlist.CurrentPlaylist = value;
                }
            }
        }

        public Playlist Playlist
        {
            get { return _playlist; }
        }

        public string? FilterMessage
        {
            get { return Config.Current.Playlist.IsCurrentBookFilterEnabled ? LoosePath.GetFileName(BookOperation.Current.Address) : null; }
        }


        private void PlaylistFolder_Changed(object? sender, PropertyChangedEventArgs e)
        {
            _playlist.Flush();

            UpdatePlaylistCollection(keepSelectedItem: false);

            this.SelectedItem = DefaultPlaylist;
            RaisePropertyChanged(nameof(SelectedItem));

            UpdatePlaylist();
        }

        private void Playlist_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _playlist.DelaySave();
            PlaylistCollectionChanged?.Invoke(this, e);
        }

        private void Playlist_ItemRenamed(object? sender, PlaylistItemRenamedEventArgs e)
        {
            _playlist.DelaySave();
        }

        private void Playlist_Saved(object? sender, PlaylistSavedEventArgs e)
        {
            PlaylistSaved?.Invoke(this, e);
        }

        private void SelectedItemChanged()
        {
            if (!this.PlaylistFiles.Contains(SelectedItem))
            {
                UpdatePlaylistCollection();
            }

            UpdatePlaylist();
        }

        public static List<string> GetPlaylistFiles(bool includeDefault)
        {
            if (!string.IsNullOrEmpty(Config.Current.Playlist.PlaylistFolder))
            {
                try
                {
                    var items = new List<string>();
                    if (includeDefault)
                    {
                        items.Add(Config.Current.Playlist.DefaultPlaylist);
                    }
                    var directory = new DirectoryInfo(System.IO.Path.GetFullPath(Config.Current.Playlist.PlaylistFolder));
                    if (directory.Exists)
                    {
                        var files = directory.GetFiles("*.nvpls")
                            .Select(e => e.FullName)
                            .Where(e => !includeDefault || e != Config.Current.Playlist.DefaultPlaylist)
                            .OrderBy(e => e, NaturalSort.Comparer).ToList();

                        items.AddRange(files);
                    }
                    return items;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return new List<string>();
        }

        [MemberNotNull(nameof(_playlistCollection))]
        public void UpdatePlaylistCollection(bool keepSelectedItem = true)
        {
            _playlistLockCount++;
            var selectedItem = this.SelectedItem;
            try
            {
                var items = new List<object>();
                items.AddRange(GetPlaylistFiles(true));

                if (keepSelectedItem && selectedItem != null && !items.Any(e => selectedItem.Equals(e)))
                {
                    items.Add(new Separator());
                    items.Add(selectedItem);
                }

                //this.PlaylistFiles = items;
                _playlistCollection = items;
                RaisePropertyChanged(nameof(PlaylistFiles));
            }
            finally
            {
                if (keepSelectedItem)
                {
                    this.SelectedItem = selectedItem;
                }
                _playlistLockCount--;
            }
        }


        public void UpdatePlaylist()
        {
            if (_playlistLockCount <= 0 && (_playlist is null || _isPlaylistDirty || _playlist?.Path != this.SelectedItem))
            {
                if (!_isPlaylistDirty && _playlist != null)
                {
                    _playlist.Flush();
                }

                SetPlaylist(LoadPlaylist(this.SelectedItem));
                _isPlaylistDirty = false;
            }
        }

        private Playlist LoadPlaylist(string path)
        {
            bool isCreateNewFile = path != DefaultPlaylist;
            return Playlist.Load(path, isCreateNewFile);
        }

        public void Reload(string path)
        {
            LocalDebug.WriteLine($"Path={path}");

            var reloadPath = GetReloadPlaylistPath(path);

            if (SelectedItem != reloadPath)
            {
                SelectedItem = reloadPath;
                return;
            }

            if (!_playlist.FileStamp.IsLatest())
            {
                ReloadPlaylist();
            }
        }

        public void ReloadPlaylist()
        {
            if (this.SelectedItem == _playlist.Path)
            {
                SetPlaylist(LoadPlaylist(this.SelectedItem));
                _isPlaylistDirty = false;
            }
        }

        /// <summary>
        /// プレイリストの状態をリフレッシュ
        /// </summary>
        /// <remarks>
        /// インポート後に呼ばれる
        /// </remarks>
        public void Refresh()
        {
            PlaylistHub.Current.UpdatePlaylistCollection();
            PlaylistHub.Current.ReloadPlaylist();
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 再読み込みするプレイリストのパスを決定する。
        /// </summary>
        /// <param name="path">現在のプレイリストが存在しないときの代替。ファイル名が変更されたとき用</param>
        /// <returns></returns>
        private string GetReloadPlaylistPath(string path)
        {
            if (File.Exists(SelectedItem))
            {
                return SelectedItem;
            }
            if (File.Exists(path))
            {
                return path;
            }
            return DefaultPlaylist;
        }

        private void SetPlaylist(Playlist value)
        {
            if (_playlist == value) return;

            DetachPlaylistEvents(_playlist);
            _playlist = value;
            AttachPlaylistEvents(_playlist);

            RaisePropertyChanged(nameof(Playlist));
            PlaylistCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void AttachPlaylistEvents(Playlist playlist)
        {
            playlist.CollectionChanged += Playlist_CollectionChanged;
            playlist.ItemRenamed += Playlist_ItemRenamed;
            playlist.Saved += Playlist_Saved;
        }

        private void DetachPlaylistEvents(Playlist playlist)
        {
            playlist.CollectionChanged -= Playlist_CollectionChanged;
            playlist.ItemRenamed -= Playlist_ItemRenamed;
            playlist.Saved -= Playlist_Saved;
        }

        public void Flush()
        {
            _playlist.Flush();
        }

        public void CreateNew()
        {
            if (string.IsNullOrEmpty(NewPlaylist))
            {
                new MessageDialog(TextResources.GetString("PlaylistErrorDialog.FolderIsNotSet"), TextResources.GetString("Word.Error")).ShowDialog();
                return;
            }

            var newPath = FileIO.CreateUniquePath(NewPlaylist);
            SelectedItem = newPath;
        }

        public void Open()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "NeeView Playlist|*.nvpls|All|*.*";

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                SelectedItem = dialog.FileName;
            }
        }

        public bool CanDelete()
        {
            return SelectedItem != null && SelectedItem != DefaultPlaylist;
        }

        public async ValueTask DeleteAsync()
        {
            if (!CanDelete()) return;
            if (!File.Exists(SelectedItem)) return;

            var entry = ArchiveEntryUtility.CreateTemporaryEntry(SelectedItem);
            bool isSucceed = await ConfirmFileIO.DeleteAsync(entry, TextResources.GetString("Playlist.DeleteDialog.Title"), null);
            if (isSucceed)
            {
                SelectedItem = DefaultPlaylist;
            }
        }

        public string SelectedItemName
        {
            get => Path.GetFileNameWithoutExtension(SelectedItem);
            set => Rename(value, false);
        }

        public bool CanRename()
        {
            return _playlist.CanRename();
        }

        public bool Rename(string newName, bool useErrorDialog = true)
        {
            if (_playlist.Rename(newName, useErrorDialog))
            {
                SelectedItem = _playlist.Path;
                return true;
            }

            return false;
        }

        public void OpenAsBook()
        {
            _playlist.Flush();
            BookHub.Current.RequestLoad(this, SelectedItem, null, BookLoadOption.IsBook, true);
        }

        public string GetNextPlaylist(int offset)
        {
            Debug.Assert(-1 <= offset || offset <= 1);

            if (_playlistCollection is null || _playlistCollection.Count == 0)
            {
                return "";
            }

            var items = _playlistCollection.OfType<string>().ToList();
            if (items.Count == 0)
            {
                return "";
            }

            int index = (items.IndexOf(SelectedItem) + items.Count + offset) % items.Count;
            return items[index];
        }

        /// <summary>
        /// すべてのプレイリストの項目のパスを一括変更
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenameItemPathRecursive(string src, string dst)
        {
            LocalDebug.WriteLine($"Begin: src={src}, dst={dst}");
          
            _playlist.Flush();
            UpdatePlaylistCollection();

            var files = _playlistCollection.OfType<string>();
            foreach (var file in files)
            {
                try
                {
                    using (ProcessLock.Lock())
                    {
                        var playlist = PlaylistSourceEditor.Create(file);
                        playlist?.RenamePathRecursive(src, dst);
                        playlist?.Save();
                    }
                }
                catch (Exception ex)
                {
                    // できるだけ編集できればよいので例外はスルー
                    Debug.WriteLine(ex);
                }
            }

            LocalDebug.WriteLine($"End");
        }

        #region Playlist Controls

        public async ValueTask DeleteInvalidItemsAsync()
        {
            _deleteInvalidItemsCancellationToken?.Cancel();
            _deleteInvalidItemsCancellationToken = new CancellationTokenSource();
            await _playlist.DeleteInvalidItemsAsync(_deleteInvalidItemsCancellationToken.Token);
        }

        public void SortItems()
        {
            _playlist.Sort();
        }

        #endregion
    }
}
