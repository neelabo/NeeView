using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class TreeViewRenameControl<TItem> : RenameControl
        where TItem : class, IRenameable
    {
        private readonly Window _window;
        private readonly Locker _scrollChangedLocker = new();
        protected TreeView _treeView;
        protected TItem _item;

        public TreeViewRenameControl(TreeView treeView, TItem item) : base(CreateRenameControlSource(treeView, item))
        {
            _window = Window.GetWindow(treeView);
            _treeView = treeView;
            _item = item;

            this.Loaded += (s, e) => OnLoaded();
            this.Unloaded += (s, e) => OnUnloaded();
        }


        private static RenameControlSource CreateRenameControlSource(TreeView treeView, TItem item)
        {
            var treeViewItem = VisualTreeUtility.FindContainer<TreeViewItem>(treeView, item) ?? throw new InvalidOperationException("Container not found: TreeViewItem");
            treeViewItem.BringIntoView(); // TODO: ScrollIntoView的なものは上位で？
            treeView.UpdateLayout();
            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(treeViewItem, "FileNameTextBlock") ?? throw new InvalidOperationException("Control not found: FileNameTextBlock");
            return new RenameControlSource(treeViewItem, textBlock, item.GetRenameText());
        }

        protected override async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            Debug.Assert(oldValue != newValue);
            return await _item.RenameAsync(newValue);
        }

        private void OnLoaded()
        {
            _window.SizeChanged += Window_SizeChanged;
            _treeView.Unloaded += TreeView_Unloaded;
            _treeView.AddHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ScrollViewer_ScrollChanged);
        }

        private void OnUnloaded()
        {
            _window.SizeChanged -= Window_SizeChanged;
            _treeView.Unloaded -= TreeView_Unloaded;
            _treeView.RemoveHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ScrollViewer_ScrollChanged);
        }

        private async void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await CloseAsync(false, false);
        }

        private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await CloseAsync(false, false);
        }

        private async void TreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            await CloseAsync(false, false);
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollChangedLocker.IsLocked) return;
            if (e.VerticalChange != 0.0 || e.HorizontalChange != 0.0)
            {
                await CloseAsync(true, true);
            }
        }
    }
}
