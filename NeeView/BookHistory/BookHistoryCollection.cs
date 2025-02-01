using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NeeView
{
    public partial class BookHistoryCollection : BindableBase
    {
        static BookHistoryCollection() => Current = new BookHistoryCollection();
        public static BookHistoryCollection Current { get; }


        private Dictionary<string, FolderParameter.Memento> _folders = new();
        private readonly System.Threading.Lock _lock = new();


        private BookHistoryCollection()
        {
            HistoryChanged += BookHistoryCollection_HistoryChanged;

            BookshelfSearchHistory.CollectionChanged += SearchHistoryChanged;
            BookmarkSearchHistory.CollectionChanged += SearchHistoryChanged;
            BookHistorySearchHistory.CollectionChanged += SearchHistoryChanged;
            PageListSearchHistory.CollectionChanged += SearchHistoryChanged;
        }


        [Subscribable]
        public event EventHandler<BookMementoCollectionChangedArgs>? HistoryChanged;

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? SearchChanged;


        // 履歴コレクションロック
        public System.Threading.Lock ItemsLock => _lock;

        // 履歴コレクション
        public LinkedDicionary<string, BookHistory> Items { get; set; } = new LinkedDicionary<string, BookHistory>();

        // 要素数
        public int Count => Items.Count;

        // 更新番号
        public int SerialNumber { get; private set; }

        // 先頭の要素
        public LinkedListNode<BookHistory>? First => Items.First;

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
                Items.Clear();
                BookMementoCollection.Current.CleanUp();
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }

        public void Load(IEnumerable<BookHistory> items, IEnumerable<BookMemento> books)
        {
            lock (_lock)
            {
                Items.Clear();
                BookMementoCollection.Current.CleanUp();

                foreach (var book in books)
                {
                    BookMementoCollection.Current.Set(book);
                }

                foreach (var item in items)
                {
                    try
                    {
                        var newItem = new BookHistory(item.Path, item.LastAccessTime);
                        Items.AddLastRaw(newItem.Path, newItem);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }


        public bool Contains(string place)
        {
            if (place == null) return false;

            lock (_lock)
            {
                return Items.ContainsKey(place);
            }
        }


        // 履歴検索
        public LinkedListNode<BookHistory>? FindNode(string place)
        {
            if (place == null) return null;

            lock (_lock)
            {
                return Items.Find(place);
            }
        }

        public BookHistory? Find(string place)
        {
            return FindNode(place)?.Value;
        }

        public BookMementoUnit? FindUnit(string place)
        {
            return Find(place)?.Unit;
        }

        // 履歴追加
        public void Add(BookMemento memento, bool isKeepOrder)
        {
            if (memento == null) return;

            var changeType = BookMementoCollectionChangedType.None;

            try
            {
                lock (_lock)
                {
                    var node = FindNode(memento.Path);
                    if (node != null && isKeepOrder)
                    {
                        node.Value.Unit.Memento = memento;
                        changeType = BookMementoCollectionChangedType.Update;
                    }
                    else
                    {
                        node = node ?? new LinkedListNode<BookHistory>(new BookHistory(BookMementoCollection.Current.Set(memento), DateTime.Now));
                        node.Value.Unit.Memento = memento;
                        node.Value.LastAccessTime = DateTime.Now;

                        if (node == Items.First)
                        {
                            changeType = BookMementoCollectionChangedType.Update;
                        }
                        else
                        {
                            Items.AddFirst(node.Value.Path, node.Value);
                            changeType = BookMementoCollectionChangedType.Add;
                        }
                    }
                }

                if (changeType != BookMementoCollectionChangedType.None)
                {
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(changeType, memento.Path));
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
            bool isRemoved = false;

            lock (_lock)
            {
                var node = FindNode(place);
                if (node != null)
                {
                    Items.Remove(place);
                    isRemoved = true;
                }
            }

            if (isRemoved)
            {
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, place));
            }
        }

        // まとめて履歴削除
        public void Remove(IEnumerable<string> places)
        {
            if (places == null) return;

            bool isRemoved = false;

            lock (_lock)
            {
                var unlinked = places.Where(e => FindNode(e) != null);

                if (unlinked.Any())
                {
                    foreach (var place in unlinked)
                    {
                        Debug.WriteLine($"HistoryRemove: {place}");
                        Items.Remove(place);
                        isRemoved = true;
                    }
                }
            }

            if (isRemoved)
            {
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
            }
        }

        // 無効な履歴削除
        public async Task<int> RemoveUnlinkedAsync(CancellationToken token)
        {
            Debug.WriteLine($"BookHistory: RemoveUnlinked...");

            List<BookHistory> items;
            lock (_lock)
            {
                items = this.Items.ToList();
            }

            var unlinked = new List<BookHistory>();
            foreach (var item in items)
            {
                if (!await ArchiveEntryUtility.ExistsAsync(item.Path, token))
                {
                    unlinked.Add(item);
                }
            }

            if (unlinked.Any())
            {
                lock (_lock)
                {
                    foreach (var item in unlinked)
                    {
                        Debug.WriteLine($"HistoryRemove: {item.Path}");
                        Items.Remove(item.Path);
                    }
                }

                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
            }

            Debug.WriteLine($"BookHistory: RemoveUnlinked done.");
            return unlinked.Count;
        }

        public void ShowRemovedMessage(int removedCount)
        {
            ToastService.Current.Show(new Toast(Properties.TextResources.GetFormatString("History.DeleteItemsMessage", removedCount)));
        }


        // 最近使った履歴のリストアップ
        public List<BookHistory> ListUp(int size)
        {
            lock (_lock)
            {
                return Items.Take(size).ToList();
            }
        }

        /// <summary>
        /// 範囲指定して履歴をリストアップ
        /// </summary>
        /// <param name="current">基準位置</param>
        /// <param name="direction">方向</param>
        /// <param name="size">取得サイズ</param>
        /// <returns></returns>
        internal List<BookHistory> ListUp(string current, int direction, int size)
        {
            lock (_lock)
            {
                var list = new List<BookHistory>();

                var now = FindNode(current);
                var node = now ?? Items.First;

                if (now == null && node != null && direction < 0)
                {
                    list.Add(node.Value);
                }

                for (int i = 0; i < size; i++)
                {
                    node = direction < 0 ? node?.Next : node?.Previous; // リストと履歴の方向は逆

                    if (node == null) break;
                    list.Add(node.Value);
                }

                return list;
            }
        }


        public void Rename(string src, string dst)
        {
            bool isRenamed = false;

            lock (_lock)
            {
                var item = Items.Find(src);
                if (item != null)
                {
                    Items.Remove(dst);
                    Items.Remap(src, dst);
                    item.Value.Path = dst;
                    isRenamed = true;
                }
            }

            if (isRenamed)
            {
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, dst));
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

                Debug.WriteLine("HistoryMerge...");

                if (Format != memento.Format)
                {
                    Debug.WriteLine("HistoryMerge failed: Illegal format");
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
                            Debug.WriteLine($"HistoryMerge: Update: {item.Path}");
                            itemMap[item.Path] = item;
                            bookMap[item.Path] = importBookMap[item.Path];
                            isDirty = true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"HistoryMerge: Add: {item.Path}");
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

            memento.Items = Limit(this.Items.Where(e => !e.Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)), Config.Current.History.LimitSize, Config.Current.History.LimitSpan).ToList();
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
