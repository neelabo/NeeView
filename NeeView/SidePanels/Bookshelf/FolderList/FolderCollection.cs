using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NeeView.Collections.Generic;
using System.Collections;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;

namespace NeeView
{
    public class FolderCollectionChangedEventArgs : EventArgs
    {
        public FolderCollectionChangedEventArgs(CollectionChangeAction action, FolderItem item)
        {
            this.Action = action;
            this.Item = item;
        }

        public CollectionChangeAction Action { get; set; }
        public FolderItem Item { get; set; }
    }

    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public abstract partial class FolderCollection : BindableBase, IDisposable, IEnumerable<FolderItem>
    {
        private static readonly ObservableCollection<FolderItem> _itemsEmpty = new();
        private ObservableCollection<FolderItem> _items = _itemsEmpty;
        protected FolderItemFactory _folderItemFactory;
        protected bool _isOverlayEnabled;
        private readonly System.Threading.Lock _lock = new();


        protected FolderCollection(QueryPath path, bool isOverlayEnabled)
        {
            _folderItemFactory = new FolderItemFactory(path, isOverlayEnabled);

            this.Place = path;
            _isOverlayEnabled = isOverlayEnabled;

            // HACK: FullPathにする。過去のデータも修正が必要
            this.FolderParameter = new FolderParameter(Place.SimplePath);
            this.FolderParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, EventArgs.Empty);
        }


        [Subscribable]
        public event EventHandler<FolderCollectionChangedEventArgs>? CollectionChanging;

        [Subscribable]
        public event EventHandler<FolderCollectionChangedEventArgs>? CollectionChanged;

        [Subscribable]
        public event EventHandler? ParameterChanged;


        // indexer
        public FolderItem this[int index]
        {
            get { Debug.Assert(index >= 0 && index < Items.Count); return Items[index]; }
            private set { Items[index] = value; }
        }

        /// <summary>
        /// 検索可能？
        /// </summary>
        public virtual bool IsSearchEnabled => false;

        /// <summary>
        /// ソート適用の種類
        /// </summary>
        public abstract FolderOrderClass FolderOrderClass { get; }

        /// <summary>
        /// Folder Parameter
        /// </summary>
        public FolderParameter FolderParameter { get; private set; }

        /// <summary>
        /// Collection本体
        /// </summary>
        public ObservableCollection<FolderItem> Items
        {
            get { return _items; }
            protected set
            {
                if (_items != value)
                {
                    if (_items != _itemsEmpty)
                    {
                        _items.CollectionChanged -= Items_CollectionChanged;
                    }
                    _items = value;
                    if (_items != _itemsEmpty)
                    {
                        _items.CollectionChanged += Items_CollectionChanged;
                    }
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// Collection count
        /// </summary>
        public int Count => Items.Count;

        /// <summary>
        /// Valid collection count
        /// </summary>
        public int ValidCount => IsEmpty() ? 0 : Items.Count;

        /// <summary>
        /// フォルダーの場所(クエリ)
        /// </summary>
        public QueryPath Place { get; private set; }

        /// <summary>
        /// フォルダーの場所(表示用)
        /// </summary>
        public string PlaceDisplayString => Place.DisplayPath;

        /// <summary>
        /// フォルダーの場所(クエリー添付)
        /// </summary>
        public string QueryPath => Place.SimpleQuery;

        /// <summary>
        /// フォルダーの並び順
        /// </summary>
        public FolderOrder FolderOrder => FolderParameter.FolderOrder;

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        private int RandomSeed => FolderParameter.RandomSeed;

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => Items != null;



        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ValidCount));
        }


        public virtual async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public bool IsEmpty()
        {
            return Items == null
                || Items.Count == 0
                || Items.Count == 1 && Items[0].IsEmpty();
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool IsDirty(FolderParameter folder)
        {
            return (Place.SimplePath != folder.Path || FolderOrder != folder.FolderOrder || RandomSeed != folder.RandomSeed);
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return IsDirty(new FolderParameter(Place.SimplePath));
        }


        /// <summary>
        /// パスから項目インデックス取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int IndexOfPath(QueryPath path)
        {
            var item = Items.FirstOrDefault(e => e.TargetPath.Equals(path));
            return (item != null) ? Items.IndexOf(item) : -1;
        }


        public FolderItem? FirstOrDefault(Func<FolderItem, bool> predicate)
        {
            return Items.FirstOrDefault(predicate);
        }

        /// <summary>
        /// 先頭項目を取得
        /// </summary>
        /// <returns></returns>
        public FolderItem? FirstOrDefault()
        {
            return Items.FirstOrDefault();
        }

        /// <summary>
        /// 有効な先頭項目を取得
        /// </summary>
        public FolderItem? FirstFolderOrDefault()
        {
            return Items.FirstOrDefault(e => !e.IsEmpty());
        }

        /// <summary>
        /// 有効な末端項目を取得
        /// </summary>
        public FolderItem? LastFolderOrDefault()
        {
            return Items.LastOrDefault(e => !e.IsEmpty());
        }

        /// <summary>
        /// 親の場所を取得
        /// </summary>
        /// <returns>親の場所。存在しない場合は null を返す</returns>
        public virtual QueryPath? GetParentQuery()
        {
            if (Place == null)
            {
                return null;
            }

            if (Place.Scheme == QueryScheme.Root)
            {
                return null;
            }

            return Place.GetParent() ?? new QueryPath(QueryScheme.Root, null);
        }

        /// <summary>
        /// パスがリストに含まれるか判定
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Contains(QueryPath path)
        {
            return Items.Any(e => e.TargetPath.Equals(path));
        }

        /// <summary>
        /// 並び替え
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected List<FolderItem> Sort(IEnumerable<FolderItem> source, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            IOrderedEnumerable<FolderItem> orderSource;

            if (Config.Current.Bookshelf.IsOrderWithoutFileType)
            {
                // NOTE: 並び順を変えないOrderBy
                orderSource = source.OrderBy(e => 0);
            }
            else
            {
                orderSource = source.OrderBy(e => e.Type);
            }

            var order = FolderOrder switch
            {
                FolderOrder.FileName
                    => orderSource.ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.FileNameDescending
                    => orderSource.ThenByDescending(e => e, new ComparerFileName(token)),
                FolderOrder.Path
                    => orderSource.ThenBy(e => e, new ComparerFullPath(token)),
                FolderOrder.PathDescending
                    => orderSource.ThenByDescending(e => e, new ComparerFullPath(token)),
                FolderOrder.FileType
                    => orderSource.ThenBy(e => e, new ComparerFileType(token)),
                FolderOrder.FileTypeDescending
                    => orderSource.ThenByDescending(e => e, new ComparerFileType(token)),
                FolderOrder.TimeStamp
                    => orderSource.ThenBy(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.TimeStampDescending
                    => orderSource.ThenByDescending(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.EntryTime
                    => orderSource.ThenBy(e => e.EntryTime).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.EntryTimeDescending
                    => orderSource.ThenByDescending(e => e.EntryTime).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.Size
                    => orderSource.ThenBy(e => e.Length).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.SizeDescending
                    => orderSource.ThenByDescending(e => e.Length).ThenBy(e => e, new ComparerFileName(token)),
                FolderOrder.Random
                    => CreateRandomOrder(orderSource),
                _
                    => orderSource.ThenBy(e => e, new ComparerFileName(token)),
            };

            try
            {
                return order.ToList();
            }
            // NOTE: Linq.OrderByでのOperationCanceledException例外はInvalidOperationExceptionとして報告される
            catch (InvalidOperationException ex) when (ex.InnerException is OperationCanceledException opex)
            {
                throw opex;
            }
        }

        private IOrderedEnumerable<FolderItem> CreateRandomOrder(IOrderedEnumerable<FolderItem> orderSource)
        {
            var random = new Random(RandomSeed);
            return orderSource.ThenBy(e => random.Next());
        }

        /// <summary>
        /// ソート用：名前で比較(昇順)
        /// </summary>
        public class ComparerFileName : IComparer<FolderItem>
        {
            private readonly CancellationToken _token;

            public ComparerFileName(CancellationToken token)
            {
                _token = token;
            }

            public int Compare(FolderItem? x, FolderItem? y)
            {
                _token.ThrowIfCancellationRequested();

                return NaturalSort.Compare(x?.Name, y?.Name);
            }
        }


        /// <summary>
        /// ソート用：フルパスで比較(昇順)
        /// </summary>
        public class ComparerFullPath : IComparer<FolderItem>
        {
            private readonly CancellationToken _token;

            public ComparerFullPath(CancellationToken token)
            {
                _token = token;
            }

            public int Compare(FolderItem? x, FolderItem? y)
            {
                _token.ThrowIfCancellationRequested();

                return NaturalSort.Compare(x?.TargetPath.FullPath, y?.TargetPath.FullPath);
            }
        }

        /// <summary>
        /// ソート用：ファイルの種類で比較(昇順)
        /// </summary>
        public class ComparerFileType : IComparer<FolderItem>
        {
            private readonly CancellationToken _token;

            public ComparerFileType(CancellationToken token)
            {
                _token = token;
            }

            public int Compare(FolderItem? x, FolderItem? y)
            {
                _token.ThrowIfCancellationRequested();

                if (x is null) return y is null ? 0 : -1;
                if (y is null) return 1;

                // ディレクトリは種類判定なし
                if (x.IsDirectoryMaybe())
                {
                    return y.IsDirectoryMaybe() ? NaturalSort.Compare(x.Name, y.Name) : 1;
                }
                if (y.IsDirectoryMaybe())
                {
                    return x.IsDirectoryMaybe() ? NaturalSort.Compare(x.Name, y.Name) : -1;
                }

                var extX = LoosePath.GetExtension(x.Name);
                var extY = LoosePath.GetExtension(y.Name);
                if (extX != extY)
                {
                    return NaturalSort.Compare(extX, extY);
                }
                else
                {
                    return NaturalSort.Compare(x.Name, y.Name);
                }
            }
        }

        /// <summary>
        /// アイコンの表示更新
        /// </summary>
        /// <param name="path">指定パスの項目を更新。nullの場合全ての項目を更新</param>
        public void RefreshIcon(QueryPath? path)
        {
            if (Items == null) return;

            if (path == null || path.IsEmpty)
            {
                foreach (var item in Items)
                {
                    item.NotifyIconOverlayChanged();
                }
            }
            else
            {
                foreach (var item in Items.Where(e => e.EntityPath == path))
                {
                    item.NotifyIconOverlayChanged();
                }
            }
        }


        public virtual void RequestCreate(QueryPath path)
        {
            AddItem(path);
        }

        public virtual void RequestDelete(QueryPath path)
        {
            DeleteItem(path);
        }

        public virtual void RequestRename(QueryPath oldPath, QueryPath path)
        {
            RenameItem(oldPath, path);
        }


        private FolderItem? FindItem(QueryPath path)
        {
            lock (_lock)
            {
                return this.Items.FirstOrDefault(e => e.TargetPath == path);
            }
        }

        // HACK: 本来はFolderCollectionの種類に応じて挙動を変える必要がある。いまのところファイルシステム系以外で呼ばれないため実装していない。
        protected virtual FolderItem? CreateItem(QueryPath path)
        {
            return _folderItemFactory.CreateFolderItem(path);
        }


        public void AddItem(QueryPath path)
        {
            var item = FindItem(path);
            if (item != null) return;

            item = CreateItem(path);
            if (item == null) return;

            AppDispatcher.Invoke(() => AddItem(item));
        }

        protected void AddItem(FolderItem item)
        {
            if (item == null) return;

            lock (_lock)
            {
                if (this.Items.Count == 1 && this.Items.First().Type == FolderItemType.Empty)
                {
                    this.Items.RemoveAt(0);
                    this.Items.Add(item);
                }
                else if (FolderOrder == FolderOrder.Random)
                {
                    this.Items.Add(item);
                }
                else if (Config.Current.Bookshelf.IsInsertItem)
                {
                    // 別にリストを作ってソートを実行し、それで挿入位置を決める
                    var list = Sort(this.Items.Concat(new List<FolderItem>() { item }), CancellationToken.None);
                    var index = list.IndexOf(item);

                    if (index >= 0)
                    {
                        this.Items.Insert(index, item);
                    }
                    else
                    {
                        this.Items.Add(item);
                    }
                }
                else
                {
                    this.Items.Add(item);
                }
            }
        }


        public void DeleteItem(QueryPath path)
        {
            var item = FindItem(path);
            if (item == null) return;

            AppDispatcher.Invoke(() => DeleteItem(item));
        }

        protected void DeleteItem(FolderItem item)
        {
            if (item == null) return;

            CollectionChanging?.Invoke(this, new FolderCollectionChangedEventArgs(CollectionChangeAction.Remove, item));

            lock (_lock)
            {
                this.Items.Remove(item);

                if (this.Items.Count == 0)
                {
                    this.Items.Add(_folderItemFactory.CreateFolderItemEmpty());
                }
            }

            CollectionChanged?.Invoke(this, new FolderCollectionChangedEventArgs(CollectionChangeAction.Remove, item));
        }


        public void RenameItem(QueryPath oldPath, QueryPath path)
        {
            if (oldPath == path) return;

            var item = FindItem(oldPath);
            if (item == null)
            {
                // リストにない項目は追加を試みる
                AddItem(path);
                return;
            }

            AppDispatcher.Invoke(() => RenameItem(item, path));
        }

        public void RenameItem(FolderItem item, QueryPath path)
        {
            if (item == null) return;

            item.SetTargetPath(path);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Items != null)
                    {
                        BindingOperations.DisableCollectionSynchronization(Items);
                        Items = _itemsEmpty;
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        #region IEnumerable Support

        public IEnumerator<FolderItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Support
    }

}
