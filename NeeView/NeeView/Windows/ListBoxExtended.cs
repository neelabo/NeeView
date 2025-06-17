using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Windows
{
    /// <summary>
    /// 複数選択専用ListBix
    /// </summary>
    public class ListBoxExtended : ListBox, ITextSearchCollection
    {
        private readonly SimpleTextSearch _textSearch = new();


        public ListBoxExtended()
        {
            SelectionMode = SelectionMode.Extended;
            IsTextSearchEnabled = false;
        }


        public event EventHandler<MouseButtonEventArgs>? PreviewMouseUpWithSelectionChanged;

        public bool IsSimpleTextSearchEnabled
        {
            get { return (bool)GetValue(IsSimpleTextSearchEnabledProperty); }
            set { SetValue(IsSimpleTextSearchEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsSimpleTextSearchEnabledProperty =
            DependencyProperty.Register(nameof(IsSimpleTextSearchEnabled), typeof(bool), typeof(ListBoxExtended), new PropertyMetadata(true));

        public int ItemsCount => this.Items.Count;


        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            AppDispatcher.BeginInvoke(() => FocusSelectedItem(false));
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (IsSimpleTextSearchEnabled)
            {
                _textSearch.DoSearch(this, e.Text);
                e.Handled = true;
            }
            else
            {
                base.OnTextInput(e);
            }
        }

        public void FocusSelectedItem(bool force)
        {
            FocusItem(this.SelectedItem, force);
        }

        private void FocusItem(object? item, bool force)
        {
            if (item is null) return;

            var listBoxItem = this.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
            if (listBoxItem is not null)
            {
                if (force || this.IsKeyboardFocusWithin)
                {
                    listBoxItem.Focus();
                }
                this.SetAnchorItem(item);
            }
        }

        public void SetAnchorItem(object? item)
        {
            try
            {
                this.AnchorItem = item;
            }
            catch
            {
                // コンテナが生成されていないときに例外になるが、大きな影響はないので無視する
            }
        }

        public void ScrollSelectedItemsIntoView()
        {
            ScrollItemsIntoView(this.SelectedItems.Cast<object>());
        }

        public void ScrollItemsIntoView<T>(IEnumerable<T>? items)
        {
            if (items == null || !items.Any()) return;

            var top = items.First();

            // なるべく選択範囲が表示されるようにスクロールする
            this.ScrollIntoView(items.Last());
            this.UpdateLayout();
            this.ScrollIntoView(top);
            this.UpdateLayout();

            FocusItem(top, false);
        }

        public void SetSelectedItems<T>(IEnumerable<T>? newItems)
        {
            base.SetSelectedItems(newItems);
        }

        public void RaisePreviewMouseUpWithSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseUpWithSelectionChanged?.Invoke(sender, e);
        }

        public void NavigateToItem(int index)
        {
            this.SelectedIndex = index;
            this.ScrollIntoView(this.SelectedItem);
            this.SetAnchorItem(this.SelectedItem);
            this.FocusItem(this.SelectedItem, true);
        }

        public string? GetPrimaryText(int index)
        {
            var count = this.Items.Count;
            if (count == 0) return null;

            if (index < 0 || index >= count)
            {
                return null;
            }

            return this.Items[index]?.ToString();
        }
    }
}
