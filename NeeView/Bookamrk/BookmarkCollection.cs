using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;
using NeeLaboratory.Generators;

namespace NeeView
{
    public partial class BookmarkCollection : BindableBase
    {
        static BookmarkCollection() => Current = new BookmarkCollection();
        public static BookmarkCollection Current { get; }

        private Lock _lock = new();

        // Constructors

        private BookmarkCollection()
        {
            _items = new TreeListNode<IBookmarkEntry>(new BookmarkFolder());
        }


        // Events
        [Subscribable]
        public event EventHandler<BookmarkCollectionChangedEventArgs>? BookmarkChanged;



        // Properties

        private TreeListNode<IBookmarkEntry> _items;
        public TreeListNode<IBookmarkEntry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        // Methods

        public void RaiseBookmarkChangedEvent(BookmarkCollectionChangedEventArgs e)
        {
            BookmarkChanged?.Invoke(this, e);
        }


        public void Load(TreeListNode<IBookmarkEntry> nodes, IEnumerable<BookMemento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;
            Items.Value = new BookmarkFolder();

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Reset));
        }


        public Bookmark? Find(string path)
        {
            if (path == null) return null;

            return Items.Select(e => e.Value).OfType<Bookmark>().FirstOrDefault(e => e.Path == path);
        }


        public BookMementoUnit? FindUnit(string place)
        {
            if (place == null) return null;

            return Find(place)?.Unit;
        }


        public TreeListNode<IBookmarkEntry>? FindNode(IBookmarkEntry entry)
        {
            if (entry == null) return null;

            return Items.FirstOrDefault(e => e.Value == entry);
        }


        public TreeListNode<IBookmarkEntry>? FindNode(string path)
        {
            if (path == null) return null;

            return FindNode(new QueryPath(path));
        }

        public TreeListNode<IBookmarkEntry>? FindNode(QueryPath path)
        {
            if (path is null)
            {
                return null;
            }

            if (path.Scheme == QueryScheme.Bookmark)
            {
                if (path.Path == null)
                {
                    return Items;
                }
                return FindNode(Items, path.Path.Split(LoosePath.Separators));
            }
            else if (path.Scheme == QueryScheme.File)
            {
                return Items.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == path.SimplePath);
            }
            else
            {
                return null;
            }
        }

        private TreeListNode<IBookmarkEntry>? FindNode(TreeListNode<IBookmarkEntry> node, IEnumerable<string> pathTokens)
        {
            if (pathTokens == null)
            {
                return null;
            }

            if (!pathTokens.Any())
            {
                return node;
            }

            var name = pathTokens.First();
            var child = node.Children.FirstOrDefault(e => e.Value.Name == name);
            if (child != null)
            {
                return FindNode(child, pathTokens.Skip(1));
            }

            return null;
        }


        public bool Contains(string place)
        {
            if (place == null) return false;

            lock (_lock)
            {
                return Find(place) != null;
            }
        }

        public bool Contains(TreeListNode<IBookmarkEntry> node)
        {
            return Items == node.Root;
        }

        public void AddFirst(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            lock (_lock)
            {
                node.UpdateEntryTime();
                Items.Root.Insert(0, node);
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
            }
        }

        // TODO: 重複チェックをここで行う
        public void AddToChild(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> parent)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            lock (_lock)
            {
                parent = parent ?? Items.Root;

                node.UpdateEntryTime();
                parent.Add(node);
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
            }
        }


        public void Restore(TreeListNodeMemento<IBookmarkEntry> memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            if (!Contains(memento.Parent))
            {
                return;
            }

            lock (_lock)
            {
                var index = memento.Index > memento.Parent.Children.Count ? memento.Parent.Children.Count : memento.Index;

                memento.Parent.Insert(index, memento.Node);
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, memento.Node.Parent, memento.Node));
            }
        }


        public bool Remove(TreeListNode<IBookmarkEntry>? node)
        {
            if (node == null) return false;
            if (node.Root != Items.Root) throw new InvalidOperationException();

            lock (_lock)
            {
                var parent = node.Parent;
                if (node.RemoveSelf())
                {
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, node));
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        // 無効な履歴削除
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            List<TreeListNode<IBookmarkEntry>> nodes;
            lock (_lock)
            {
                nodes = Items.Where(e => e.Value is Bookmark).ToList();
            }
            var unlinked = new List<TreeListNode<IBookmarkEntry>>();
            foreach (var node in nodes)
            {
                var bookmark = (Bookmark)node.Value;
                if (!await ArchiveEntryUtility.ExistsAsync(bookmark.Path, false, token))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            if (unlinked.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var node in unlinked)
                    {
                        var bookmark = (Bookmark)node.Value;
                        Debug.WriteLine($"BookmarkRemove: {bookmark.Path}");
                        node.RemoveSelf();
                    }

                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
                }
            }
        }


        public TreeListNode<IBookmarkEntry>? AddNewFolder(TreeListNode<IBookmarkEntry> target, string? name)
        {
            if (target == Items || target.Value is BookmarkFolder)
            {
                lock (_lock)
                {
                    var ignoreNames = target.Children.Where(e => e.Value is BookmarkFolder).Select(e => e.Value.Name).WhereNotNull();
                    var validName = GetValidateFolderName(ignoreNames, name, Properties.TextResources.GetString("Word.NewFolder"));
                    var node = new TreeListNode<IBookmarkEntry>(new BookmarkFolder() { Name = validName });

                    target.Add(node);
                    target.IsExpanded = true;
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));

                    return node;
                }
            }

            return null;
        }


        public void Move(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target, int direction)
        {
            lock (_lock)
            {
                if (item.Value is BookmarkFolder && item.Parent != target.Parent)
                {
                    var conflict = target.Parent?.Children.FirstOrDefault(e => e.Value is BookmarkFolder && e.Value.Name == item.Value.Name);
                    if (conflict != null)
                    {
                        Merge(item, conflict);
                        return;
                    }
                }

                MoveInner(item, target, direction);
            }
        }

        private void MoveInner(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target, int direction)
        {
            if (item == target) return;
            if (item.Parent is null || target.Parent is null) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            bool isChangeDirectory = item.Parent != target.Parent;

            var parent = item.Parent;
            var oldIndex = parent.Children.IndexOf(item);
            item.RemoveSelf();
            if (isChangeDirectory)
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
            }

            item.UpdateEntryTime();
            target.Parent.Insert(target, direction, item);
            if (isChangeDirectory)
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
            }
            else
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Move, item.Parent, item) { OldIndex = oldIndex });
            }
        }

        public void MoveToChild(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            lock (_lock)
            {
                if (target != Items && target.Value is not BookmarkFolder)
                {
                    return;
                }
                if (item.Parent == target)
                {
                    return;
                }

                if (item.Value is BookmarkFolder folder)
                {
                    if (target.ParentContains(item))
                    {
                        return;
                    }

                    var conflict = target.Children.FirstOrDefault(e => folder.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        Merge(item, conflict);
                    }
                    else
                    {
                        MoveToChildInner(item, target);
                    }
                }

                else if (item.Value is Bookmark bookmark)
                {
                    var conflict = target.Children.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        Remove(item);
                    }
                    else
                    {
                        MoveToChildInner(item, target);
                    }
                }
            }
        }

        private void MoveToChildInner(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            var parent = item.Parent;
            item.RemoveSelf();
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));

            item.UpdateEntryTime();
            target.Insert(0, item);
            target.IsExpanded = true;
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
        }


        public void Merge(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (item?.Value is not BookmarkFolder) throw new ArgumentException("item must be BookmarkFolder");
            if (target?.Value is not BookmarkFolder) throw new ArgumentException("target must be BookmarkFolder");

            lock (_lock)
            {
                var parent = item.Parent;
                if (item.RemoveSelf())
                {
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
                }

                foreach (var child in item.Children.ToList())
                {
                    child.RemoveSelf();
                    if (child.Value is BookmarkFolder folder)
                    {
                        var conflict = target.Children.FirstOrDefault(e => folder.IsEqual(e.Value));
                        if (conflict != null)
                        {
                            Merge(child, conflict);
                            continue;
                        }
                    }
                    else if (child.Value is Bookmark bookmark)
                    {
                        var conflict = target.Children.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                        if (conflict != null)
                        {
                            continue;
                        }
                    }

                    target.Add(child);
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, target, child));
                }
            }
        }

        public void Rename(string src, string dst)
        {
            lock (_lock)
            {
                foreach (var item in Items)
                {
                    if (item.Value is Bookmark bookmark && bookmark.Path == src)
                    {
                        bookmark.Path = dst;
                        BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, item.Parent, item));
                    }
                }
            }
        }


        private static string GetValidateFolderName(IEnumerable<string> names, string? name, string defaultName)
        {
            name = BookmarkFolder.GetValidateName(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = defaultName;
            }
            if (names.Contains(name))
            {
                int count = 1;
                string newName;
                do
                {
                    newName = $"{name} ({++count})";
                }
                while (names.Contains(newName));
                name = newName;
            }

            return name;
        }


        private void ValidateFolderName(TreeListNode<IBookmarkEntry> node)
        {
            var names = new List<string>();

            foreach (var child in node.Children.Where(e => e.Value is BookmarkFolder))
            {
                ValidateFolderName(child);

                var folder = ((BookmarkFolder)child.Value);

                var name = BookmarkFolder.GetValidateName(folder.Name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "_";
                }
                if (names.Contains(name))
                {
                    int count = 1;
                    string newName = name;
                    do
                    {
                        newName = $"{name} ({++count})";
                    }
                    while (names.Contains(newName));
                    name = newName;
                }
                names.Add(name);

                folder.Name = name;
            }
        }

        /// <summary>
        /// 情報更新
        /// </summary>
        /// <param name="memento">新しい情報</param>
        /// <param name="isForce">変更がなくても更新する</param>
        public void Update(BookMemento memento, bool isForce)
        {
            lock (_lock)
            {
                var node = FindNode(memento.Path);
                if (node is null) return;
                if (node.Value is not Bookmark bookmark) return;
                if (!isForce && bookmark.Unit.Memento.IsEquals(memento)) return;

                bookmark.Unit.Memento = memento;
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Update, node.Parent, node));
            }
        }

        #region Memento

        [Memento]
        public class Memento
        {
            public static string FormatName => Environment.SolutionName + ".Bookmark";

            public FormatVersion? Format { get; set; }

            public BookmarkNode? Nodes { get; set; }

            public List<BookMemento>? Books { get; set; }

            public QuickAccessCollection.Memento? QuickAccess { get; set; }


            public Memento()
            {
                Nodes = new BookmarkNode();
                Books = new List<BookMemento>();
            }


            public void Save(string path)
            {
                Format = new FormatVersion(FormatName);

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
        }


        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Nodes = BookmarkNodeConverter.ConvertFrom(Items);
            memento.Books = Items.Select(e => e.Value).OfType<Bookmark>().Select(e => e.Unit.Memento).Distinct().ToList();

            // QuickAccess情報もここに保存する
            memento.QuickAccess = QuickAccessCollection.Current.CreateMemento();

            return memento;
        }

        // memento適用
        public void Restore(Memento? memento)
        {
            if (memento is null) return;

            QuickAccessCollection.Current.Restore(memento.QuickAccess);
            if (memento.Nodes is not null && memento.Books is not null)
            {
                this.Load(BookmarkNodeConverter.ConvertToTreeListNode(memento.Nodes), memento.Books);
            }
        }

        #endregion
    }


    public class BookmarkNode
    {
        public string? Name { get; set; }

        public string? Path { get; set; }

        public DateTime EntryTime { get; set; }

        public List<BookmarkNode>? Children { get; set; }

        public bool IsFolder => Children != null;
    }

    public static class BookmarkNodeConverter
    {
        public static BookmarkNode ConvertFrom(TreeListNode<IBookmarkEntry> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var node = new BookmarkNode();

            if (source.Value is BookmarkFolder folder)
            {
                node.Name = folder.Name;
                node.Children = new List<BookmarkNode>();
                foreach (var child in source.Children)
                {
                    node.Children.Add(ConvertFrom(child));
                }
            }
            else if (source.Value is Bookmark bookmark)
            {
                node.Path = bookmark.Path;
                node.EntryTime = bookmark.EntryTime;
            }
            else
            {
                throw new NotSupportedException();
            }

            return node;
        }

        public static TreeListNode<IBookmarkEntry> ConvertToTreeListNode(BookmarkNode source)
        {
            if (source.IsFolder)
            {
                var bookmarkFolder = new BookmarkFolder()
                {
                    Name = source.Name,
                };
                var node = new TreeListNode<IBookmarkEntry>(bookmarkFolder);
                if (source.Children is not null)
                {
                    foreach (var child in source.Children)
                    {
                        node.Add(ConvertToTreeListNode(child));
                    }
                }
                return node;
            }
            else
            {
                if (source.Path is null) throw new InvalidOperationException();
                var bookmark = new Bookmark(source.Path)
                {
                    EntryTime = source.EntryTime
                };
                var node = new TreeListNode<IBookmarkEntry>(bookmark);
                return node;
            }
        }
    }

    public static class TreeListNodeExtensions
    {
        public static QueryPath CreateQuery<T>(this TreeListNode<T> node, QueryScheme scheme)
            where T : IHasName
        {
            var path = string.Join("\\", node.Hierarchy.Select(e => e.Value).Skip(1).OfType<T>().Select(e => e.Name));
            return new QueryPath(scheme, path, null);
        }

        /// <summary>
        /// Bookmark用パス等価判定
        /// </summary>
        public static bool IsEqual(this TreeListNode<IBookmarkEntry> node, QueryPath path)
        {
            if (node is null || path is null)
            {
                return false;
            }

            if (path.Scheme == QueryScheme.Bookmark)
            {
                return node.CreateQuery(QueryScheme.Bookmark) == path;
            }
            else if (path.Scheme == QueryScheme.File)
            {
                if (node.Value is Bookmark bookmark)
                {
                    return bookmark.Path == path.SimplePath;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// TreeListNode&lt;IBookmarkEntry&rt; 拡張関数
    /// </summary>
    public static class BookmarkTreeListNodeExtensions
    {
        /// <summary>
        /// 登録日を現在日時で更新
        /// </summary>
        public static void UpdateEntryTime(this TreeListNode<IBookmarkEntry> node)
        {
            if (node.Value is Bookmark bookmark)
            {
                bookmark.EntryTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Query生成
        /// </summary>
        public static QueryPath CreateQuery(this TreeListNode<IBookmarkEntry> node)
        {
            return node.CreateQuery(QueryScheme.Bookmark);
        }
    }
}
