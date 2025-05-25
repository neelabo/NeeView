//#define LOCAL_DEBUG

using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using NeeLaboratory.Generators;

namespace NeeView
{
    /// <summary>
    /// フォルダーエントリコレクション
    /// </summary>
    [LocalDebug]
    public partial class FolderEntryCollection : FolderCollection, IDisposable
    {
        private FileSystemWatcher? _fileSystemWatcher;
        private readonly FolderCollectionEngine? _engine;
        private readonly bool _isWatchFileSystem;


        public FolderEntryCollection(QueryPath path, bool isWatchFileSystem, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
            _isWatchFileSystem = isWatchFileSystem;

            if (isWatchFileSystem)
            {
                _engine = new FolderCollectionEngine(this);
            }
        }


        public override bool IsSearchEnabled => Place.Path != null;

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Normal;


        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            await Task.Run(() => InitializeItems(null, token), token);
        }

        public void InitializeItems(FolderOrder? folderOrderHint, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(Place.SimplePath))
            {
                this.Items = new ObservableCollection<FolderItem>(DriveInfo.GetDrives().Select(e => _folderItemFactory.CreateFolderItem(e)).WhereNotNull());
                return;
            }
            else
            {
                var directory = new DirectoryInfo(Place.SimplePath);

                if (!directory.Exists)
                {
                    var items = new ObservableCollection<FolderItem>
                    {
                        _folderItemFactory.CreateFolderItemEmpty()
                    };
                    this.Items = items;
                }
                else
                {
                    try
                    {
                        var fileSystemInfos = directory.GetFileSystemInfos();
                        token.ThrowIfCancellationRequested();

                        var items = fileSystemInfos
                            .Where(e => FileIOProfile.Current.IsFileValid(e.Attributes))
                            .Select(e => _folderItemFactory.CreateFolderItem(e))
                            .WhereNotNull()
                            .ToList();

                        // RAR連番フィルター
                        if (Config.Current.Bookshelf.IsMultipleRarFilterEnabled)
                        {
                            var archives = items.Where(e => e.Type == FolderItemType.File).ToList();
                            var groups = archives.Select(e => new MultipleArchive(e)).GroupBy(e => e.Key);
                            var part0 = groups.Where(e => e.Key == null).Cast<IEnumerable<MultipleArchive>>().FirstOrDefault() ?? Enumerable.Empty<MultipleArchive>();
                            var part1 = groups.Where(e => e.Key != null).Select(g => g.OrderBy(e => e.PartNumber).First());
                            archives = part0.Concat(part1).Select(e => e.FolderItem).ToList();

                            items = items.Where(e => e.Type != FolderItemType.File).Concat(archives).ToList();
                        }

                        // 除外フィルター
                        if (BookshelfFolderList.Current.ExcludeRegex != null)
                        {
                            items = items
                                .Where(e => e.Name != null && !BookshelfFolderList.Current.ExcludeRegex.IsMatch(e.Name))
                                .ToList();
                        }

                        var folderOrder = folderOrderHint.HasValue ? folderOrderHint.Value : FolderOrder;
                        var list = Sort(items, folderOrder, token);

                        if (!list.Any())
                        {
                            list.Add(_folderItemFactory.CreateFolderItemEmpty());
                        }

                        this.Items = new ObservableCollection<FolderItem>(list);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.Items = new ObservableCollection<FolderItem>() { _folderItemFactory.CreateFolderItemEmpty() };
                        return;
                    }
                }
            }

            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            if (_isWatchFileSystem)
            {
                // フォルダー監視
                InitializeWatcher(Place.SimplePath);
                StartWatch();
            }
        }


        // 分割アーカイブフィルタ用
        private partial class MultipleArchive
        {
            // .partXX.rar のみ対応
            [GeneratedRegex(@"^(.+)\.part(\d+)\.rar$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
            private static partial Regex _regex { get; }

            public FolderItem FolderItem { get; set; }
            public string? Key { get; set; }
            public int PartNumber { get; set; }

            public bool IsPart => Key != null;

            public MultipleArchive(FolderItem folderItem)
            {
                FolderItem = folderItem;

                if (folderItem.Name is null) return;

                var match = _regex.Match(folderItem.Name);
                if (match.Success)
                {
                    Key = match.Groups[1].Value;
                    PartNumber = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                }
            }
        }


        public override void RequestCreate(QueryPath path)
        {
            if (_disposedValue) return;

            LocalDebug.WriteLine($"Create: {path}");
            _engine?.RequestCreate(path);
        }

        public override void RequestDelete(QueryPath path)
        {
            if (_disposedValue) return;

            LocalDebug.WriteLine($"Delete: {path}");
            _engine?.RequestDelete(path);
        }

        public override void RequestRename(QueryPath oldPath, QueryPath path)
        {
            if (_disposedValue) return;

            LocalDebug.WriteLine($"Rename: {oldPath} => {path}");
            _engine?.RequestRename(oldPath, path);
        }

        /// <summary>
        /// ファイルシステム監視初期化
        /// </summary>
        /// <param name="path"></param>
        private void InitializeWatcher(string path)
        {
            _fileSystemWatcher = new FileSystemWatcher();

            try
            {
                _fileSystemWatcher.Path = path;
                _fileSystemWatcher.IncludeSubdirectories = false;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                _fileSystemWatcher.Created += Watcher_Created;
                _fileSystemWatcher.Deleted += Watcher_Deleted;
                _fileSystemWatcher.Renamed += Watcher_Renamed;
                _fileSystemWatcher.Error += Watcher_Error;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            Debug.WriteLine($"FileSystemWatcher: Error!! : {ex} : {ex.Message}");

            // recovery...
            ////var path = _fileSystemWatcher.Path;
            ////TerminateWatcher();
            ////InitializeWatcher(path);
        }

        /// <summary>
        /// ファイルシステム監視終了
        /// </summary>
        private void TerminateWatcher()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Error -= Watcher_Error;
                _fileSystemWatcher.Created -= Watcher_Created;
                _fileSystemWatcher.Deleted -= Watcher_Deleted;
                _fileSystemWatcher.Renamed -= Watcher_Renamed;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        /// <summary>
        /// ファイルシステム監視開始
        /// </summary>
        private void StartWatch()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// ファイル生成イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name is null) return;

            // 除外フィルター
            var excludeRegex = BookshelfFolderList.Current.ExcludeRegex;
            if (excludeRegex != null && excludeRegex.IsMatch(e.Name))
            {
                return;
            }

            RequestCreate(new QueryPath(e.FullPath));
        }

        /// <summary>
        /// ファイル削除イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RequestDelete(new QueryPath(e.FullPath));
        }

        /// <summary>
        /// ファイル名変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            RequestRename(new QueryPath(e.OldFullPath), new QueryPath(e.FullPath));
        }


        #region IDisposable Support

        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_engine != null)
                    {
                        _engine.Dispose();
                    }

                    TerminateWatcher();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
