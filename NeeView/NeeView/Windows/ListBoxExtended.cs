using NeeView.Windows.Media;
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

        private const double _scrollLineDelta = 16.0;
        internal static double MouseWheelDelta => SystemParameters.WheelScrollLines * _scrollLineDelta;


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

        public bool FixHomeAndEndKeyBehavior
        {
            get { return (bool)GetValue(FixHomeAndEndKeyBehaviorProperty); }
            set { SetValue(FixHomeAndEndKeyBehaviorProperty, value); }
        }

        public static readonly DependencyProperty FixHomeAndEndKeyBehaviorProperty =
            DependencyProperty.Register(nameof(FixHomeAndEndKeyBehavior), typeof(bool), typeof(ListBoxExtended), new PropertyMetadata(false));

        public Orientation MouseWheelScrollOrientation
        {
            get { return (Orientation)GetValue(MouseWheelScrollOrientationProperty); }
            set { SetValue(MouseWheelScrollOrientationProperty, value); }
        }

        public static readonly DependencyProperty MouseWheelScrollOrientationProperty =
            DependencyProperty.Register(nameof(MouseWheelScrollOrientation), typeof(Orientation), typeof(ListBoxExtended), new PropertyMetadata(Orientation.Vertical));

        public double MouseWheelSpeedRate
        {
            get { return (double)GetValue(MouseWheelSpeedRateProperty); }
            set { SetValue(MouseWheelSpeedRateProperty, value); }
        }

        public static readonly DependencyProperty MouseWheelSpeedRateProperty =
            DependencyProperty.Register(nameof(MouseWheelSpeedRate), typeof(double), typeof(ListBoxExtended), new PropertyMetadata(1.0));

        public bool IsNavigateWithWrapEnabled
        {
            get { return (bool)GetValue(IsNavigateWithWrapEnabledProperty); }
            set { SetValue(IsNavigateWithWrapEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsNavigateWithWrapEnabledProperty =
            DependencyProperty.Register(nameof(IsNavigateWithWrapEnabled), typeof(bool), typeof(ListBoxExtended), new PropertyMetadata(false));


        public int ItemsCount => this.Items is not null ? this.Items.Count : 0;


        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // 仮想化かつスクロール単位がPixelでない場合はデフォルト処理
            if (!VirtualizingPanel.GetIsVirtualizing(this) || VirtualizingPanel.GetScrollUnit(this) != ScrollUnit.Pixel)
            {
                base.OnPreviewMouseWheel(e);
                return;
            }

            e.Handled = true;

            if (this.Items.Count == 0)
            {
                return;
            }

            ScrollViewer? scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(this);
            if (scrollViewer == null)
            {
                return;
            }

            double scrollAmount = (e.Delta / 120.0) * MouseWheelDelta * MouseWheelSpeedRate;
            if (MouseWheelScrollOrientation == Orientation.Vertical)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollAmount);
            }
            else
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - scrollAmount);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            NavigateLeftAndRightKey(e);
            if (e.Handled) return;

            NavigateHomeAndEndKey(e);
            if (e.Handled) return;

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// 左右キーで SelectedIndex 増減 (左右キーでの折り返し)
        /// </summary>
        private void NavigateLeftAndRightKey(KeyEventArgs e)
        {
            if (!IsNavigateWithWrapEnabled || this.ItemsCount <= 0)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                return;
            }

            var delta = e.Key switch
            {
                Key.Left => -1,
                Key.Right => +1,
                _ => 0
            };

            if (delta == 0)
            {
                return;
            }

            e.Handled = true;

            var focusedItem = Keyboard.FocusedElement as ListBoxItem;
            if (focusedItem == null)
            {
                return;
            }

            int currentIndex = this.ItemContainerGenerator.IndexFromContainer(focusedItem);
            int targetIndex = currentIndex + delta;

            Navigate(targetIndex);
        }

        /// <summary>
        /// Home/Endキーで正しくリストの先頭/末尾項目を選択するようにする
        /// </summary>
        private void NavigateHomeAndEndKey(KeyEventArgs e)
        {
            if (!FixHomeAndEndKeyBehavior || ItemsCount <= 0)
            {
                return;
            }

            if (e.Key == Key.Home)
            {
                Navigate(0);
                e.Handled = true;
            }
            else if (e.Key == Key.End)
            {
                Navigate(ItemsCount - 1);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 指定項目へナビゲート
        /// </summary>
        /// <param name="targetIndex"></param>
        private void Navigate(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= this.ItemsCount)
            {
                return;
            }

            var targetContainer = this.ItemContainerGenerator.ContainerFromIndex(targetIndex) as ListBoxItem;

            if (targetContainer == null)
            {
                this.ScrollIntoView(this.Items[targetIndex]);
                this.UpdateLayout();
                targetContainer = this.ItemContainerGenerator.ContainerFromIndex(targetIndex) as ListBoxItem;
            }

            if (targetContainer != null)
            {
                targetContainer.Focus();

                if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == 0)
                {
                    this.SelectedIndex = targetIndex;
                }
            }
        }

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
