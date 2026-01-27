//#define LOCAL_DEBUG
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    [LocalDebug]
    public partial class FileSearchResultWatcher : IDisposable, ISearchResult<FileItem>
    {
        /// <summary>
        /// 所属する検索エンジン
        /// </summary>
        private readonly FileSearchEngine _engine;

        /// <summary>
        /// 監視する検索結果
        /// </summary>
        private readonly SearchResult<FileItem> _result;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="result"></param>
        public FileSearchResultWatcher(FileSearchEngine engine, SearchResult<FileItem> result)
        {
            _engine = engine;
            _result = result;

            _engine.Tree.ContentAdded += Tree_ContentAdded;
            _engine.Tree.ContentRemoved += Tree_ContentRemoved;
            _engine.Tree.ContentChanged += Tree_ContentChanged;
        }


        /// <summary>
        /// 検索結果変更
        /// </summary>
        public event EventHandler<CollectionChangedEventArgs<FileItem>>? CollectionChanged;


        // TODO: 投げっぱなし非同期なので例外処理をここで行う
        private async void Tree_ContentAdded(object? sender, FileItemTree.FileTreeContentChangedEventArgs e)
        {
            if (_disposedValue) return;

            var entries = new List<FileItem>() { e.FileItem };
            var items = await _engine.SearchAsync(_result.Keyword, entries, CancellationToken.None);

            foreach (var item in items)
            {
                LocalDebug.WriteLine($"Add: {item.Path}");
                _result.Items.Add(item);
                CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<FileItem>(CollectionChangedAction.Add, item));
            }
        }


        private void Tree_ContentRemoved(object? sender, FileItemTree.FileTreeContentChangedEventArgs e)
        {
            if (_disposedValue) return;

            LocalDebug.WriteLine($"Remove: {e.FileItem.Path}");
            _result.Items.Remove(e.FileItem);
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<FileItem>(CollectionChangedAction.Remove, e.FileItem));
        }

        private void Tree_ContentChanged(object? sender, FileItemTree.FileTreeContentChangedEventArgs e)
        {
            if (_disposedValue) return;
            Debug.Assert(e.OldFileItem != null);
            if (e.OldFileItem is null) return;

            LocalDebug.WriteLine($"Change: {e.FileItem.Path} <- {e.OldFileItem.Path}");
            var index = _result.Items.IndexOf(e.OldFileItem);
            if (index < 0)
            {
                // 項目の追加検索
                Tree_ContentAdded(sender, e);
            }
            else
            {
                // 項目の入れ替え
                _result.Items[index] = e.FileItem;
                CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<FileItem>(CollectionChangedAction.Rename, e.FileItem, e.OldFileItem));
            }
        }

        #region ISearchResult Support

        /// <summary>
        /// 検索結果項目
        /// </summary>
        public ObservableCollection<FileItem> Items => _result.Items;

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword => _result.Keyword;

        /// <summary>
        /// 検索失敗時の例外
        /// </summary>
        public Exception? Exception => _result.Exception;

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _engine.Tree.ContentAdded -= Tree_ContentAdded;
                    _engine.Tree.ContentRemoved -= Tree_ContentRemoved;
                    _engine.Tree.ContentChanged -= Tree_ContentChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
