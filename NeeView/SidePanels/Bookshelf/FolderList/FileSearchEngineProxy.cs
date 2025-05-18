using NeeLaboratory.IO.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーリスト用検索エンジン
    /// 検索結果は同時に１つのみ存在
    /// </summary>
    public class FileSearchEngineProxy : IDisposable
    {
        private FileSearchEngine? _searchEngine;
        private bool _disposedValue = false;


        public FileSearchEngineProxy()
        {
        }


        /// <summary>
        /// サブディレクトリを含む
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// 除外属性
        /// </summary>
        public FileAttributes AttributesToSkip => FileIOProfile.Current.AttributesToSkip;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Reset();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public IEnumerable<SearchKey> Analyze(string keyword)
        {
            return FileSearchEngine.DefaultSearcher.Analyze(keyword);
        }

        public async ValueTask<FileSearchResultWatcher> SearchAsync(string path, string keyword, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var searchEngine = GetSearchEngine(path);
                searchEngine.CancelSearch();
                var result = await searchEngine.SearchAsync(keyword, token);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search Exception: {ex.Message}");
                Reset();
                throw;
            }
        }

        private FileSearchEngine GetSearchEngine(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (IsSearchEngineValid(path))
            {
                return _searchEngine;
            }
            else
            {
                _searchEngine?.Dispose();
                _searchEngine = new FileSearchEngine(path, IncludeSubdirectories, AttributesToSkip);
                return _searchEngine;
            }
        }

        [MemberNotNullWhen(true, nameof(_searchEngine))]
        private bool IsSearchEngineValid(string path)
        {
            return _searchEngine != null && _searchEngine.Path == path && _searchEngine.IncludeSubdirectories == IncludeSubdirectories && _searchEngine.AttributesToSkip == AttributesToSkip;
        }

        public void CancelSearch()
        {
            _searchEngine?.CancelSearch();
        }

        public void ResetIfConditionChanged(string path)
        {
            if (_disposedValue) return;

            if (!IsSearchEngineValid(path))
            {
                Reset();
            }
        }

        public void Reset()
        {
            if (_disposedValue) return;

            if (_searchEngine != null)
            {
                _searchEngine.Dispose();
                _searchEngine = null;
            }
        }


    }
}
