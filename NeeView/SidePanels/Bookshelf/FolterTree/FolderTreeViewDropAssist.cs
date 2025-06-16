using NeeView.Collections.Generic;
using NeeView.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Media;
using System.Diagnostics;
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
            return item is TreeViewItem e && (e.DataContext is QuickAccessFolderNode || e.DataContext is BookmarkFolderNode);
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

            if (treeViewItem.DataContext is FolderTreeNodeBase node)
            {
                if (node.Parent == _vm.Model?.Root)
                {
                    return 0;
                }
            }

            if (treeViewItem.DataContext is QuickAccessFolderNode quickAccessNode)
            {
                if (quickAccessNode.IsExpanded && quickAccessNode.Children.Count > 0 && delta == +1)
                {
                    return 0;
                }
            }
            else if (treeViewItem.DataContext is BookmarkFolderNode bookmarkFolderNode)
            {
                return 0;
            }

            return delta;
        }
    }



    public class TreeViewDropAssistProfile : DropAssistProfile
    {
        public override FrameworkElement GetAdornerTarget(FrameworkElement itemsControl)
        {
            var presenter = VisualTreeUtility.GetChildElement<ScrollContentPresenter>(itemsControl);
            Debug.Assert(presenter != null);
            return presenter;
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return true;
        }

        public override FrameworkElement? ItemHitTest(FrameworkElement itemsControl, Point point)
        {
            Debug.Assert(itemsControl is TreeView);
            var item = VisualTreeUtility.HitTest<TreeViewItem>(itemsControl, point);
            return TreeViewTools.GetHeader(item);
        }

        public override DropTargetItem PointToDropTargetItem(DragEventArgs e, FrameworkElement itemsControl, bool allowInsert, Orientation orientation)
        {
            Debug.Assert(itemsControl is TreeView);
            var (item, view, rate) = TreeViewTools.PointToViewItemRate((TreeView)itemsControl, e, orientation);
            var delta = item is not null && allowInsert ? GetInsertOffset(rate, IsFolder(item)) : 0;
            return new DropTargetItem(item, view, delta);
        }
    }

}
