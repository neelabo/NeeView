using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// TODO: パネルからのUI操作とスクリプトからの操作の２系統がごちゃまぜになっているので整備する

namespace NeeView
{
    public class RootFolderTree : FolderTreeNodeBase
    {
        public override string Name { get => ""; set { } }
        public override string DisplayName { get => "@Bookshelf"; set { } }

        public override IImageSourceCollection? Icon => null;
    }

    [Flags]
    public enum FolderTreeCategory
    {
        QuickAccess = 0x01,
        Directory = 0x02,
        BookmarkFolder = 0x04,

        All = QuickAccess | Directory | BookmarkFolder
    }

    public class FolderTreeModel : BindableBase
    {
        // Fields

        private readonly FolderList _folderList;
        private readonly RootFolderTree _root;
        private readonly RootQuickAccessNode? _rootQuickAccess;
        private readonly RootDirectoryNode? _rootDirectory;
        private readonly RootBookmarkFolderNode? _rootBookmarkFolder;

        // Constructors

        public FolderTreeModel(FolderList folderList, FolderTreeCategory categories)
        {
            _folderList = folderList;
            _root = new RootFolderTree();

            _root.Children = new ObservableCollection<FolderTreeNodeBase>();

            if ((categories & FolderTreeCategory.QuickAccess) != 0)
            {
                _rootQuickAccess = new RootQuickAccessNode();
                _rootQuickAccess.Initialize(_root);
                _root.Children.Add(_rootQuickAccess);
            }

            if ((categories & FolderTreeCategory.Directory) != 0)
            {
                _rootDirectory = new RootDirectoryNode(_root);
                _root.Children.Add(_rootDirectory);
            }

            if ((categories & FolderTreeCategory.BookmarkFolder) != 0)
            {
                _rootBookmarkFolder = new RootBookmarkFolderNode(_root);
                _root.Children.Add(_rootBookmarkFolder);
            }
        }


        // Events

        public event EventHandler? SelectedItemChanged;


        // Properties

        public RootFolderTree Root => _root;
        public RootQuickAccessNode? RootQuickAccess => _rootQuickAccess;
        public RootDirectoryNode? RootDirectory => _rootDirectory;
        public RootBookmarkFolderNode? RootBookmarkFolder => _rootBookmarkFolder;

        private FolderTreeNodeBase? _selectedItem;
        public FolderTreeNodeBase? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value is not null && !value.ContainsRoot(_root))
                {
                    return;
                }
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    _selectedItem.IsSelected = true;
                }
            }
        }

        public bool IsFocusAtOnce { get; set; }


        // Methods

        public void SetSelectedItem(FolderTreeNodeBase? node)
        {
            SelectedItem = node;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public void FocusAtOnce()
        {
            IsFocusAtOnce = true;
        }

        private static IEnumerable<FolderTreeNodeBase> GetNodeWalker(IEnumerable<FolderTreeNodeBase>? collection)
        {
            if (collection == null)
            {
                yield break;
            }

            foreach (var item in collection)
            {
                yield return item;

                foreach (var child in GetNodeWalker(item.Children))
                {
                    yield return child;
                }

                switch (item)
                {
                    case FolderTreeNodeDelayBase node:
                        if (node.ChildrenRaw != null)
                        {
                            foreach (var child in GetNodeWalker(node.ChildrenRaw))
                            {
                                yield return child;
                            }
                        }
                        break;

                    default:
                        foreach (var child in GetNodeWalker(item.Children))
                        {
                            yield return child;
                        }
                        break;
                }
            }
        }

        //private void Config_DpiChanged(object sender, EventArgs e)
        //{
        //    RaisePropertyChanged(nameof(FolderIcon));
        //
        //    foreach (var item in GetNodeWalker(_root.Children))
        //    {
        //        item.RefreshIcon();
        //    }
        //}

        public void ExpandRoot()
        {
            if (_root.Children is null) return;

            foreach (var node in _root.Children)
            {
                node.IsExpanded = true;
            }
        }

        public void SelectRootQuickAccess()
        {
            SelectedItem = _rootQuickAccess;
        }

        public void SelectRootBookmarkFolder()
        {
            SelectedItem = _rootBookmarkFolder;
        }

        public void Decide(object item)
        {
            switch (item)
            {
                case QuickAccessNode quickAccess:
                    SetFolderListPlace(quickAccess.QuickAccessSource.Path);
                    break;

                case RootDirectoryNode:
                    SetFolderListPlace("");
                    break;

                case DriveDirectoryNode drive:
                    if (drive.IsReady)
                    {
                        SetFolderListPlace(drive.Path);
                    }
                    break;

                case DirectoryNode folder:
                    SetFolderListPlace(folder.Path);
                    break;

                case BookmarkFolderNode bookmarkFolder:
                    SetFolderListPlace(bookmarkFolder.Path);
                    break;
            }
        }

        private void SetFolderListPlace(string path)
        {
            // TODO: リクエストの重複がありうる。キャンセル処理が必要?
            _folderList.RequestPlace(new QueryPath(path), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword);
        }

        /// <summary>
        /// 新しいノードの作成と追加 (スクリプト用)
        /// </summary>
        /// <param name="parent">親ノード</param>
        /// <returns>新しいノード。作れなかったら null</returns>
        public FolderTreeNodeBase? NewNode(FolderTreeNodeBase parent)
        {
            if (parent is null) return null;

            switch (parent)
            {
                case RootQuickAccessNode n:
                    return NewQuickAccess(n);
                case BookmarkFolderNode n:
                    return NewBookmarkFolder(n);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 新しいノードの作成と挿入 (スクリプト用)
        /// </summary>
        /// <param name="parent">親ノード</param>
        /// <param name="index">挿入位置</param>
        /// <returns>新しいノード。作れなかったら null</returns>
        public FolderTreeNodeBase? NewNode(FolderTreeNodeBase parent, int index)
        {
            if (parent is null) return null;

            switch (parent)
            {
                case RootQuickAccessNode n:
                    return NewQuickAccess(n, index);
                case BookmarkFolderNode n:
                    return NewBookmarkFolder(n); // ブックマークフォルダ―は挿入できない
                default:
                    return null;
            }
        }

        /// <summary>
        /// 新しいクイックアクセスの作成と追加 (スクリプト用)
        /// </summary>
        private QuickAccessNode? NewQuickAccess(RootQuickAccessNode parent)
        {
            return NewQuickAccess(parent, 0);
        }

        /// <summary>
        /// 新しいクイックアクセスの作成と挿入 (スクリプト用)
        /// </summary>
        private QuickAccessNode? NewQuickAccess(RootQuickAccessNode parent, int index)
        {
            if (_rootQuickAccess != parent) throw new ArgumentException("Not root node", nameof(parent));

            parent.IsExpanded = true;

            var quickAccess = new QuickAccess(_folderList.GetCurrentQueryPath());
            QuickAccessCollection.Current.Insert(index, quickAccess);

            var newItem = parent.Children.Cast<QuickAccessNode>().FirstOrDefault(e => e.QuickAccessSource == quickAccess);
            if (newItem is not null)
            {
                SelectedItem = newItem;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
            return newItem;
        }

        public void AddQuickAccess(object item)
        {
            switch (item)
            {
                case RootQuickAccessNode:
                    AddQuickAccess(_folderList.GetCurrentQueryPath());
                    break;

                case DirectoryNode folder:
                    AddQuickAccess(folder.Path);
                    break;

                case string filename:
                    AddQuickAccess(filename);
                    break;
            }
        }

        public void AddQuickAccess(string? path)
        {
            InsertQuickAccess(0, path);
        }

        public void InsertQuickAccess(QuickAccessNode? dst, string? path)
        {
            var index = dst != null ? QuickAccessCollection.Current.Items.IndexOf(dst.Source) : 0;
            if (index < 0)
            {
                return;
            }

            InsertQuickAccess(index, path);
        }

        public void InsertQuickAccess(int index, string? path)
        {
            if (_rootQuickAccess is null) return;

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            if (path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal))
            {
                ToastService.Current.Show(new Toast(Properties.TextResources.GetString("QuickAccessTempError.Message"), null, ToastIcon.Error));
                return;
            }

            _rootQuickAccess.IsExpanded = true;

            var item = QuickAccessCollection.Current.Items.FirstOrDefault(e => e.Path == path);
            if (item != null)
            {
                var node = _rootQuickAccess.Children.FirstOrDefault(e => ((QuickAccessNode)e).Source == item);
                if (node != null)
                {
                    SelectedItem = node;
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }
                return;
            }

            QuickAccessCollection.Current.Insert(index, new QuickAccess(path));
        }

        public bool RemoveQuickAccess(QuickAccessNode item)
        {
            if (item == null)
            {
                return false;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            bool isRemoved = QuickAccessCollection.Current.Remove(item.QuickAccessSource);
            if (isRemoved)
            {
                if (next != null)
                {
                    SelectedItem = next;
                }
            }
            return isRemoved;
        }

        public bool RemoveBookmarkFolder(BookmarkFolderNode item)
        {
            if (item == null || item is RootBookmarkFolderNode)
            {
                return false;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            var memento = new TreeListNodeMemento<IBookmarkEntry>(item.BookmarkSource);

            bool isRemoved = BookmarkCollection.Current.Remove(item.BookmarkSource);
            if (isRemoved)
            {
                if (item.BookmarkSource.Value is BookmarkFolder)
                {
                    var count = item.BookmarkSource.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        var toast = new Toast(Properties.TextResources.GetFormatString("BookmarkFolderDelete.Message", count), null, ToastIcon.Information, Properties.TextResources.GetString("Word.Restore"), () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show("FolderList", toast);
                    }
                }

                if (next != null)
                {
                    next.IsSelected = true;
                    SelectedItem = next;
                }
            }
            return isRemoved;
        }

        /// <summary>
        /// ノードの削除 (スクリプト用)
        /// </summary>
        /// <param name="item">削除ノード</param>
        /// <returns>成否</returns>
        /// <exception cref="NotSupportedException">削除できないノード</exception>
        public bool RemoveNode(FolderTreeNodeBase item)
        {
            switch (item)
            {
                case QuickAccessNode n:
                    return RemoveQuickAccess(n);
                case BookmarkFolderNode n:
                    return RemoveBookmarkFolder(n);
                default:
                    throw new NotSupportedException($"Unsupported type: {item.GetType().FullName}");
            }
        }

        public bool RemoveNodeAt(FolderTreeNodeBase parent, int index)
        {
            var item = parent.Children?[index];
            if (item is null) return false;

            return RemoveNode(item);
        }

        /// <summary>
        /// 新しいブックマークフォルダーの作成と追加
        /// </summary>
        public BookmarkFolderNode? NewBookmarkFolder(BookmarkFolderNode parent)
        {
            if (parent == null)
            {
                return null;
            }

            parent.IsExpanded = true;

            var node = BookmarkCollection.Current.AddNewFolder(parent.BookmarkSource, null);
            if (node == null)
            {
                return null;
            }

            var newItem = parent.Children.OfType<BookmarkFolderNode>().FirstOrDefault(e => e.Source == node);
            if (newItem != null)
            {
                SelectedItem = newItem;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }

            return newItem;
        }

        internal void AddBookmarkTo(BookmarkFolderNode item)
        {
            var address = BookHub.Current.GetCurrentBook()?.Path;
            if (address == null)
            {
                return;
            }

            var parentNode = item.BookmarkSource;

            // TODO: 重複チェックはBookmarkCollectionで行うようにする
            var node = parentNode.Children.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == address);
            if (node == null)
            {
                var unit = BookMementoCollection.Current.Set(address);
                node = new TreeListNode<IBookmarkEntry>(new Bookmark(unit));
                BookmarkCollection.Current.AddToChild(node, parentNode);
            }
        }

        public void MoveQuickAccess(QuickAccessNode src, QuickAccessNode dst)
        {
            if (src == dst)
            {
                return;
            }
            var srcIndex = QuickAccessCollection.Current.Items.IndexOf(src.Source);
            if (srcIndex < 0)
            {
                return;
            }
            var dstIndex = QuickAccessCollection.Current.Items.IndexOf(dst.Source);
            if (dstIndex < 0)
            {
                return;
            }
            QuickAccessCollection.Current.Move(srcIndex, dstIndex);
        }

        /// <summary>
        /// ノードの移動 (スクリプト用。実質クイックアクセス専用)
        /// </summary>
        /// <param name="parent">親ノード</param>
        /// <param name="oldIndex">移動する項目のインデックス番号</param>
        /// <param name="newIndex">項目の新しいインデックス番号</param>
        /// <exception cref="NotSupportedException"></exception>
        public void MoveNode(FolderTreeNodeBase parent, int oldIndex, int newIndex)
        {
            if (parent is not RootQuickAccessNode) throw new NotSupportedException();

            QuickAccessCollection.Current.Move(oldIndex, newIndex);
        }

        public void SyncDirectory(string place)
        {
            if (_rootDirectory is null) return;

            var path = new QueryPath(place);
            if (path.Scheme == QueryScheme.File)
            {
                _rootDirectory.RefreshDriveChildren();
            }
            else
            {
                return;
            }

            var node = GetDirectoryNode(path, true, true);
            if (node != null)
            {
                var parent = node.Parent;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = (parent as FolderTreeNodeBase)?.Parent;
                }

                SelectedItem = node;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private FolderTreeNodeBase? GetDirectoryNode(QueryPath path, bool createChildren, bool asFarAsPossible)
        {
            return path.Scheme switch
            {
                QueryScheme.File => _rootDirectory?.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible),
                QueryScheme.Bookmark => _rootBookmarkFolder?.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible),
                QueryScheme.QuickAccess => _rootBookmarkFolder?.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible),
                _ => throw new NotImplementedException(),
            };
        }

        public void RefreshDirectory()
        {
            if (_rootDirectory is null) return;

            _rootDirectory.Refresh();
        }
    }
}
