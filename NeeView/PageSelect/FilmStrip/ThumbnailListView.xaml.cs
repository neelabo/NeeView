using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ThumbnailListView.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailListView : UserControl, IVisibleElement
    {
        #region DependencyProperties

        public ThumbnailList Source
        {
            get { return (ThumbnailList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ThumbnailList), typeof(ThumbnailListView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailListView control)
            {
                control.Initialize();
            }
        }


        public bool IsContentVisible
        {
            get { return (bool)GetValue(IsContentVisibleProperty); }
            private set { SetValue(IsContentVisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsContentVisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("IsContentVisible", typeof(bool), typeof(ThumbnailListView), new PropertyMetadata(false));

        public static readonly DependencyProperty IsContentVisibleProperty = IsContentVisiblePropertyKey.DependencyProperty;


        public bool IsBackgroundOpacityEnabled
        {
            get { return (bool)GetValue(IsBackgroundOpacityEnabledProperty); }
            set { SetValue(IsBackgroundOpacityEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsBackgroundOpacityEnabledProperty =
            DependencyProperty.Register("IsBackgroundOpacityEnabled", typeof(bool), typeof(ThumbnailListView), new PropertyMetadata(false));


        #endregion


        // フィルムストリップのパネルコントロール
        private VirtualizingStackPanel? _listPanel;

        private ThumbnailListViewModel? _vm;
        private bool _isThumbnailDirty;


        /// <summary>
        /// サムネイル更新要求を拒否する
        /// </summary>
        private bool _isFrozen;

        private readonly MouseWheelDelta _mouseWheelDelta = new();



        static ThumbnailListView()
        {
            InitializeCommandStatic();
        }

        public ThumbnailListView()
        {
            InitializeComponent();

            _commandResource = new ThumbnailListItemCommandResource();
            InitializeCommand();

            this.Root.IsVisibleChanged +=
                (s, e) => this.IsContentVisible = (bool)e.NewValue;
        }


        #region Commands

        public static readonly RoutedCommand OpenCommand = new(nameof(OpenCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand OpenBookCommand = new(nameof(OpenBookCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand OpenExplorerCommand = new(nameof(OpenExplorerCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand OpenExternalAppCommand = new(nameof(OpenExternalAppCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand CutCommand = new(nameof(CutCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand CopyToFolderCommand = new(nameof(CopyToFolderCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand MoveToFolderCommand = new(nameof(MoveToFolderCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand RemoveCommand = new(nameof(RemoveCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand RenameCommand = new(nameof(RenameCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new(nameof(OpenDestinationFolderCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new(nameof(OpenExternalAppDialogCommand), typeof(ThumbnailListView));
        public static readonly RoutedCommand PlaylistMarkCommand = new(nameof(PlaylistMarkCommand), typeof(ThumbnailListView));

        private readonly ThumbnailListItemCommandResource _commandResource;

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Return));
            ////OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            CutCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            PlaylistMarkCommand.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenBookCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CutCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RemoveCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RenameCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PlaylistMarkCommand));
        }

        #endregion


        private void Initialize()
        {
            this.Source.VisibleElement = this;

            _vm = new ThumbnailListViewModel(this.Source);

            _vm.CollectionChanging +=
                (s, e) => ViewModel_CollectionChanging(s, e);

            _vm.CollectionChanged +=
                (s, e) => ViewModel_CollectionChanged(s, e);

            _vm.ViewItemsChanged +=
                (s, e) => ViewModel_ViewItemsChanged(s, e);

            this.ThumbnailListBox.ManipulationBoundaryFeedback +=
                _vm.Model.ScrollViewer_ManipulationBoundaryFeedback;

            this.ThumbnailListBox.SelectionChanged +=
                ThumbnailListBox_SelectionChanged;

            this.Root.DataContext = _vm;
        }


        private void ViewModel_CollectionChanging(object? sender, EventArgs e)
        {
            _isFrozen = true;
        }

        private void ViewModel_CollectionChanged(object? sender, EventArgs e)
        {
            _isFrozen = false;
        }

        private void ViewModel_ViewItemsChanged(object? sender, ViewItemsChangedEventArgs e)
        {
            if (!this.IsVisible) return;

            ScrollIntoViewItems(e.ViewItems, e.Direction);
        }

        private void UpdateThumbnailListLayout(bool withLoadThumbnails)
        {
            ScrollIntoViewFixed(this.ThumbnailListBox.SelectedIndex);

            // 必要であればサムネイル要求を行う
            if (withLoadThumbnails || _isThumbnailDirty)
            {
                if (this.ThumbnailListBox.SelectedIndex >= 0)
                {
                    _isThumbnailDirty = false;
                    LoadThumbnails(+1);
                }
                else
                {
                    _isThumbnailDirty = true;
                }
            }
        }

        /// <summary>
        /// スライダー操作によるScrollIntoView
        /// </summary>
        private void ScrollIntoViewFixed(int index)
        {
            if (_listPanel == null) return;
            if (!this.IsVisible) return;
            if (!Config.Current.FilmStrip.IsEnabled) return;

            if (Config.Current.FilmStrip.IsSelectedCenter)
            {
                ScrollIntoViewIndexCenter(index);
            }
            else
            {
                this.ThumbnailListBox.Width = double.NaN;
                ScrollIntoViewIndex(this.ThumbnailListBox.SelectedIndex);
            }

            UpdateThumbnailListBoxAlign();
        }

        private void UpdateThumbnailListBoxAlign()
        {
            if (this.ThumbnailListBox.Width > this.Root.ActualWidth)
            {
                if (this.ThumbnailListBox.SelectedIndex <= 0)
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (this.ThumbnailListBox.SelectedIndex >= this.ThumbnailListBox.Items.Count - 1)
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        /// <summary>
        /// 項目を中央表示するScrollIntoView
        /// </summary>
        private void ScrollIntoViewIndexCenter(int index)
        {
            if (index < 0) return;

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox) == ScrollUnit.Pixel);

            // 項目の幅 取得
            double itemWidth = GetItemWidth();
            if (itemWidth <= 0.0) return;

            // 表示領域の幅
            double panelWidth = this.Root.ActualWidth;

            // 表示項目数を計算 (なるべく奇数)
            int itemsCount = (int)(panelWidth / itemWidth) / 2 * 2 + 3;
            if (itemsCount < 1) itemsCount = 1;

            // 表示先頭項目
            int topIndex = index - itemsCount / 2;
            if (topIndex < 0) topIndex = 0;

            // 少項目数補正
            var totalCount = this.ThumbnailListBox.Items.Count;
            if (totalCount < itemsCount)
            {
                itemsCount = totalCount;
                topIndex = 0;
            }

            // ListBoxの幅を表示項目数にあわせる
            this.ThumbnailListBox.Width = itemWidth * itemsCount + 18; // TODO: 余裕が必要？

            // 表示項目先頭指定
            var horizontalOffset = topIndex * itemWidth;
            _listPanel?.SetHorizontalOffset(horizontalOffset);
        }

        /// <summary>
        /// 項目が固定幅であることを前提とした高速ScrollIntoView
        /// </summary>
        private void ScrollIntoViewIndex(int index)
        {
            if (_listPanel is null) return;
            if (index < 0) return;

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox) == ScrollUnit.Pixel);

            // 項目の幅 取得
            double itemWidth = GetItemWidth();
            if (itemWidth <= 0.0) return;

            var panelWidth = Math.Min(this.Root.ActualWidth - (_listPanel.Margin.Left + _listPanel.Margin.Right), _listPanel.ActualWidth);

            var a0 = _listPanel.HorizontalOffset;
            var a1 = a0 + panelWidth;

            var x0 = itemWidth * index;
            var x1 = x0 + itemWidth;

            var x = a0;

            if (a1 < x1)
            {
                x = Math.Max(x0 - (panelWidth - itemWidth), 0.0);
            }

            if (x0 < a0)
            {
                x = x0;
            }

            if (x != a0)
            {
                _listPanel.SetHorizontalOffset(x);
                _listPanel.UpdateLayout();
            }
        }

        /// <summary>
        /// 指定ページのScrollIntoView
        /// </summary>
        private void ScrollIntoViewItems(List<Page> items, int direction)
        {
            if (_vm == null) return;
            if (!this.ThumbnailListBox.IsLoaded) return;
            if (_vm.Model.Items == null) return;
            if (_vm.Model.IsItemsDirty) return;
            if (!this.IsVisible) return;

            if (items.Count == 0)
            {
            }
            else if (items.Count == 1)
            {
                ScrollIntoView(items.First());
            }
            else
            {
                ScrollIntoView(items.Last());
                ScrollIntoView(items.First());
            }
        }

        private void ScrollIntoView(object item)
        {
            //// Debug.WriteLine($"> ScrollInoView: {item}");
            var index = this.ThumbnailListBox.Items.IndexOf(item);
            ScrollIntoViewIndex(index);
        }

        /// <summary>
        /// 項目の幅を取得
        /// </summary>
        private double GetItemWidth()
        {
            if (_listPanel == null || _listPanel.Children.Count <= 0) return 0.0;

            return ((ListBoxItem)_listPanel.Children[0]).ActualWidth;
        }


        // サムネ更新。表示されているページのサムネの読み込み要求
        private void LoadThumbnails(int direction)
        {
            if (_vm == null) return;
            if (_isFrozen) return;

            if (!this.Root.IsVisible || !this.ThumbnailListBox.IsVisible || _listPanel == null || _listPanel.Children.Count <= 0)
            {
                _vm.CancelThumbnailRequest();
                return;
            }

            if (this.ThumbnailListBox.SelectedIndex < 0)
            {
                return;
            }

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox) == ScrollUnit.Pixel);

            var itemWidth = GetItemWidth();
            if (itemWidth <= 0.0) return; // 項目の準備ができていない？
            var start = (int)(_listPanel.HorizontalOffset / itemWidth);
            var count = (int)(_listPanel.ViewportWidth / itemWidth) + 1;

            // タイミングにより計算値が不正になることがある対策
            // 再現性が低い
            if (count < 0)
            {
                Debug.WriteLine($"Error Value!: {count}");
                return;
            }

            _vm.RequestThumbnail(start, count, 2, direction);
        }

        private void MoveSelectedIndex(int delta)
        {
            if (_listPanel == null || _vm is null || _vm.Model.SelectedIndex < 0) return;

            if (_listPanel.FlowDirection == FlowDirection.RightToLeft)
                delta = -delta;

            _vm.MoveSelectedIndex(delta);
        }


        #region ThunbnailList event func

        private void ThumbnailListArea_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateThumbnailListLayout(false);
        }

        private void ThumbnailListBox_Loaded(object? sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBoxPanel_Loaded(object? sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            _listPanel = sender as VirtualizingStackPanel;
            UpdateThumbnailListLayout(true);
        }

        // リストボックスのドラッグ機能を無効化する
        private void ThumbnailListBox_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ThumbnailListBox.IsMouseCaptured)
            {
                MouseInputHelper.ReleaseMouseCapture(this, this.ThumbnailListBox);
            }
        }

        private void ThumbnailListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right);
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBoxPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                // 決定
                if (e.Key == Key.Return)
                {
                    BookOperation.Current.JumpPage(this, ThumbnailListBox.SelectedItem as Page);
                }
                // 左右スクロールは自前で実装
                else if (e.Key == Key.Right)
                {
                    MoveSelectedIndex(+1);
                }
                else if (e.Key == Key.Left)
                {
                    MoveSelectedIndex(-1);
                }

                e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return);
            }
        }

        private async void ThumbnailListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                await Task.Yield();
                UpdateThumbnailListLayout(true);
                _listPanel?.UpdateLayout();
                FocusSelectedItem();
            }
        }

        public void FocusSelectedItem()
        {
            if (_vm is null) return;
            if (this.ThumbnailListBox.SelectedIndex < 0) this.ThumbnailListBox.SelectedIndex = 0;
            if (this.ThumbnailListBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            ScrollIntoViewIndex(this.ThumbnailListBox.SelectedIndex);

            // フォーカスを移動
            if (_vm.Model.IsFocusAtOnce && this.ThumbnailListBox.IsLoaded)
            {
                var listBoxItem = (ListBoxItem)(this.ThumbnailListBox.ItemContainerGenerator.ContainerFromIndex(this.ThumbnailListBox.SelectedIndex));
                var isFocused = listBoxItem?.Focus();
                _vm.Model.IsFocusAtOnce = isFocused != true;
            }
        }

        private void ThumbnailListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;
            int delta = -_mouseWheelDelta.NotchCount(e);
            if (delta != 0)
            {
                _vm.MoveWheel(delta, PageSlider.Current.IsSliderDirectionReversed);
            }
            e.Handled = true;
        }

        private void ThumbnailListBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (_vm == null) return;
            this.ThumbnailListBox.UpdateLayout();
            UpdateThumbnailListLayout(true);
            _vm.Model.IsItemsDirty = false;
        }

        private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateThumbnailListLayout(false);
        }

        // スクロールしたらサムネ更新
        private void ThumbnailList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_listPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnails(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // 履歴項目決定
        private void ThumbnailListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.Content is Page page)
            {
                BookOperation.Current.JumpPage(this, page);
            }
        }

        // ContextMenu
        private void ThumbnailListItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is not ListBoxItem container)
            {
                return;
            }

            if (container.Content is not Page item)
            {
                return;
            }

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            if (item.IsBook)
            {
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.OpenAsBook"), Command = OpenBookCommand });
                contextMenu.Items.Add(new Separator());
            }

            var listBox = this.ThumbnailListBox;
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Open"), Command = OpenCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.AddToPlaylist"), Command = PlaylistMarkCommand, IsChecked = _commandResource.PlaylistMark_IsChecked(listBox) });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Explorer"), Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(_commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Cut"), Command = CutCommand });
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Copy"), Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@PageListItem.Menu.CopyToFolder"), _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@PageListItem.Menu.MoveToFolder"), _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Delete"), Command = RemoveCommand });
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PageListItem.Menu.Rename"), Command = RenameCommand });
        }

        #endregion
    }
}
