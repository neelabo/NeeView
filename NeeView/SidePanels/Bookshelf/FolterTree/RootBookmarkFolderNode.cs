using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Properties;
using System;
using System.Diagnostics;
using System.Windows.Media;

namespace NeeView
{
    public class RootBookmarkFolderNode : BookmarkFolderNode
    {
        public RootBookmarkFolderNode(FolderTreeNodeBase? parent) : base(BookmarkCollection.Current.Items, parent)
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;

            Icon = new SingleImageSourceCollection(ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_grade_24px"));
        }


        public override string Name => QueryScheme.Bookmark.ToSchemeString();

        public override string DisplayName { get => TextResources.GetString("Word.Bookmark"); set { } }

        public override IImageSourceCollection Icon { get; }

        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Reset:
                case EntryCollectionChangedAction.Replace:
                    Source = BookmarkCollection.Current.Items;
                    RefreshChildren(isExpanded: true);
                    RaisePropertyChanged(nameof(Children));
                    break;

                case EntryCollectionChangedAction.Add:
                    Directory_Created(e.Parent, e.Item);
                    break;

                case EntryCollectionChangedAction.Remove:
                    Directory_Deleted(e.Parent, e.Item);
                    break;

                case EntryCollectionChangedAction.Rename:
                    Directory_Renamed(e.Parent, e.Item);
                    break;
            }
        }

        private void Directory_Created(TreeListNode<IBookmarkEntry>? parent, TreeListNode<IBookmarkEntry>? item)
        {
            if (parent is null) return;
            if (item is null) return;

            if (item.Value is not BookmarkFolder)
            {
                return;
            }

            Debug.WriteLine("Create: " + item.CreateQuery(QueryScheme.Bookmark));

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Bookmark));
            if (node != null)
            {
                ////AppDispatcher.BeginInvoke((Action)(() => node.Add(item)));
                var newNode = new BookmarkFolderNode(item, null);
                node.Add(newNode);
            }
            else
            {
                Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(TreeListNode<IBookmarkEntry>? parent, TreeListNode<IBookmarkEntry>? item)
        {
            if (parent is null) return;
            if (item is null) return;

            if (item.Value is not BookmarkFolder)
            {
                return;
            }

            Debug.WriteLine("Delete: " + item.Value.Name);

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Bookmark));
            if (node != null)
            {
                ////AppDispatcher.BeginInvoke((Action)(() => node.Remove(item)));
                node.Remove(item);
            }
            else
            {
                Debug.WriteLine("Skip delete");
            }
        }

        private void Directory_Renamed(TreeListNode<IBookmarkEntry>? parent, TreeListNode<IBookmarkEntry>? item)
        {
            if (parent is null) return;
            if (item is null) return;

            if (item.Value is not BookmarkFolder)
            {
                return;
            }

            Debug.WriteLine("Rename: " + item.CreateQuery(QueryScheme.Bookmark));

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Bookmark));
            if (node != null)
            {
                ////AppDispatcher.BeginInvoke((Action)(() => node.Rename(item)));
                node.Renamed(item);
            }
            else
            {
                Debug.WriteLine("Skip rename");
            }
        }

        private BookmarkFolderNode? GetDirectoryNode(QueryPath path)
        {
            return GetDirectoryNode(path.Path);
        }

        private BookmarkFolderNode? GetDirectoryNode(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this;
            }

            return GetFolderTreeNode(path, false, false) as BookmarkFolderNode;
        }

        public override bool CanRename()
        {
            return false;
        }

    }
}
