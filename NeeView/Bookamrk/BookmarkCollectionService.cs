using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Properties;
using System;
using System.Globalization;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ブックマークリスト用のブックマーク登録管理
    /// </summary>
    public static class BookmarkCollectionService
    {
        /// <summary>
        /// ブックマークを追加する
        /// </summary>
        public static TreeListNode<IBookmarkEntry>? Add(QueryPath query, TreeListNode<IBookmarkEntry>? parent, string? name, bool allowDuplicate)
        {
            if (parent is not null)
            {
                return AddTo(query, parent, name, allowDuplicate);
            }
            else
            {
                return Add(query, name, allowDuplicate);
            }
        }

        /// <summary>
        /// 現在開いているフォルダーリストの場所を優先してブックマークを追加する
        /// </summary>
        public static TreeListNode<IBookmarkEntry>? Add(QueryPath query, string? name, bool allowDuplicate)
        {
            if (!BookmarkFolderList.Current.AddBookmark(query, false))
            {
                return AddTo(query, BookmarkCollection.Current.Items, name, allowDuplicate);
            }
            return null;
        }

        /// <summary>
        /// ブックマークフォルダーを指定してブックマークを追加する
        /// </summary>
        public static TreeListNode<IBookmarkEntry>? AddTo(QueryPath query, TreeListNode<IBookmarkEntry> parent, string? name, bool allowDuplicate)
        {
            if (query.Scheme != QueryScheme.File)
            {
                return null;
            }

            if (!allowDuplicate)
            {
                if (FindBookmark(parent, query, null) != null)
                {
                    return null;
                }
            }

            var unit = BookMementoCollection.Current.Set(query.SimplePath);
            var bookmark = new Bookmark(unit);
            if (name is not null)
            {
                bookmark.Name = name;
            }
            var node = new TreeListNode<IBookmarkEntry>(bookmark);
            BookmarkCollection.Current.AddToChild(node, parent);

            return node;
        }

        /// <summary>
        /// ブックマークを削除する
        /// </summary>
        public static bool Remove(QueryPath query, TreeListNode<IBookmarkEntry>? parent)
        {
            if (parent is not null)
            {
                return RemoveFrom(query, parent);
            }
            else
            {
                return Remove(query);
            }
        }

        /// <summary>
        /// 現在開いているフォルダーリストを優先してブックマークを削除する
        /// </summary>
        public static bool Remove(QueryPath query)
        {
            if (BookshelfFolderList.Current.FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var node = bookmarkFolderCollection.BookmarkPlace.FirstOrDefault(e => e.IsEqual(query));
                if (node != null)
                {
                    return BookmarkCollection.Current.Remove(node);
                }
            }

            return BookmarkCollection.Current.Remove(BookmarkCollection.Current.FindNode(query));
        }

        /// <summary>
        /// 指定したフォルダーからブックマークを削除する
        /// </summary>
        public static bool RemoveFrom(QueryPath query, TreeListNode<IBookmarkEntry> parent)
        {
            var node = FindChildBookmark(query, parent);
            if (node != null)
            {
                return BookmarkCollection.Current.Remove(node);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ブックマークを削除
        /// </summary>
        /// <param name="node"></param>
        public static void Remove(TreeListNode<IBookmarkEntry> node)
        {
            BookmarkCollection.Current.Remove(node);
        }

        /// <summary>
        /// ブックマークを他のフォルダーに移動
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        public static void MoveToChild(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> parent)
        {
            BookmarkCollection.Current.MoveToChild(node, parent);
        }

        /// <summary>
        /// フォルダー内でブックマークを検索する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static TreeListNode<IBookmarkEntry>? FindChildBookmark(QueryPath path, TreeListNode<IBookmarkEntry>? parent)
        {
            if (path is null || path.Scheme != QueryScheme.File)
            {
                return null;
            }

            if (parent is null)
            {
                return BookmarkCollection.Current.FindNode(path);
            }
            else
            {
                return parent.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == path.SimplePath);
            }
        }

        /// <summary>
        /// ブックマークの名前変更とそれに伴う統合を行う
        /// </summary>
        public static bool Rename(TreeListNode<IBookmarkEntry> node, string newName)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Value is BookmarkFolder folder)
            {
                newName = BookmarkTools.GetValidateName(newName);
                var oldName = folder.Name;

                if (string.IsNullOrEmpty(newName))
                {
                    return false;
                }

                if (newName != oldName)
                {
                    var conflict = node.Parent?.FirstOrDefault(e => e != node && e.Value is BookmarkFolder && e.Value.Name == newName);
                    if (conflict != null)
                    {
                        var dialog = new MessageDialog(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("MergeFolderDialog.Message"), newName), TextResources.GetString("MergeFolderDialog.Title"));
                        dialog.Commands.Add(UICommands.Yes);
                        dialog.Commands.Add(UICommands.No);
                        var result = dialog.ShowDialog();

                        if (result.Command == UICommands.Yes)
                        {
                            BookmarkCollection.Current.Merge(node, conflict);
                            return true;
                        }
                    }
                    else
                    {
                        folder.Name = newName;
                        BookmarkCollection.Current.RaiseBookmarkChangedEvent(new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = oldName });
                        return true;
                    }
                }
                return false;
            }
            else if (node.Value is Bookmark bookmark)
            {
                newName = BookmarkTools.GetValidateName(newName);
                var oldName = bookmark.Name;
                bookmark.Name = newName;
                if (bookmark.Name != oldName)
                {
                    BookmarkCollection.Current.RaiseBookmarkChangedEvent(new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = oldName });
                }
                return true;
            }

            throw new InvalidOperationException($"Cannot rename node of type {node.Value.GetType().Name}.");
        }

        /// <summary>
        /// Query から BookmarkNode を生成する。親ノードはまだ未定。
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TreeListNode<IBookmarkEntry>? CreateBookmarkNode(QueryPath query)
        {
            if (!IsBookPath(query)) return null;

            var unit = BookMementoCollection.Current.Set(query.SimplePath);
            var bookmark = new Bookmark(unit);
            return new TreeListNode<IBookmarkEntry>(bookmark);
        }

        /// <summary>
        /// 有効なブックパスであるかを判定する
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool IsBookPath(QueryPath query)
        {
            if (query.Search != null)
            {
                return false;
            }

            return query.Scheme switch
            {
                QueryScheme.File => ArchiveManager.Current.IsSupported(query.SimplePath, true, true) || System.IO.Directory.Exists(query.SimplePath),
                _ => false,
            };
        }

        /// <summary>
        /// ブックマークフォルダーを検索する
        /// </summary>
        /// <param name="path">ブックマークフォルダ―パス</param>
        /// <returns>ブックマークノード。見つからなかったら null</returns>
        public static TreeListNode<IBookmarkEntry>? FindBookmarkFolder(QueryPath path)
        {
            var node = BookmarkCollection.Current.FindNode(path);
            return node != null && node.Value is BookmarkFolder ? node : null;
        }

        /// <summary>
        /// ブックマークフォルダ―からブックマークを検索する
        /// </summary>
        /// <param name="parent">検索するブックマークフォルダ―</param>
        /// <param name="query">ブックマークのターゲットパス</param>
        /// <param name="name">ブックマークの名前。null の場合はブックマークの名前を比較しない</param>
        /// <returns>ブックマークノード。見つからなかったら null</returns>
        public static TreeListNode<IBookmarkEntry>? FindBookmark(TreeListNode<IBookmarkEntry> parent, QueryPath query, string? name)
        {
            if (name is null)
            {
                return parent.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == query.SimplePath);
            }
            else
            {
                return parent.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Path == query.SimplePath && bookmark.Name == name);
            }
        }

    }
}
