//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// FilmStripView.xaml の相互作用ロジック
    /// </summary>
    [LocalDebug]
    public partial class FilmStripView : UserControl, IVisibleElement
    {
        #region DependencyProperties

        public FilmStrip Source
        {
            get { return (FilmStrip)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FilmStrip), typeof(FilmStripView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilmStripView control)
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
            DependencyProperty.RegisterReadOnly("IsContentVisible", typeof(bool), typeof(FilmStripView), new PropertyMetadata(false));

        public static readonly DependencyProperty IsContentVisibleProperty = IsContentVisiblePropertyKey.DependencyProperty;


        public bool IsBackgroundOpacityEnabled
        {
            get { return (bool)GetValue(IsBackgroundOpacityEnabledProperty); }
            set { SetValue(IsBackgroundOpacityEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsBackgroundOpacityEnabledProperty =
            DependencyProperty.Register("IsBackgroundOpacityEnabled", typeof(bool), typeof(FilmStripView), new PropertyMetadata(false));


        public bool IsFocusRequest
        {
            get { return (bool)GetValue(IsFocusRequestProperty); }
            set { SetValue(IsFocusRequestProperty, value); }
        }

        public static readonly DependencyProperty IsFocusRequestProperty =
            DependencyProperty.Register(nameof(IsFocusRequest), typeof(bool), typeof(FilmStripView),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsFocusRequest_Changed));

        private static void IsFocusRequest_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilmStripView control && (bool)e.NewValue)
            {
                control.FocusAtOnce();
                control.Dispatcher.BeginInvoke(() => control.IsFocusRequest = false);
            }
        }

        #endregion

        // ViewModel
        private FilmStripViewModel? _vm;

        // フィルムストリップのパネルコントロール
        private VirtualizingStackPanel? _listPanel;

        // 項目を中央表示するためのTransform
        private TranslateTransform _layoutTransform = new();

        private bool _isThumbnailDirty;

        // サムネイル更新要求を拒否する
        private bool _isFrozen;

        private readonly MouseWheelDelta _mouseWheelDelta = new();

        // SelectionChanged での変更を識別するための前回選択インデックス
        private int _lastSelectedIndex = -1;


        static FilmStripView()
        {
            InitializeCommandStatic();
        }

        public FilmStripView()
        {
            InitializeComponent();

            _commandResource = new FilmStripItemCommandResource(FilmStripItemDetailToolTip.Current);
            InitializeCommand();

            this.FilmStripBox.HorizontalAlignment = HorizontalAlignment.Center;
            this.FilmStripBox.RenderTransform = _layoutTransform;

            this.Root.IsVisibleChanged +=
                (s, e) => this.IsContentVisible = (bool)e.NewValue;
        }


        #region Commands

        public static readonly RoutedCommand OpenCommand = new(nameof(OpenCommand), typeof(FilmStripView));
        public static readonly RoutedCommand OpenBookCommand = new(nameof(OpenBookCommand), typeof(FilmStripView));
        public static readonly RoutedCommand OpenExplorerCommand = new(nameof(OpenExplorerCommand), typeof(FilmStripView));
        public static readonly RoutedCommand OpenExternalAppCommand = new(nameof(OpenExternalAppCommand), typeof(FilmStripView));
        public static readonly RoutedCommand CutCommand = new(nameof(CutCommand), typeof(FilmStripView));
        public static readonly RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(FilmStripView));
        public static readonly RoutedCommand CopyToFolderCommand = new(nameof(CopyToFolderCommand), typeof(FilmStripView));
        public static readonly RoutedCommand MoveToFolderCommand = new(nameof(MoveToFolderCommand), typeof(FilmStripView));
        public static readonly RoutedCommand RemoveCommand = new(nameof(RemoveCommand), typeof(FilmStripView));
        public static readonly RoutedCommand RenameCommand = new(nameof(RenameCommand), typeof(FilmStripView));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new(nameof(OpenDestinationFolderCommand), typeof(FilmStripView));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new(nameof(OpenExternalAppDialogCommand), typeof(FilmStripView));
        public static readonly RoutedCommand PlaylistMarkCommand = new(nameof(PlaylistMarkCommand), typeof(FilmStripView));

        private readonly FilmStripItemCommandResource _commandResource;

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
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenBookCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CutCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RemoveCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RenameCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.FilmStripBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PlaylistMarkCommand));
        }

        #endregion


        private void Initialize()
        {
            this.Source.VisibleElement = this;

            _vm = new FilmStripViewModel(this.Source, FilmStripItemDetailToolTip.Current);

            _vm.CollectionChanging +=
                (s, e) => ViewModel_CollectionChanging(s, e);

            _vm.CollectionChanged +=
                (s, e) => ViewModel_CollectionChanged(s, e);

            _vm.ViewItemsChanged +=
                (s, e) => ViewModel_ViewItemsChanged(s, e);

            this.FilmStripBox.ManipulationBoundaryFeedback +=
                _vm.Model.ScrollViewer_ManipulationBoundaryFeedback;

            this.FilmStripBox.GotFocus += FilmStripBox_GotFocus;

            this.Root.DataContext = _vm;
        }


        private void ViewModel_CollectionChanging(object? sender, EventArgs e)
        {
            _isFrozen = true;
        }

        private void ViewModel_CollectionChanged(object? sender, EventArgs e)
        {
            _isFrozen = false;
            _lastSelectedIndex = -1;
            _isThumbnailDirty = true;
        }

        private void ViewModel_ViewItemsChanged(object? sender, ViewItemsChangedEventArgs e)
        {
            if (!this.IsVisible) return;

            ScrollIntoViewItems(e.ViewItems, e.Direction);
        }

        private void UpdateFilmStripLayout(bool withLoadThumbnails)
        {
            ScrollIntoViewFixed(this.FilmStripBox.SelectedIndex);

            // 必要であればサムネイル要求を行う
            if (withLoadThumbnails || _isThumbnailDirty)
            {
                if (this.FilmStripBox.SelectedIndex >= 0)
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

            // レイアウト補正をリセット
            _layoutTransform.X = 0.0;

            // 中央表示モードなら中央表示、そうでなければ通常表示
            if (Config.Current.FilmStrip.IsSelectedCenter)
            {
                ScrollIntoViewIndexCenter(index);
            }
            else
            {
                ScrollIntoViewIndex(this.FilmStripBox.SelectedIndex);
            }
        }

        /// <summary>
        /// 項目を中央表示するScrollIntoView
        /// </summary>
        private void ScrollIntoViewIndexCenter(int index)
        {
            if (index < 0) return;
            if (_listPanel is null) return;

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.FilmStripBox) == ScrollUnit.Pixel);

            double itemWidth = GetItemWidth();
            if (itemWidth <= 0.0) return;

            // Set the horizontal offset to center the item
            var viewWidth = _listPanel.ActualWidth;
            var itemLeft = itemWidth * index;
            var horizontalOffset = Math.Floor(itemLeft + itemWidth * 0.5 - viewWidth * 0.5);
            _listPanel.SetHorizontalOffset(horizontalOffset);

            // Set the panel layout to center the item
            var scrollableWidth = Math.Max(0, itemWidth * this.FilmStripBox.Items.Count - viewWidth);
            var layoutX = 0.0;
            if (horizontalOffset < 0)
            {
                layoutX = -horizontalOffset;
            }
            else if (horizontalOffset > scrollableWidth)
            {
                layoutX = (scrollableWidth - horizontalOffset);
            }
            _layoutTransform.X = layoutX;

            LocalDebug.WriteLine($"scrollableWidth={scrollableWidth}, offset={horizontalOffset}, layoutX={layoutX}");
        }

        /// <summary>
        /// ListBoxItem での ScrollIntoView
        /// </summary>
        /// <remarks>
        /// 項目が中央表示モードのときに、項目が完全に表示されるようにスクロール位置を調整する
        /// </remarks>
        private void ScrollIntoViewLayout(ListBoxItem item)
        {
            if (Config.Current.FilmStrip.IsSelectedCenter)
            {
                var pos = item.TranslatePoint(new Point(0, 0), this);
                if (pos.X + item.ActualWidth > this.ActualWidth)
                {
                    _layoutTransform.X = Math.Max(_layoutTransform.X - (pos.X + item.ActualWidth - this.ActualWidth), 0.0);
                }
                if (pos.X < 0)
                {
                    _layoutTransform.X = Math.Min(_layoutTransform.X - pos.X, 0.0);
                }
            }
        }

        /// <summary>
        /// 項目が固定幅であることを前提とした高速ScrollIntoView
        /// </summary>
        private void ScrollIntoViewIndex(int index)
        {
            if (_listPanel is null) return;
            if (index < 0) return;

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.FilmStripBox) == ScrollUnit.Pixel);

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
            if (!this.FilmStripBox.IsLoaded) return;
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
            var index = this.FilmStripBox.Items.IndexOf(item);
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

            if (!this.Root.IsVisible || !this.FilmStripBox.IsVisible || _listPanel == null || _listPanel.Children.Count <= 0)
            {
                _vm.CancelThumbnailRequest();
                return;
            }

            if (this.FilmStripBox.SelectedIndex < 0)
            {
                return;
            }

            Debug.Assert(VirtualizingStackPanel.GetScrollUnit(this.FilmStripBox) == ScrollUnit.Pixel);

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

        private void FilmStripArea_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateFilmStripLayout(false);
        }

        private void FilmStripBox_Loaded(object? sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void FilmStripBoxPanel_Loaded(object? sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            _listPanel = sender as VirtualizingStackPanel;
            UpdateFilmStripLayout(true);
        }

        // リストボックスのドラッグ機能を無効化する
        private void FilmStripBox_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.FilmStripBox.IsMouseCaptured)
            {
                MouseInputHelper.ReleaseMouseCapture(this, this.FilmStripBox);
            }
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制 (不要かも)
        private void FilmStripBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right);
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void FilmStripBoxPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Return)
            {
                // 決定
                BookOperation.Current.JumpPage(this, FilmStripBox.SelectedItem as Page);
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Down)
            {
                // 上下キー無効
                e.Handled = true;
            }
        }

        private void FilmStripBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ListBoxItem container)
            {
                LocalDebug.WriteLine($"ListBoxItem Focused: {container.DataContext}");
                if (Config.Current.FilmStrip.IsSelectedCenter)
                {
                    // 項目中央表示の場合にキーボード操作によるフォーカス項目が画面内に表示されるようにする
                    // 通常表示の場合はListBoxの機能で行われるため不要
                    ScrollIntoViewLayout(container);
                }
            }
        }

        private void FilmStripBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    UpdateFilmStripLayout(true);
                    _listPanel?.UpdateLayout();
                    FocusSelectedItem();
                }, DispatcherPriority.Input);
            }
        }

        public void FocusSelectedItem(bool focus = false)
        {
            if (_vm is null) return;
            if (this.FilmStripBox.SelectedIndex < 0) this.FilmStripBox.SelectedIndex = 0;
            if (this.FilmStripBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            ScrollIntoViewIndex(this.FilmStripBox.SelectedIndex);

            // フォーカスを移動
            if (focus)
            {
                var listBoxItem = (ListBoxItem)(this.FilmStripBox.ItemContainerGenerator.ContainerFromIndex(this.FilmStripBox.SelectedIndex));
                var isFocused = listBoxItem?.Focus();
            }
        }

        private void FocusAtOnce()
        {
            FocusTools.FocusAtOnce(this.FilmStripBox, () => FocusSelectedItem(true));
        }

        private void FilmStripBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;
            int delta = -_mouseWheelDelta.NotchCount(e);
            if (delta != 0)
            {
                _vm.MoveWheel(delta, PageSlider.Current.IsSliderDirectionReversed);
            }
            e.Handled = true;
        }

        private void FilmStripBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (_vm == null) return;
            this.FilmStripBox.UpdateLayout();
            UpdateFilmStripLayout(true);
            _vm.Model.IsItemsDirty = false;
        }

        private void FilmStripBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // NOTE: SelectedIndex が変化したときだけに反応する
            int currentId = this.FilmStripBox.SelectedIndex;
            if (_lastSelectedIndex != currentId)
            {
                _lastSelectedIndex = currentId;
                UpdateFilmStripLayout(false);
            }
        }

        // スクロールしたらサムネ更新
        private void FilmStrip_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_listPanel != null && this.FilmStripBox.Items.Count > 0)
            {
                LoadThumbnails(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // 履歴項目決定
        private void FilmStripItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if ((sender as ListBoxItem)?.Content is Page page)
            {
                BookOperation.Current.JumpPage(this, page);
            }
        }

        // ContextMenu
        private void FilmStripItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
                contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.OpenAsBook"), Command = OpenBookCommand });
                contextMenu.Items.Add(new Separator());
            }

            var listBox = this.FilmStripBox;
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Open"), Command = OpenCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.AddToPlaylist"), Command = PlaylistMarkCommand, IsChecked = _commandResource.PlaylistMark_IsChecked(listBox) });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Explorer"), Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(TextResources.GetString("PageListItem.Menu.OpenExternalApp"), _commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Cut"), Command = CutCommand });
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Copy"), Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(TextResources.GetString("PageListItem.Menu.CopyToFolder"), _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(TextResources.GetString("PageListItem.Menu.MoveToFolder"), _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Delete"), Command = RemoveCommand });
            contextMenu.Items.Add(new MenuItem() { Header = TextResources.GetString("PageListItem.Menu.Rename"), Command = RenameCommand });
        }

        #endregion
    }
}
