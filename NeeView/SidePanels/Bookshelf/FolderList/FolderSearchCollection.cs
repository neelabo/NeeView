using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    [LocalDebug]
    public partial class FolderSearchCollection : FolderCollection, IDisposable
    {
        private readonly FileSearchResultWatcher _searchResult;
        private readonly FolderCollectionEngine? _engine;
        private readonly bool _isWatchSearchResult;


        public FolderSearchCollection(QueryPath path, FileSearchResultWatcher searchResult, bool isWatchSearchResult, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            Debug.Assert(path.Search == searchResult.Keyword);

            _searchResult = searchResult;
            _isWatchSearchResult = isWatchSearchResult;

            if (_isWatchSearchResult)
            {
                _engine = new FolderCollectionEngine(this);
            }
        }


        public override bool IsSearchEnabled => true;

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.WithPath;



        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            await Task.Run(() => InitializeItems(token), token);
        }

        private void InitializeItems(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var items = _searchResult.Items
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            // 除外フィルター
            if (BookshelfFolderList.Current.ExcludeRegex != null)
            {
                items = items
                    .Where(e => e.Name != null && !BookshelfFolderList.Current.ExcludeRegex.IsMatch(e.Name))
                    .ToList();
            }

            var list = Sort(items, token);

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            foreach (var item in list)
            {
                item.TargetPlace = item.TargetPath.GetParent().SimplePath;
            }

            this.Items = new ObservableCollection<FolderItem>(list);

            if (_isWatchSearchResult)
            {
                _searchResult.CollectionChanged += SearchResult_CollectionChanged;
            }
        }


        public override void RequestCreate(QueryPath path)
        {
            if (_disposedValue) return;

            // 除外フィルター
            var excludeRegex = BookshelfFolderList.Current.ExcludeRegex;
            if (excludeRegex != null && excludeRegex.IsMatch(LoosePath.GetFileName(path.SimplePath)))
            {
                return;
            }

            _engine?.RequestCreate(path);
        }

        public override void RequestDelete(QueryPath path)
        {
            if (_disposedValue) return;

            _engine?.RequestDelete(path);
        }

        public override void RequestRename(QueryPath oldPath, QueryPath path)
        {
            if (_disposedValue) return;

            _engine?.RequestRename(oldPath, path);
        }


        private void SearchResult_CollectionChanged(object? sender, CollectionChangedEventArgs<FileItem> e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case CollectionChangedAction.Add:
                    if (e.Item is null) throw new ArgumentException("e.Item is null");
                    LocalDebug.WriteLine($"Add: {e.Item}");
                    RequestCreate(new QueryPath(e.Item.Path));
                    break;

                case CollectionChangedAction.Remove:
                    LocalDebug.WriteLine($"Remove: {e.Item}");
                    if (e.Item is null) throw new ArgumentException("e.Item is null");
                    RequestDelete(new QueryPath(e.Item.Path));
                    break;

                case CollectionChangedAction.Rename:
                    LocalDebug.WriteLine($"Rename: {e.Item} <= {e.OldItem}");
                    if (e.OldItem is null) throw new ArgumentException("e.OldItem is null");
                    if (e.Item is null) throw new ArgumentException("e.Item is null");
                    RequestRename(new QueryPath(e.OldItem.Path), new QueryPath(e.Item.Path));
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 検索結果からFolderItem作成
        /// </summary>
        private FolderItem? CreateFolderItem(FileItem nodeContent)
        {
            if (nodeContent.IsDirectory)
            {
                return CreateFolderItemDirectory(nodeContent);
            }
            else
            {
                return CreateFolderItemFile(nodeContent);
            }
        }

        private FolderItem CreateFolderItemDirectory(FileItem nodeContent)
        {
            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.Directory,
                Place = Place,
                Name = Path.GetFileName(nodeContent.Path),
                TargetPath = new QueryPath(nodeContent.Path),
                LastWriteTime = nodeContent.LastWriteTime,
                CreationTime = nodeContent.CreationTime,
                Length = -1,
                Attributes = FolderItemAttribute.Directory,
                IsReady = true
            };
        }

        private FolderItem? CreateFolderItemFile(FileItem nodeContent)
        {
            if (FileShortcut.IsShortcut(nodeContent.Path))
            {
                var shortcut = new FileShortcut(nodeContent.Path);
                return _folderItemFactory.CreateFolderItem(shortcut);
            }

            var archiveType = ArchiveManager.Current.GetSupportedType(nodeContent.Path);
            if (archiveType != ArchiveType.None)
            {
                var item = new FileFolderItem(_isOverlayEnabled)
                {
                    Type = FolderItemType.File,
                    Place = Place,
                    Name = Path.GetFileName(nodeContent.Path),
                    TargetPath = new QueryPath(nodeContent.Path),
                    LastWriteTime = nodeContent.LastWriteTime,
                    CreationTime = nodeContent.CreationTime,
                    Length = nodeContent.Size,
                    IsReady = true
                };
                if (archiveType == ArchiveType.PlaylistArchive)
                {
                    item.Type = FolderItemType.Playlist;
                    item.Attributes = FolderItemAttribute.Playlist;
                    item.Length = -1;
                }
                return item;
            }

            return null;
        }


        #region IDisposable Support

        private bool _disposedValue = false;

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

                    _searchResult.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}
