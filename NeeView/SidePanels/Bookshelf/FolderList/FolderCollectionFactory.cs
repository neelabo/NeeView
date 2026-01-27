using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FolderCollectionFactory
    {
        private readonly bool _isOverlayEnabled;
        private readonly FileSearchEngineProxy? _searchEngine;


        public FolderCollectionFactory(FileSearchEngineProxy? searchEngine, bool isOverlayEnabled)
        {
            _searchEngine = searchEngine;
            _isOverlayEnabled = isOverlayEnabled;
        }


        // フォルダーコレクション作成
        public async ValueTask<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isActive, bool includeSubdirectories, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (path.Scheme == QueryScheme.Root)
            {
                return await CreateRootFolderCollectionAsync(path, isActive, token);
            }
            else if (path.Scheme == QueryScheme.QuickAccess)
            {
                return await CreateQuickAccessFolderCollectionAsync(path, isActive, token);
            }
            else if (path.Scheme == QueryScheme.Bookmark)
            {
                if (path.Search != null)
                {
                    return await CreateSearchBookmarkFolderCollectionAsync(path, isActive, includeSubdirectories, token);
                }
                else
                {
                    return await CreateBookmarkFolderCollectionAsync(path, isActive, token);
                }
            }
            else if (path.Scheme == QueryScheme.File)
            {
                if (path.Search != null)
                {
                    return await CreateSearchFolderCollectionAsync(path, isActive, includeSubdirectories, token);
                }
                else if (path.Path == null || Directory.Exists(path.Path))
                {
                    return await CreateEntryFolderCollectionAsync(path, isActive, token);
                }
                else if (PlaylistArchive.IsSupportExtension(path.Path))
                {
                    return await CreatePlaylistFolderCollectionAsync(path, isActive, token);
                }
                else
                {
                    return await CreateArchiveFolderCollectionAsync(path, isActive, token);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        // 検索コレクション作成
        public async ValueTask<FolderCollection> CreateSearchFolderCollectionAsync(QueryPath path, bool isActive, bool includeSubdirectories, CancellationToken token)
        {
            if (_searchEngine == null) throw new InvalidOperationException("SearchEngine does not exist.");
            if (path.Search is null) throw new InvalidOperationException("path.Search must not be null.");

            token.ThrowIfCancellationRequested();

            try
            {
                _searchEngine.IncludeSubdirectories = includeSubdirectories;
                var result = await _searchEngine.SearchAsync(path.SimplePath, path.Search, token);

                var collection = new FolderSearchCollection(path, result, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        // 通常フォルダーコレクション作成
        private async ValueTask<FolderCollection> CreateEntryFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            FolderCollection collection;
            try
            {
                collection = new FolderEntryCollection(path, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // NOTE: 救済措置。取得に失敗した時はカレントディレクトリに移動
                Debug.WriteLine($"Cannot open: {ex.Message}");
                collection = new FolderEntryCollection(new QueryPath(System.Environment.CurrentDirectory), isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
            }

            return collection;
        }

        // ブックマークフォルダーコレクション作成
        private async ValueTask<FolderCollection> CreateBookmarkFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _ = isActive;

            var collection = new BookmarkFolderCollection(path, _isOverlayEnabled);
            await collection.InitializeItemsAsync(token);

            return collection;
        }

        // 検索ブックマークフォルダーコレクション作成
        private async ValueTask<FolderCollection> CreateSearchBookmarkFolderCollectionAsync(QueryPath path, bool isActive, bool includeSubdirectories, CancellationToken token)
        {
            if (path.Search is null) throw new InvalidOperationException("path.Search must not be null.");

            token.ThrowIfCancellationRequested();

            _ = isActive;

            var collection = new SearchBookmarkFolderCollection(path, _isOverlayEnabled, includeSubdirectories);
            await collection.InitializeItemsAsync(token);

            return collection;
        }

        // プレイリストフォルダーコレクション作成
        public async ValueTask<FolderCollection> CreatePlaylistFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var collection = new PlaylistFolderCollection(path, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // NOTE: 展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                var place = ArchiveEntryUtility.GetExistDirectoryName(path.SimplePath);
                return await CreateEntryFolderCollectionAsync(new QueryPath(place), isActive, token);
            }
        }

        // アーカイブフォルダーコレクション作成
        public async ValueTask<FolderCollection> CreateArchiveFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var collection = new FolderArchiveCollection(path, Config.Current.System.ArchiveRecursiveMode, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // NOTE: アーカイブパスが展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                var place = ArchiveEntryUtility.GetExistDirectoryName(path.SimplePath);
                return await CreateEntryFolderCollectionAsync(new QueryPath(place), isActive, token);
            }
        }

        /// <summary>
        /// Rootコレクション作成
        /// </summary>
        private async ValueTask<FolderCollection> CreateRootFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _ = isActive;

            var collection = new RootFolderCollection(path, _isOverlayEnabled);
            await collection.InitializeItemsAsync(token);
            return collection;
        }

        /// <summary>
        /// クイックアクセスコレクション作成
        /// </summary>
        private async ValueTask<FolderCollection> CreateQuickAccessFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _ = isActive;

            var collection = new QuickAccessFolderCollection(path, _isOverlayEnabled);
            await collection.InitializeItemsAsync(token);
            return collection;
        }
    }
}
