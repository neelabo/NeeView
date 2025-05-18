using NeeView.Collections.Generic;
using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxItem用RenameControl定義
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ListBoxItemRenameControl<TItem> : RenameControl
        where TItem : class, IRenameable
    {
        private readonly Window _window;
        private readonly Locker _scrollChangedLocker = new();
        protected ListBox _listBox;
        protected TItem _item;

        public ListBoxItemRenameControl(ListBox listBox, TItem item) : this(listBox, item, true)
        {
        }

        public ListBoxItemRenameControl(ListBox listBox, TItem item, bool findTextBlock) : base(CreateRenameControlSource(listBox, item, findTextBlock))
        {
            _window = Window.GetWindow(listBox);
            _listBox = listBox;
            _item = item;
            this.Loaded += (s, e) => OnLoaded();
            this.Unloaded += (s, e) => OnUnloaded();
        }


        private static RenameControlSource CreateRenameControlSource(ListBox listBox, TItem item, bool findTextBlock)
        {
            listBox.ScrollIntoView(item);
            listBox.UpdateLayout();
            var listBoxItem = VisualTreeUtility.GetListBoxItemFromItem(listBox, item) ?? throw new InvalidOperationException("ListBoxItem not found.");
            var textBlock = findTextBlock ? VisualTreeUtility.FindVisualChild<TextBlock>(listBoxItem, "FileNameTextBlock") ?? throw new InvalidOperationException("TextBlock(FileNameTextBlock) not found.") : null;
            return new RenameControlSource(listBoxItem, textBlock, item.GetRenameText());
        }

        protected override async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            Debug.Assert(oldValue != newValue);
            return await _item.RenameAsync(newValue);
        }

        private void OnLoaded()
        {
            _window.SizeChanged += Window_SizeChanged;
            _listBox.SelectionChanged += ListBox_SelectionChanged;
            _listBox.Unloaded += ListBox_Unloaded;
            _listBox.AddHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ListBox_ScrollChanged);
        }

        private void OnUnloaded()
        {
            _window.SizeChanged -= Window_SizeChanged;
            _listBox.SelectionChanged -= ListBox_SelectionChanged;
            _listBox.Unloaded -= ListBox_Unloaded;
            _listBox.RemoveHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ListBox_ScrollChanged);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            using var locked = _scrollChangedLocker.Lock();
            _listBox.ScrollIntoView(_listBox.SelectedItem);
            _listBox.UpdateLayout();
            SyncLayout();
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await CloseAsync(false, false);
        }

        private async void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            await CloseAsync(false, false);
        }

        private async void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollChangedLocker.IsLocked) return;
            if (e.VerticalChange != 0.0 || e.HorizontalChange != 0.0)
            {
                await CloseAsync(true, true);
            }
        }
    }
}
