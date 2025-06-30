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
    public class FolderTreeModel : BindableBase
    {
        private readonly FolderList _folderList;
        private readonly ITreeViewNode? _rootQuickAccess;
        private readonly RootDirectoryNode? _rootDirectory;
        private readonly RootBookmarkFolderNode? _rootBookmarkFolder;
        private readonly ObservableCollection<ITreeViewNode> _items = new();
        private ITreeViewNode? _selectedItem;


        public FolderTreeModel(FolderList folderList, FolderTreeCategory categories)
        {
            _folderList = folderList;

            if ((categories & FolderTreeCategory.QuickAccess) != 0)
            {
                _rootQuickAccess = QuickAccessCollection.Current.Root;
                _items.Add(_rootQuickAccess);
            }

            if ((categories & FolderTreeCategory.Directory) != 0)
            {
                _rootDirectory = new RootDirectoryNode(null);
                _items.Add(_rootDirectory);
            }

            if ((categories & FolderTreeCategory.BookmarkFolder) != 0)
            {
                _rootBookmarkFolder = new RootBookmarkFolderNode(null);
                _items.Add(_rootBookmarkFolder);
            }
        }


        public event EventHandler? SelectedItemChanged;


        public ObservableCollection<ITreeViewNode> Items => _items;
        public ITreeViewNode? RootQuickAccess => _rootQuickAccess;
        public RootDirectoryNode? RootDirectory => _rootDirectory;
        public RootBookmarkFolderNode? RootBookmarkFolder => _rootBookmarkFolder;

        public ITreeViewNode? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value is null || !_items.Contains(value.GetRoot()))
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


        public void SetSelectedItem(ITreeViewNode? node)
        {
            SelectedItem = node;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public void FocusAtOnce()
        {
            IsFocusAtOnce = true;
        }

        public void ExpandRoot()
        {
            foreach (var node in _items)
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
                case TreeListNode<QuickAccessEntry> { Value: QuickAccess quickAccess }:
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
        public ITreeViewNode? NewNode(ITreeViewNode parent, string? option)
        {
            if (parent is null) return null;

            return parent switch
            {
                TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } n => option switch
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
        /// <param name="option">生成ノードの種類</param>
        /// <returns>新しいノード。作れなかったら null</returns>
        public ITreeViewNode? NewNode(ITreeViewNode parent, int index, string? option)
        {
            if (parent is null) return null;

            return parent switch
            {
                TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } n => option switch
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
        private TreeListNode<QuickAccessEntry>? NewQuickAccess(TreeListNode<QuickAccessEntry> parent)
        {
            return NewQuickAccess(parent, -1);
        }

        /// <summary>
        /// 新しいクイックアクセスの作成と挿入 (スクリプト用)
        /// </summary>
        private TreeListNode<QuickAccessEntry>? NewQuickAccess(TreeListNode<QuickAccessEntry> parent, int index)
        {
            return InsertQuickAccess(parent, index, _folderList.GetCurrentQueryPath());
        }

        /// <summary>
        /// 新しい QuickAccess を追加
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="path"></param>
        public TreeListNode<QuickAccessEntry>? AddQuickAccess(TreeListNode<QuickAccessEntry>? parent, string? path)
        {
            return InsertQuickAccess(parent, -1, path);
        }

        /// <summary>
        /// 新しい QuickAccess を挿入
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="index">挿入位置</param>
        /// <param name="path">新しい QuickAccess パス</param>
        public TreeListNode<QuickAccessEntry>? InsertQuickAccess(TreeListNode<QuickAccessEntry>? parent, int index, string? path)
        {
            if (parent is null || parent.Value is not QuickAccessFolder)
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

            var item = parent.FirstOrDefault(e => e.Value.Path == path);
            if (item is null)
            {
                item = new TreeListNode<QuickAccessEntry>(new QuickAccess(path));
                parent.Insert(index, item);
            }

            SelectedItem = item;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);

            return item;
        }

        public TreeListNode<QuickAccessEntry>? NewQuickAccessFolder(TreeListNode<QuickAccessEntry> parent)
        {
            return NewQuickAccessFolder(parent, -1);
        }

        public TreeListNode<QuickAccessEntry>? NewQuickAccessFolder(TreeListNode<QuickAccessEntry> parent, int index)
        {
            if (parent == null)
            {
                return null;
            }

            parent.IsExpanded = true;

            var node = QuickAccessCollection.Current.InsertNewFolder(parent, index, null);
            if (node == null)
            {
                return null;
            }

            SelectedItem = node;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            return node;
        }

        /// <summary>
        /// 現在の場所を新しい QuickAccess として追加
        /// </summary>
        /// <param name="parent"></param>
        public void AddCurrentPlaceQuickAccess(TreeListNode<QuickAccessEntry> parent)
        {
            AddQuickAccess(parent, _folderList.GetCurrentQueryPath());
        }

        /// <summary>
        /// 新しい QuickAccess を挿入
        /// </summary>
        /// <param name="parent">親フォルダー</param>
        /// <param name="dst">挿入一のノード。このノードの手前に新しく挿入する</param>
        /// <param name="path">新しい QuickAccess パス</param>
        public void InsertQuickAccess(TreeListNode<QuickAccessEntry>? parent, TreeListNode<QuickAccessEntry>? dst, int delta, string? path)
        {
            // 子として追加
            if (delta == 0)
            {
                parent ??= dst?.Value is QuickAccessFolder ? dst : null;
                AddQuickAccess(parent, path);
            }
            // 前後に追加
            else
            {
                parent ??= dst?.Parent?.Value is QuickAccessFolder ? dst.Parent : null;
                if (parent is null)
                {
                    return;
                }

                var index = dst != null ? parent.IndexOf(dst) : 0;
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

        public bool RemoveQuickAccess(TreeListNode<QuickAccessEntry> item)
        {
            if (item == null)
            {
                return false;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            bool isRemoved = item.RemoveSelf();
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
                    var count = item.BookmarkSource.WalkChildren().Count(e => e.Value is Bookmark);
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
        public bool RemoveNode(ITreeViewNode item)
        {
            switch (item)
            {
                case TreeListNode<QuickAccessEntry> n:
                    return RemoveQuickAccess(n);
                case BookmarkFolderNode n:
                    return RemoveBookmarkFolder(n);
                default:
                    throw new NotSupportedException($"Unsupported type: {item.GetType().FullName}");
            }
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
            var node = parentNode.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == address);
            if (node == null)
            {
                var unit = BookMementoCollection.Current.Set(address);
                node = new TreeListNode<IBookmarkEntry>(new Bookmark(unit));
                BookmarkCollection.Current.AddToChild(node, parentNode);
            }
        }

        public void MoveQuickAccess(TreeListNode<QuickAccessEntry> src, TreeListNode<QuickAccessEntry> dst, int delta)
        {
            if (src == dst)
            {
                return;
            }

            // 子に移動
            if (delta == 0)
            {
                var parent = dst;
                if (parent is null)
                {
                    return;
                }
                parent.IsExpanded = true;
                src.MoveTo(parent, -1);
            }

            // 階層をまたいだ移動
            else if (src.Parent != dst.Parent)
            {
                var parent = dst.Parent;
                if (parent is null)
                {
                    return;
                }

                var dstIndex = parent.IndexOf(dst);
                if (dstIndex < 0)
                {
                    return;
                }

                if (delta > 0)
                {
                    dstIndex++;
                }

                src.MoveTo(parent, dstIndex);
            }

            // 同一階層での移動
            else
            {
                var parent = src.Parent;
                if (parent is null)
                {
                    return;
                }

                var srcIndex = parent.IndexOf(src);
                if (srcIndex < 0)
                {
                    return;
                }
                var dstIndex = parent.IndexOf(dst);
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

                parent.Move(srcIndex, dstIndex);
            }
        }

        /// <summary>
        /// ノードの移動 (スクリプト用。実質クイックアクセス専用)
        /// </summary>
        /// <param name="parent">親ノード</param>
        /// <param name="oldIndex">移動する項目のインデックス番号</param>
        /// <param name="newIndex">項目の新しいインデックス番号</param>
        /// <exception cref="NotSupportedException"></exception>
        public void MoveNode(ITreeViewNode parent, int oldIndex, int newIndex)
        {
            if (parent is not TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } folder) throw new NotSupportedException();

            folder.Move(oldIndex, newIndex);
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
                    parent = parent?.Parent;
                }

                SelectedItem = node;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ITreeViewNode? GetDirectoryNode(QueryPath path, bool createChildren, bool asFarAsPossible)
        {
            return path.Scheme switch
            {
                QueryScheme.File => _rootDirectory?.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible),
                QueryScheme.Bookmark => _rootBookmarkFolder?.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible),
                QueryScheme.QuickAccess => (_rootQuickAccess as TreeListNode<QuickAccessEntry>)?.GetNode(path.Path, asFarAsPossible),
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
