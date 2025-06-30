using NeeView.Collections.Generic;
using NeeView.ComponentModel;
using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class FolderTreeViewDropAssist : InsertDropAssist
    {
        public FolderTreeViewDropAssist(TreeView treeView, FolderTreeViewModel vm)
            : base(treeView, new FolderTreeViewDropAssistProfile(vm))
        {
        }
    }


    public class FolderTreeViewDropAssistProfile : TreeViewDropAssistProfile
    {
        private readonly FolderTreeViewModel _vm;

        public FolderTreeViewDropAssistProfile(FolderTreeViewModel vm)
        {
            _vm = vm;
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return item is TreeViewItem e && (e.DataContext is TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } || e.DataContext is BookmarkFolderNode);
        }

        public override DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation)
        {
            var target = base.PointToDropTargetItem(e, itemsControl, allowInsert, orientation);
            var delta = AdjustDropTargetDelta(target.Item, target.Delta);
            if (delta != target.Delta)
            {
                target = target with { Delta = delta };
            }
            return target;
        }

        public override Rect AdjustElementRect(Rect rect)
        {
            return rect.InflateTop(1.0);
        }

        private int AdjustDropTargetDelta(FrameworkElement? item, int delta)
        {
            if (item is not TreeViewItem treeViewItem)
            {
                return delta;
            }
            if (treeViewItem.DataContext is not ITreeViewNode node)
            {
                return delta;
            }

            if (node.Parent == null)
            {
                return 0;
            }

            if (node is TreeListNode<QuickAccessEntry> quickAccessNode)
            {
                if (quickAccessNode.IsExpanded && quickAccessNode.Count > 0 && delta == 1)
                {
                    return 0;
                }
            }

            else if (node is BookmarkFolderNode)
            {
                return 0;
            }

            return delta;
        }
    }

}
