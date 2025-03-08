//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using NeeView.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [LocalDebug]
    public partial class BookHistoryCollection : BindableBase
    {
        static BookHistoryCollection() => Current = new BookHistoryCollection();
        public static BookHistoryCollection Current { get; }


        private Dictionary<string, FolderParameter.Memento> _folders = new();
        private readonly Lock _lock = new();

        // 履歴は内部的には日時昇順の ObservableCollection として保持する。
        // 末端のほうが更新頻度が高いので入れ替えに有利なため。
        private readonly ObservableCollectionEx<BookHistory> _items;
        private readonly ObservableCollectionMap<BookHistory> _itemsMap;

        private BookHistoryCollection()
        {
            HistoryChanged += BookHistoryCollection_HistoryChanged;

            BookshelfSearchHistory.CollectionChanged += SearchHistoryChanged;
            BookmarkSearchHistory.CollectionChanged += SearchHistoryChanged;
            BookHistorySearchHistory.CollectionChanged += SearchHistoryChanged;
            PageListSearchHistory.CollectionChanged += SearchHistoryChanged;

            _items = new();
            _itemsMap = new(_items);
        }


        [Subscribable]
        public event EventHandler<BookMementoCollectionChangedArgs>? HistoryChanged;

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? SearchChanged;


        // 履歴コレクションロック
        public Lock ItemsLock => _lock;

        // 履歴コレクション
        public ObservableCollectionEx<BookHistory> Items => _items;

        // 要素数
        public int Count => _items.Count;

        // 更新番号
        public int SerialNumber { get; private set; }

        // 本棚 検索履歴
        public HistoryStringCollection BookshelfSearchHistory { get; } = new();

        // ブックマーク 検索履歴
        public HistoryStringCollection BookmarkSearchHistory { get; } = new();

        // 履歴 検索履歴
        public HistoryStringCollection BookHistorySearchHistory { get; } = new();

        // ページリスト 検索履歴
        public HistoryStringCollection PageListSearchHistory { get; } = new();


        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            SerialNumber++;
            RaisePropertyChanged(nameof(Count));
        }

        private void SearchHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SearchChanged?.Invoke(sender, e);
        }

        // 履歴クリア
        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                BookMementoCollection.Current.CleanUp();
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Reset));
        }

        public void Load(IEnumerable<BookHistory> items, IEnumerable<BookMemento> books)
        {
            lock (_lock)
            {
                _items.Clear();
                BookMementoCollection.Current.CleanUp();

                foreach (var book in books)
                {
                    BookMementoCollection.Current.Set(book);
                }

                try
                {
                    // 日時昇順にする。ソート済のはずなので反転のみ行う。
                    if (items.Any() && items.First().LastAccessTime > items.Last().LastAccessTime)
                    {
                        items = items.Reverse();
                    }
                    _items.Reset(items.Select(e => new BookHistory(e.Path, e.LastAccessTime)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load));
        }

        public bool Contains(string place)
        {
            if (place == null) return false;

            lock (_lock)
            {
                return _itemsMap.ContainsKey(place);
            }
        }

        public BookMementoUnit? FindUnit(string place)
        {
            if (place == null) return null;

            lock (_lock)
            {
                return _itemsMap.Find(place)?.Unit;
            }
        }

        // 履歴追加
        public void Add(BookMemento memento, bool isKeepOrder)
        {
            if (memento == null) return;

            var changeType = BookMementoCollectionChangedType.None;

            try
            {
                BookHistory? item;
                lock (_lock)
                {
                    item = _itemsMap.Find(memento.Path);
                    if (item != null)
                    {
                        item.Unit.Memento = memento;
                        if (!isKeepOrder)
                        {
                            item.LastAccessTime = DateTime.Now;
                            //HistoryChanged?.Invoke(this, BookMementoCollectionChangedArgs.Create(BookMementoCollectionChangedType.UpdateLastAccessTime, [item]));
                            MoveCore(_items.Count - 1, item);
                            changeType = BookMementoCollectionChangedType.Replace;
                        }
                    }
                    else
                    {
                        item = new BookHistory(BookMementoCollection.Current.Set(memento), DateTime.Now);
                        item.Unit.Memento = memento;
                        item.LastAccessTime = DateTime.Now;
                        //InsertCore(0, item);
                        AddCore(item);
                        changeType = BookMementoCollectionChangedType.Add;
                    }
                }

                if (changeType != BookMementoCollectionChangedType.None)
                {
                    HistoryChanged?.Invoke(this, BookMementoCollectionChangedArgs.Create(changeType, [item]));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        // 履歴削除
        public void Remove(string place)
        {
            var item = RemoveCore(place);

            if (item is not null)
            {
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null, [item]));
            }
        }

        // まとめて履歴削除
        public void Remove(IEnumerable<string> places)
        {
            if (places == null) return;

            var unlinked = RemoveCore(places);

            if (unlinked.Count != 0)
            {
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null, unlinked));
            }
        }

        // 無効な履歴削除
        public async Task<int> RemoveUnlinkedAsync(CancellationToken token)
        {
            LocalDebug.WriteLine($"RemoveUnlinked...");

            var unlinked = new List<BookHistory>();
            foreach (var item in this.ToList())
            {
                if (!await ArchiveEntryUtility.ExistsAsync(item.Path, token))
                {
                    unlinked.Add(item);
                }
            }

            if (unlinked.Count != 0)
            {
                RemoveCore(unlinked.Select(e => e.Path));
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null, unlinked));
            }

            LocalDebug.WriteLine($"RemoveUnlinked done.");
            return unlinked.Count;
        }

        public void ShowRemovedMessage(int removedCount)
        {
            ToastService.Current.Show(new Toast(Properties.TextResources.GetFormatString("History.DeleteItemsMessage", removedCount)));
        }

        public List<BookHistory> ToList()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        // 最近使った履歴のリストアップ
        public List<BookHistory> ListUp(int size)
        {
            lock (_lock)
            {
                return _items.Take(size).ToList();
            }
        }

        public void Rename(string src, string dst)
        {
            BookHistory? node;
            lock (_lock)
            {
                node = _itemsMap.Find(src);
                if (node != null)
                {
                    var dstNode = _itemsMap.Find(dst);
                    if (dstNode is not null)
                    {
                        _items.Remove(dstNode);
                    }
                    node.Path = dst;
                    _itemsMap.Remap(src, node);
                }
            }

            if (node is not null)
            {
                List<BookHistory> list = [node];
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Replace, list, list));
            }
        }

        private void AddCore(BookHistory item)
        {
            lock (_lock)
            {
                Debug.Assert(!_itemsMap.ContainsKey(item.Key));
                _items.Add(item);
            }
        }

        private void InsertCore(int index, BookHistory item)
        {
            lock (_lock)
            {
                Debug.Assert(!_itemsMap.ContainsKey(item.Key));
                _items.Insert(index, item);
            }
        }

        private bool MoveCore(int index, BookHistory item)
        {
            lock (_lock)
            {
                var oldIndex = _items.LastIndexOf(item);
                if (oldIndex < 0) throw new ArgumentException("Cannot found item.");
                if (oldIndex == index) return false;
                _items.Move(oldIndex, index);
                return true;
            }
        }

        private BookHistory? RemoveCore(string place)
        {
            lock (_lock)
            {
                var item = _itemsMap.Find(place);
                if (item != null)
                {
                    _items.Remove(item);
                }
                return item;
            }
        }

        private List<BookHistory> RemoveCore(IEnumerable<string> places)
        {
            lock (_lock)
            {
                var unlinked = places.Select(e => _itemsMap.Find(e)).WhereNotNull().ToList();
                if (unlinked.Count != 0)
                {
                    foreach (var item in unlinked)
                    {
                        LocalDebug.WriteLine($"Remove {item.Path}");
                        _items.Remove(item);
                    }
                }
                return unlinked;
            }
        }


        #region for Folders


        // フォルダー設定
        public void SetFolderMemento(string path, FolderParameter.Memento memento)
        {
            path = path ?? "<<root>>";

            // 標準設定は記憶しない
            if (memento.IsDefault(path))
            {
                _folders.Remove(path);
            }
            else
            {
                _folders[path] = memento;
            }
        }

        // フォルダー設定取得
        public FolderParameter.Memento GetFolderMemento(string path)
        {
            path = path ?? "<<root>>";

            _folders.TryGetValue(path, out FolderParameter.Memento? memento);
            return memento ?? FolderParameter.Memento.GetDefault(path);
        }

        #endregion for Folders

        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [Memento]
        public class Memento : BindableBase
        {
            public static string FormatName => Environment.SolutionName + ".History";

            public FormatVersion? Format { get; set; }

            public List<BookHistory> Items { get; set; }

            public List<BookMemento> Books { get; set; }

            public Dictionary<string, FolderParameter.Memento>? Folders { get; set; }

            public List<string>? BookshelfSearchHistory { get; set; }
            public List<string>? BookmarkSearchHistory { get; set; }
            public List<string>? BookHistorySearchHistory { get; set; }
            public List<string>? PageListSearchHistory { get; set; }

            #region Obsolete
            [Obsolete(), Alternative(nameof(BookshelfSearchHistory), 40)] // ver.40
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public List<string>? SearchHistory { get; set; }
            #endregion Obsolete

            public Memento()
            {
                Format = new FormatVersion(FormatName);
                Items = new List<BookHistory>();
                Books = new List<BookMemento>();
            }


            public void Save(string path)
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(this, UserSettingTools.GetSerializerOptions());
                File.WriteAllBytes(path, json);
            }

            public static Memento Load(string path)
            {
                using var stream = File.OpenRead(path);
                return Load(stream);
            }

            public static Memento Load(Stream stream)
            {
                var memento = JsonSerializer.Deserialize<Memento>(stream, UserSettingTools.GetSerializerOptions());
                if (memento is null) throw new FormatException();
                return memento.Validate();
            }

            // 合成
            public void Merge(Memento? memento)
            {
                if (memento == null) return;

                LocalDebug.WriteLine("HistoryMerge...");

                if (Format != memento.Format)
                {
                    LocalDebug.WriteLine("HistoryMerge failed: Illegal format");
                    return;
                }

                bool isDirty = false;
                var itemMap = Items.ToDictionary(e => e.Path, e => e);
                var bookMap = Books.ToDictionary(e => e.Path, e => e);
                var importBookMap = memento.Books.ToDictionary(e => e.Path, e => e);

                foreach (var item in memento.Items)
                {
                    if (itemMap.ContainsKey(item.Path))
                    {
                        if (itemMap[item.Path].LastAccessTime < item.LastAccessTime)
                        {
                            LocalDebug.WriteLine($"HistoryMerge: Update: {item.Path}");
                            itemMap[item.Path] = item;
                            bookMap[item.Path] = importBookMap[item.Path];
                            isDirty = true;
                        }
                    }
                    else
                    {
                        LocalDebug.WriteLine($"HistoryMerge: Add: {item.Path}");
                        itemMap.Add(item.Path, item);
                        bookMap.Add(item.Path, importBookMap[item.Path]);
                        isDirty = true;
                    }
                }

                if (isDirty)
                {
                    Items = Limit(itemMap.Values.OrderByDescending(e => e.LastAccessTime), Config.Current.History.LimitSize, Config.Current.History.LimitSpan).ToList();
                    Books = bookMap.Values.ToList();
                }
            }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            // NOTE: 保存時は日時降順にする
            memento.Items = Limit(this.Items.Reverse().Where(e => !e.Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)), Config.Current.History.LimitSize, Config.Current.History.LimitSpan).ToList();
            memento.Books = memento.Items.Select(e => e.Unit.Memento).ToList();

            if (Config.Current.History.IsKeepFolderStatus)
            {
                memento.Folders = _folders;
            }

            if (Config.Current.History.IsKeepSearchHistory)
            {
                memento.BookshelfSearchHistory = this.BookshelfSearchHistory.Any() ? this.BookshelfSearchHistory.ToList() : null;
                memento.BookmarkSearchHistory = this.BookmarkSearchHistory.Any() ? this.BookmarkSearchHistory.ToList() : null;
                memento.BookHistorySearchHistory = this.BookHistorySearchHistory.Any() ? this.BookHistorySearchHistory.ToList() : null;
                memento.PageListSearchHistory = this.PageListSearchHistory.Any() ? this.PageListSearchHistory.ToList() : null;
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento? memento, bool fromLoad)
        {
            if (memento == null) return;

            _folders = memento.Folders ?? _folders;

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            this.BookshelfSearchHistory.Replace(memento.BookshelfSearchHistory ?? memento.SearchHistory);
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
            this.BookmarkSearchHistory.Replace(memento.BookmarkSearchHistory);
            this.BookHistorySearchHistory.Replace(memento.BookHistorySearchHistory);
            this.PageListSearchHistory.Replace(memento.PageListSearchHistory);

            this.Load(fromLoad ? Limit(memento.Items, Config.Current.History.LimitSize, Config.Current.History.LimitSpan) : memento.Items, memento.Books);
        }

        // 履歴数制限
        public static IEnumerable<BookHistory> Limit(IEnumerable<BookHistory> source, int limitSize, TimeSpan limitSpan)
        {
            // limit size
            var collection = limitSize == -1 ? source : source.Take(limitSize);

            // limit time
            var limitTime = DateTime.Now - limitSpan;
            collection = limitSpan == default ? collection : collection.TakeWhile(e => e.LastAccessTime > limitTime);

            return collection;
        }

        #endregion
    }
}
