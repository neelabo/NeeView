//#define LOCAL_DEBUG
using NeeLaboratory.IO;
using NeeLaboratory.IO.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class FileSearchEngine : IDisposable
    {
        // TODO: cache を無効化
        public static Searcher DefaultSearcher = new Searcher(new FileSearchContext(SearchValueCacheFactory.CreateWithoutCache()));

        private readonly Searcher _searcher;
        private readonly FileItemTree _tree;
        private CancellationTokenSource? _searchCancellationTokenSource;
        private bool _disposedValue;


        public FileSearchEngine(string path, bool includeSubdirectories, FileAttributes attributesToSkip)
        {
            Path = path;
            IncludeSubdirectories = includeSubdirectories;
            AttributesToSkip = attributesToSkip;

            _tree = new FileItemTree(Path, IOExtensions.CreateEnumerationOptions(includeSubdirectories, attributesToSkip));
            _searcher = new Searcher(new FileSearchContext(SearchValueCacheFactory.Create()));
        }


        public bool IncludeSubdirectories { get; }

        public FileAttributes AttributesToSkip { get; }

        public bool IsBusy { get; private set; }

        public string Path { get; private set; }

        public FileItemTree Tree => _tree;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _searchCancellationTokenSource?.Cancel();
                    _searchCancellationTokenSource?.Dispose();
                    _searchCancellationTokenSource = null;

                    _tree.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        public IEnumerable<SearchKey> Analyze(string keyword)
        {
            return _searcher.Analyze(keyword);
        }

        public void CancelSearch()
        {
            _searchCancellationTokenSource?.Cancel();
        }

        public async ValueTask<FileSearchResultWatcher> SearchAsync(string keyword, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();

            IsBusy = true;
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _searchCancellationTokenSource.Token);

            try
            {
                await _tree.InitializeAsync(token);

                using (await _tree.LockAsync(token))
                {
                    var entries = _tree.CollectFileItems();

                    // 検索
                    var items = await Task.Run(() => _searcher.Search(keyword, entries, tokenSource.Token).ToList());

                    // 監視開始
                    var watcher = new FileSearchResultWatcher(this, new SearchResult<FileItem>(keyword, items.Cast<FileItem>()));
                    return watcher;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var watcher = new FileSearchResultWatcher(this, new SearchResult<FileItem>(keyword, null, ex));
                return watcher;
            }
            finally
            {
                tokenSource.Dispose();
                IsBusy = false;
            }
        }


        public async ValueTask<List<FileItem>> SearchAsync(string keyword, IEnumerable<FileItem> entries, CancellationToken token)
        {
            return await Task.Run(() => _searcher.Search(keyword, entries, token).Cast<FileItem>().ToList());
        }
    }

}
