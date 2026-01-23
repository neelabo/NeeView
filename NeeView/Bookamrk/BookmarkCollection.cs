using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public partial class BookmarkCollection : BindableBase
    {
        static BookmarkCollection() => Current = new BookmarkCollection();
        public static BookmarkCollection Current { get; }

        private TreeListNode<IBookmarkEntry> _items;
        private Lock _lock = new();


        private BookmarkCollection()
        {
            _items = CreateEmptyTree();
        }


        [Subscribable]
        public event EventHandler<BookmarkCollectionChangedEventArgs>? BookmarkChanged;


        public TreeListNode<IBookmarkEntry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        private static TreeListNode<IBookmarkEntry> CreateEmptyTree()
        {
            return new TreeListNode<IBookmarkEntry>(new BookmarkFolder());
        }

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

            return Items.WalkChildren().Select(e => e.Value).OfType<Bookmark>().FirstOrDefault(e => e.Path == path);
        }

        public BookMementoUnit? FindUnit(string place)
        {
            if (place == null) return null;

            return Find(place)?.Unit;
        }

        public TreeListNode<IBookmarkEntry>? FindNode(IBookmarkEntry entry)
        {
            if (entry == null) return null;

            return Items.WalkChildren().FirstOrDefault(e => e.Value == entry);
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
                return Items.WalkChildren().FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == path.SimplePath);
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
            var child = node.FirstOrDefault(e => e.Value.Name == name);
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
                var index = memento.Index > memento.Parent.Count ? memento.Parent.Count : memento.Index;

                memento.Parent.Insert(index, memento.Node);
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, memento.Node.Parent, memento.Node));
            }
        }

        public bool Remove(TreeListNode<IBookmarkEntry>? node)
        {
            if (node == null) return false;
            if (node.Parent is null) return false;
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
        public async ValueTask RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            List<TreeListNode<IBookmarkEntry>> nodes;
            lock (_lock)
            {
                nodes = Items.WalkChildren().Where(e => e.Value is Bookmark).ToList();
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
                    var ignoreNames = target.Where(e => e.Value is BookmarkFolder).Select(e => e.Value.Name).WhereNotNull();
                    var validName = GetValidateFolderName(ignoreNames, name, TextResources.GetString("Word.NewFolder"));
                    var node = new TreeListNode<IBookmarkEntry>(new BookmarkFolder() { Name = validName });

                    target.Add(node);
                    target.IsExpanded = true;
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));

                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// 移動 (汎用)
        /// </summary>
        /// <remarks>
        /// 同階層の移動だけでなく、異なる階層の移動や新規挿入にも対応している。
        /// </remarks>
        /// <param name="parent">移動先階層</param>
        /// <param name="item">移動元項目</param>
        /// <param name="newIndex">移動位置</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Move(TreeListNode<IBookmarkEntry> parent, TreeListNode<IBookmarkEntry> item, int newIndex)
        {
            lock (_lock)
            {
                newIndex = Math.Clamp(newIndex, 0, parent.Count);

                // 親がいないときは挿入
                if (item.Parent is null)
                {
                    // 親がいないのは新しいエントリなので重複を除外する
                    var itemPath = (item.Value as Bookmark)?.Path;
                    if (itemPath is null)
                    {
                        return false;
                    }
                    var node = parent.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == itemPath);
                    if (node is not null)
                    {
                        return false;
                    }

                    // 新しい項目として挿入する
                    parent.Insert(newIndex, item);
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item) { NewIndex = item.GetIndex() });
                    return true;
                }

                if (parent == item || parent.ParentContains(item))
                {
                    // 親を子には移動できない
                    throw new InvalidOperationException("Can't move a parent to a child.");
                }

                var isChangeDirectory = item.Parent != parent;
                if (isChangeDirectory)
                {
                    var oldParent = item.Parent;
                    item.RemoveSelf();
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, oldParent, item));
                    parent.Insert(newIndex, item);
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item) { NewIndex = item.GetIndex() });
                    return true;
                }
                else
                {
                    var oldIndex = item.GetIndex();
                    Move(parent, oldIndex, newIndex);
                    return true;
                }
            }
        }

        private void Move(TreeListNode<IBookmarkEntry> parent, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;

            var item = parent[oldIndex];
            var target = parent[newIndex];
            parent.Move(oldIndex, newIndex);

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Move, item.Parent, item) { Target = target, OldIndex = oldIndex, NewIndex = newIndex });
        }

        public bool MoveToChild(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            lock (_lock)
            {
                if (target != Items && target.Value is not BookmarkFolder)
                {
                    return false;
                }
                if (item.Parent == target)
                {
                    return false;
                }

                if (item.Value is BookmarkFolder folder)
                {
                    if (target.ParentContains(item))
                    {
                        return false;
                    }

                    var conflict = target.FirstOrDefault(e => folder.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        return Merge(item, conflict);
                    }
                    else
                    {
                        return MoveToChildInner(item, target);
                    }
                }
                else if (item.Value is Bookmark bookmark)
                {
                    var conflict = target.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        return Remove(item);
                    }
                    else
                    {
                        return MoveToChildInner(item, target);
                    }
                }

                return false;
            }
        }

        private bool MoveToChildInner(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (item == target) return false;
            if (target.ParentContains(item)) return false; // TODO: 例外にすべき？

            var parent = item.Parent;
            item.RemoveSelf();
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));

            target.Insert(0, item);
            target.IsExpanded = true;
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));

            return true;
        }

        public bool Merge(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
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

                foreach (var child in item.ToList())
                {
                    child.RemoveSelf();
                    if (child.Value is BookmarkFolder folder)
                    {
                        var conflict = target.FirstOrDefault(e => folder.IsEqual(e.Value));
                        if (conflict != null)
                        {
                            Merge(child, conflict);
                            continue;
                        }
                    }
                    else if (child.Value is Bookmark bookmark)
                    {
                        var conflict = target.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                        if (conflict != null)
                        {
                            continue;
                        }
                    }

                    target.Add(child);
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, target, child));
                }

                return true;
            }
        }

        public void Rename(string src, string dst)
        {
            lock (_lock)
            {
                foreach (var item in Items.WalkChildren())
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
            name = BookmarkTools.GetValidateName(name);
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

            foreach (var child in node.Where(e => e.Value is BookmarkFolder).ToList())
            {
                ValidateFolderName(child);

                var folder = ((BookmarkFolder)child.Value);

                var name = BookmarkTools.GetValidateName(folder.Name);
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
            memento.Books = Items.WalkChildren().Select(e => e.Value).OfType<Bookmark>().Select(e => e.Unit.Memento).Distinct().ToList();

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
                var nodes = BookmarkNodeConverter.ConvertToTreeListNode(memento.Nodes) ?? CreateEmptyTree();
                this.Load(nodes, memento.Books);
            }

            // 互換用 : FileResolver 登録
            if (memento.Books is not null && memento.Format?.CompareTo(new FormatVersion(BookmarkCollection.Memento.FormatName, 45, 0, 3978)) < 0)
            {
                FileResolver.Current.AddRangeArchivePath(memento.Books.Select(e => e.Path));
            }
        }

        #endregion
    }


    public class BookmarkNode
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Path { get; set; }

        public DateTime EntryTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<BookmarkNode>? Children { get; set; }

        public bool IsFolder => Children != null;

        public IEnumerable<BookmarkNode> Walk()
        {
            yield return this;

            if (Children != null)
            {
                foreach(var child in Children)
                {
                    foreach(var subChild in child.Walk())
                    {
                        yield return subChild;
                    }
                }
            }
        }
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
                foreach (var child in source)
                {
                    node.Children.Add(ConvertFrom(child));
                }
            }
            else if (source.Value is Bookmark bookmark)
            {
                node.Name = bookmark.RawName;
                node.Path = bookmark.Path;
                node.EntryTime = bookmark.EntryTime;
            }
            else
            {
                throw new NotSupportedException();
            }

            return node;
        }

        public static TreeListNode<IBookmarkEntry>? ConvertToTreeListNode(BookmarkNode source)
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
                        var childNode = ConvertToTreeListNode(child);
                        if (childNode is not null)
                        {
                            node.Add(childNode);
                        }
                    }
                }
                return node;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(source.Path))
                {
                    return null;
                }
                var bookmark = new Bookmark(source.Path)
                {
                    Name = source.Name ?? "",
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
            where T : ITreeListNode
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
        /// Query生成
        /// </summary>
        public static QueryPath CreateQuery(this TreeListNode<IBookmarkEntry> node)
        {
            return node.CreateQuery(QueryScheme.Bookmark);
        }
    }
}
