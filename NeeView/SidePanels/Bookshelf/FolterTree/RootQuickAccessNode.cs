using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{
    public class RootQuickAccessNode : QuickAccessFolderNode
    {
        public RootQuickAccessNode() : base(QuickAccessCollection.Current.Root, null)
        {
            // NOTE: need call Initialize()
            Icon = new SingleImageSourceCollection(ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_lightning"));
        }


        public override string Name { get => QueryScheme.QuickAccess.ToSchemeString(); set { } }

        public override string DisplayName { get => Properties.TextResources.GetString("Word.QuickAccess"); set { } }

        public override IImageSourceCollection Icon { get; }


        public void Initialize(FolderTreeNodeBase parent)
        {
            Parent = parent;

            RefreshChildren();
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }


        private void QuickAccessCollection_CollectionChanged(object? sender, TreeCollectionChangeEventArgs<IQuickAccessEntry> e)
        {
            switch (e.Action)
            {
                case TreeCollectionChangeAction.Refresh:
                    Source = BookmarkCollection.Current.Items;
                    RefreshChildren(isExpanded: true);
                    RaisePropertyChanged(nameof(Children));
                    break;

                case TreeCollectionChangeAction.Add:
                    Directory_Created(e.NewItem, e.NewIndex);
                    break;

                case TreeCollectionChangeAction.Remove:
                    Directory_Deleted(e.OldParent, e.OldItem);
                    break;

                case TreeCollectionChangeAction.Move:
                    Directory_Move(e.NewItem, e.OldIndex, e.NewIndex);
                    break;
            }
        }

        private void Directory_Move(TreeListNode<IQuickAccessEntry>? item, int oldIndex, int newIndex)
        {
            if (item is null) return;

            var parent = item.Parent;
            if (parent is null) return;

            var node = GetDirectoryNode(parent);
            if (node != null)
            {
                node.Children.Move(oldIndex, newIndex);
            }
            else
            {
                Debug.WriteLine("Skip move");
            }
        }

        private void Directory_Created(TreeListNode<IQuickAccessEntry>? item, int index)
        {
            if (item is null) return;

            // 不要？
            if (item.Value is not IQuickAccessEntry)
            {
                return;
            }

            //Debug.WriteLine("Create: " + item.CreateQuery(QueryScheme.Bookmark));

            var parent = item.Parent;
            if (parent is null) return;

            var node = GetDirectoryNode(parent);
            if (node != null)
            {
                var newNode = CreateFolderNode(item, null);
                node.Insert(index, newNode);
            }
            else
            {
                Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(TreeListNode<IQuickAccessEntry>? parent, TreeListNode<IQuickAccessEntry>? item)
        {
            if (parent is null) return;
            if (item is null) return;

            // 不要？
            if (item.Value is not IQuickAccessEntry)
            {
                return;
            }

            Debug.WriteLine("Delete: " + item.Value.Name);

            var node = GetDirectoryNode(parent);
            if (node != null)
            {
                node.Remove(item);
            }
            else
            {
                Debug.WriteLine("Skip delete");
            }
        }

        private QuickAccessFolderNode? GetDirectoryNode(TreeListNode<IQuickAccessEntry>? item)
        {
            if (item is null) return null;

            return this.Walk().OfType<QuickAccessFolderNode>().FirstOrDefault(e => e.QuickAccessSource == item);
        }

#if false
        // NOTE: 重複したパスに非対応
        private QuickAccessFolderNode? GetDirectoryNode(QueryPath path)
        {
            return GetDirectoryNode(path.Path);
        }

        private QuickAccessFolderNode? GetDirectoryNode(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this;
            }

            return GetFolderTreeNode(path, false, false) as QuickAccessFolderNode;
        }
#endif

        public override bool CanRename()
        {
            return false;
        }

    }
}
