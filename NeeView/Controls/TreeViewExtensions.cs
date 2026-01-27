using NeeView.Collections.Generic;
using NeeView.Windows.Media;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public static class TreeViewExtensions
    {
        public static ItemsControl? ScrollIntoView<T>(this TreeView treeView, T? item)
        where T : ITreeNode
        {
            if (item == null)
            {
                return null;
            }

            treeView.UpdateLayout();

            ItemsControl? container = treeView;
            var lastContainer = container;

            // 上位ノードから順番に ScrollIntoView する。
            foreach (var node in item.GetHierarchy())
            {
                int index;
                if (node.Parent == null)
                {
                    // TreeViewItemsSource.MultiRoot=true の場合は複数のルートを持つ。
                    // そうでない場合、ルートの子供から TreeView のアイテムにする前提なので Skip(1) している。
                    var multiRoot = TreeViewItemsSource.GetMultiRoot(treeView);
                    if (multiRoot)
                    {
                        index = IndexOf(container.ItemsSource, node);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (node.Parent.Children is null)
                    {
                        break;
                    }
                    index = node.Parent.Children.IndexOf(node);
                }

                if (index < 0)
                {
                    break;
                }

                container = ScrollIntoView(container, index);
                if (container == null)
                {
                    break;
                }

                container.UpdateLayout();
                lastContainer = container;
            }

            return lastContainer;
        }

        // from https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview
        // HACK: BindingErrorが出る
        private static TreeViewItem? ScrollIntoView(ItemsControl container, int index)
        {
            // Expand the current container
            if (container is TreeViewItem item && !item.IsExpanded)
            {
                container.SetValue(TreeViewItem.IsExpandedProperty, true);
                container.UpdateLayout();
            }

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the 
            // virtualizing case even if the item is marked 
            // expanded we still need to do this step in order to 
            // regenerate the visuals because they may have been virtualized away.
            container.ApplyTemplate();
            ItemsPresenter? itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter, 
                // so walk the descendents and find the child.
                itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();
                    itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                }
            }

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            // Ensure that the generator for this panel has been created.
            _ = itemsHostPanel.Children;

            if (itemsHostPanel is CustomVirtualizingStackPanel virtualizingPanel)
            {
                virtualizingPanel.BringIntoView(index);
                var subContainer = (TreeViewItem?)container.ItemContainerGenerator.ContainerFromIndex(index);
                return subContainer;
            }
            else
            {
                var subContainer = (TreeViewItem?)container.ItemContainerGenerator.ContainerFromIndex(index);
                // Bring the item into view to maintain the 
                // same behavior as with a virtualizing panel.
                subContainer?.BringIntoView();
                return subContainer;
            }
        }

        private static int IndexOf(IEnumerable items, object? value)
        {
            int index = 0;
            foreach (var item in items)
            {
                if (item == value) return index;
                index++;
            }
            return -1;
        }
    }


    public static class TreeViewItemsSource
    {
        public static readonly DependencyProperty MultiRootProperty =
            DependencyProperty.RegisterAttached("MultiRoot", typeof(bool), typeof(TreeViewItemsSource), new PropertyMetadata(false));

        public static bool GetMultiRoot(DependencyObject obj) => (bool)obj.GetValue(MultiRootProperty);
        public static void SetMultiRoot(DependencyObject obj, bool value) => obj.SetValue(MultiRootProperty, value);
    }
}
