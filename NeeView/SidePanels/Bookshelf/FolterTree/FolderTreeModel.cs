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

                case QuickAccessNode quickAccessNode when quickAccessNode.QuickAccessSource.Value is QuickAccess quickAccess:
                    SetFolderListPlace(quickAccess.Path);
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
        public FolderTreeNodeBase? NewNode(FolderTreeNodeBase parent, string? option)
        {
            if (parent is null) return null;

            return parent switch
            {
                QuickAccessFolderNode n => option switch
                {
                    "folder"
                        => NewQuickAccessFolder(n),
                    _
                        => NewQuickAccess(n)
                },
                BookmarkFolderNode n
                    => NewBookmarkFolder(n),
                _
                    => null,
            };
        }

        /// <summary>
        /// 新しいノードの作成と挿入 (スクリプト用)
        /// </summary>
        /// <param name="parent">親ノード</param>
        /// <param name="index">挿入位置</param>
        /// <param name="type">生成ノードの種類</param>
        /// <returns>新しいノード。作れなかったら null</returns>
        public FolderTreeNodeBase? NewNode(FolderTreeNodeBase parent, int index, string? type)
        {
            if (parent is null) return null;

            return parent switch
            {
                QuickAccessFolderNode n => type?.ToLower() switch
                {
                    "folder"
                        => NewQuickAccessFolder(n, index),
                    _
                        => NewQuickAccess(n, index)
                },
                BookmarkFolderNode n
                    => NewBookmarkFolder(n), // NOTE: ブックマークフォルダ―は挿入できない
                _
                    => null,
            };
        }

        /// <summary>
        /// 新しいクイックアクセスの作成と追加 (スクリプト用)
        /// </summary>
        private QuickAccessNode? NewQuickAccess(QuickAccessFolderNode parent)
        {
            return NewQuickAccess(parent, -1);
        }

        /// <summary>
        /// 新しいクイックアクセスの作成と挿入 (スクリプト用)
        /// </summary>
        private QuickAccessNode? NewQuickAccess(QuickAccessFolderNode parent, int index)
        {
            return InsertQuickAccess(parent, index, _folderList.GetCurrentQueryPath());
        }

        /// <summary>
        /// 新しい QuickAccess を追加
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="path"></param>
        public QuickAccessNode? AddQuickAccess(QuickAccessFolderNode? parent, string? path)
        {
            return InsertQuickAccess(parent, -1, path);
        }

        /// <summary>
        /// 新しい QuickAccess を挿入
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="index">挿入位置</param>
        /// <param name="path">新しい QuickAccess パス</param>
        public QuickAccessNode? InsertQuickAccess(QuickAccessFolderNode? parent, int index, string? path)
        {
            parent ??= _rootQuickAccess;
            if (parent is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal))
            {
                ToastService.Current.Show(new Toast(Properties.TextResources.GetString("QuickAccessTempError.Message"), null, ToastIcon.Error));
                return null;
            }

            parent.IsExpanded = true;

            var item = parent.QuickAccessSource.Children.FirstOrDefault(e => e.Value.Path == path);
            if (item is null)
            {
                item = new TreeListNode<IQuickAccessEntry>(new QuickAccess(path));
                QuickAccessCollection.Current.Insert(parent.QuickAccessSource, index, item);
            }

            var node = parent.Children.OfType<QuickAccessNode>().FirstOrDefault(e => e.QuickAccessSource == item);
            if (node != null)
            {
                SelectedItem = node;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }

            return node;
        }

        public QuickAccessFolderNode? NewQuickAccessFolder(QuickAccessFolderNode parent)
        {
            return NewQuickAccessFolder(parent, -1);
        }

        public QuickAccessFolderNode? NewQuickAccessFolder(QuickAccessFolderNode parent, int index)
        {
            if (parent == null)
            {
                return null;
            }

            parent.IsExpanded = true;

            var node = QuickAccessCollection.Current.InsertNewFolder(parent.QuickAccessSource, index, null);
            if (node == null)
            {
                return null;
            }

            var newItem = parent.Children.OfType<QuickAccessFolderNode>().FirstOrDefault(e => e.Source == node);
            if (newItem != null)
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
                case QuickAccessFolderNode quickAccessFolder:
                    AddQuickAccess(quickAccessFolder, _folderList.GetCurrentQueryPath());
                    break;

                case DirectoryNode folder:
                    AddQuickAccess(folder.Path);
                    break;

                case string filename:
                    AddQuickAccess(filename);
                    break;
            }
        }

        /// <summary>
        /// 新しい QuickAccess を挿入
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="dst">挿入一のノード。このノードの手前に新しく挿入する</param>
        /// <param name="path">新しい QuickAccess パス</param>
        public void InsertQuickAccess(QuickAccessFolderNode? parent, QuickAccessNodeBase? dst, int delta, string? path)
        {
            // 子として追加
            if (delta == 0)
            {
                parent ??= dst as QuickAccessFolderNode ?? _rootQuickAccess;
                AddQuickAccess(parent, path);
            }
            // 前後に追加
            else
            {
                parent ??= dst?.Parent as QuickAccessFolderNode ?? _rootQuickAccess;
                if (parent is null)
                {
                    return;
                }

                var index = dst != null ? parent.QuickAccessSource.Children.IndexOf(dst.Source) : 0;
                if (index < 0)
                {
                    return;
                }

                if (delta > 0)
                {
                    index++;
                }

                InsertQuickAccess(parent, index, path);
            }
        }

        public bool RemoveQuickAccessFolder(QuickAccessFolderNode item)
        {
            if (item == null)
            {
                return false;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            bool isRemoved = QuickAccessCollection.Current.RemoveSelf(item.QuickAccessSource);
            if (isRemoved)
            {
                if (next != null)
                {
                    SelectedItem = next;
                }
            }
            return isRemoved;
        }

        public bool RemoveQuickAccess(QuickAccessNode item)
        {
            if (item == null)
            {
                return false;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            bool isRemoved = QuickAccessCollection.Current.RemoveSelf(item.QuickAccessSource);
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
                case QuickAccessFolderNode n:
                    return RemoveQuickAccessFolder(n);
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

        public void MoveQuickAccess(QuickAccessNodeBase src, QuickAccessNodeBase dst, int delta)
        {
            if (src == dst)
            {
                return;
            }

            // 子に移動
            if (delta == 0)
            {
                var parent = (dst as QuickAccessFolderNode)?.QuickAccessSource;
                if (parent is null)
                {
                    return;
                }
                var item = src.QuickAccessSource;
                QuickAccessCollection.Current.Move(parent, item, -1);
            }

            // 階層をまたいだ移動
            else if (src.Parent != dst.Parent)
            {
                var parent = (dst.Parent as QuickAccessFolderNode)?.QuickAccessSource;
                if (parent is null)
                {
                    return;
                }

                var item = src.QuickAccessSource;
                var dstIndex = parent.Children.IndexOf(dst.Source);
                if (dstIndex < 0)
                {
                    return;
                }

                if (delta > 0)
                {
                    dstIndex++;
                }

                QuickAccessCollection.Current.Move(parent, item, dstIndex);
            }

            // 同一階層での移動
            else
            {
                var parent = (src.Parent as QuickAccessFolderNode)?.QuickAccessSource;
                if (parent is null)
                {
                    return;
                }

                var srcIndex = parent.Children.IndexOf(src.Source);
                if (srcIndex < 0)
                {
                    return;
                }
                var dstIndex = parent.Children.IndexOf(dst.Source);
                if (dstIndex < 0)
                {
                    return;
                }

                if (srcIndex < dstIndex)
                {
                    if (delta < 0)
                    {
                        dstIndex -= 1;
                    }
                }
                else
                {
                    if (delta > 0)
                    {
                        dstIndex += 1;
                    }
                }

                QuickAccessCollection.Current.Move(parent, srcIndex, dstIndex);
            }
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
            if (parent is not QuickAccessFolderNode folder) throw new NotSupportedException();

            QuickAccessCollection.Current.Move(folder.QuickAccessSource, oldIndex, newIndex);
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
