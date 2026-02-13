using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// PageListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListBox : UserControl, IPageListPanel, IDisposable
    {
        public static readonly string DragDropFormat = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), nameof(PageListBox));

        private readonly PageListBoxViewModel _vm;
        private ListBoxThumbnailLoader? _thumbnailLoader;
        private PageThumbnailJobClient? _jobClient;
        private bool _disposedValue = false;
        private readonly DisposableCollection _disposables = new();

        static PageListBox()
        {
            InitializeCommandStatic();
        }

        public PageListBox(PageListBoxViewModel vm)
        {
            InitializeComponent();

            _commandResource = new PageListItemCommandResource(vm.DetailToolTip);
            InitializeCommand();

            _vm = vm;
            _vm.CollectionChanged += ViewModel_CollectionChanged;

            this.DataContext = _vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
            this.ListBox.PreviewMouseUpWithSelectionChanged += PageList_PreviewMouseUpWithSelectionChanged;

            this.Loaded += PageListBox_Loaded;
            this.Unloaded += PageListBox_Unloaded;
            this.MouseLeave += PageListBox_MouseLeave;

            _disposables.Add(PageFrameBoxPresenter.Current.SubscribeIsSortBusyChanged(PageFrameBox_IsSortBusyChanged));
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_jobClient != null)
                    {
                        _disposables.Dispose();
                        _jobClient.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void PageFrameBox_IsSortBusyChanged(object? sender, IsSortBusyChangedEventArgs e)
        {
            this.BusyFade.IsBusy = e.IsSortBusy;
            if (e.IsSortBusy)
            {
                RenameManager.GetRenameManager(this)?.CloseAll(false, false);
            }
        }

        private void ViewModel_CollectionChanged(object? sender, EventArgs e)
        {
            this.ListBox.UpdateLayout();

            _thumbnailLoader?.Load();

            if (this.ListBox.IsFocused)
            {
                FocusSelectedItem(true);
            }
        }

        #region IPageListPanel support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisible => PageList.Current.IsThumbnailVisible;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion


        #region Commands

        public static readonly RoutedCommand OpenCommand = new(nameof(OpenCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenBookCommand = new(nameof(OpenBookCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new(nameof(OpenExplorerCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExternalAppCommand = new(nameof(OpenExternalAppCommand), typeof(PageListBox));
        public static readonly RoutedCommand CutCommand = new(nameof(CutCommand), typeof(PageListBox));
        public static readonly RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(PageListBox));
        public static readonly RoutedCommand CopyToFolderCommand = new(nameof(CopyToFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand MoveToFolderCommand = new(nameof(MoveToFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand RemoveCommand = new(nameof(RemoveCommand), typeof(PageListBox));
        public static readonly RoutedCommand RenameCommand = new(nameof(RenameCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new(nameof(OpenDestinationFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new(nameof(OpenExternalAppDialogCommand), typeof(PageListBox));
        public static readonly RoutedCommand PlaylistMarkCommand = new(nameof(PlaylistMarkCommand), typeof(PageListBox));

        private readonly PageListItemCommandResource _commandResource;

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Return));
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            CutCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            PlaylistMarkCommand.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Exec, Open_CanExec));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenBookCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CutCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RemoveCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RenameCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PlaylistMarkCommand));
        }

        #endregion

        /// <summary>
        /// ページを開くコマンド
        /// </summary>
        private void Open_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as ListBox)?.SelectedItem is Page;
        }

        private void Open_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is not Page page) return;
            _vm.Model.MoveTo(page);
        }

        private void PageListBox_Loaded(object? sender, RoutedEventArgs e)
        {
            _vm.Loaded();
            _vm.ViewItemsChanged += ViewModel_ViewItemsChanged;

            _jobClient = new PageThumbnailJobClient("PageList", JobCategories.PageThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);
            _thumbnailLoader.Load();

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;

            FocusSelectedItem(false);
        }

        private void PageListBox_Unloaded(object? sender, RoutedEventArgs e)
        {
            _vm.Unloaded();
            _vm.ViewItemsChanged -= ViewModel_ViewItemsChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;

            _jobClient?.Dispose();
        }

        private void PageListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            _vm.Model.MoveEnd();
        }

        private void PanelListItemProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        private void ViewModel_ViewItemsChanged(object? sender, ViewItemsChangedEventArgs e)
        {
            UpdateViewItems(e.ViewItems, e.Direction);
        }

        private void UpdateViewItems()
        {
            if (_vm.Model.ViewItems == null) return;

            UpdateViewItems(_vm.Model.ViewItems, 0);
        }

        private void UpdateViewItems(List<Page> items, int direction)
        {
            if (!this.ListBox.IsLoaded) return;
            if (_vm.Model.Items == null) return;
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
            ////Debug.WriteLine($"#### PL:ScrollIntoView: {item}");
            this.ListBox.ScrollIntoView(item);
            this.ListBox.UpdateLayout();
        }

        public void FocusSelectedItem(bool isForce)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            UpdateViewItems();

            if (isForce || _vm.FocusAtOnce)
            {
                _vm.FocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        // 選択項目変更
        private void PageList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _vm.Model.SelectedItems = this.ListBox.SelectedItems.Cast<Page>().ToList();
        }

        // マウス入力
        private void PageList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _vm.Model.ResetMoveFlag();
        }

        // リストのキ入力
        private void PageList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (this.ListBox.IsSimpleTextSearchEnabled)
            {
                KeyExGesture.AddFilter(KeyExGestureFilter.TextKey);
            }

            var page = this.ListBox.SelectedItem as Page;

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    // 現在ブックの上の階層に移動
                    BookHub.Current.RequestLoadParent(this);
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    // 選択ブックに移動
                    if (page != null && page.PageType.IsFolder())
                    {
                        BookHub.Current.RequestLoad(this, page.ArchiveEntry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
                    }
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    // 直前のページに移動
                    PageHistory.Current.MoveToPrevious();
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    // 直後のページに移動
                    PageHistory.Current.MoveToNext();
                    e.Handled = true;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return && page is not null)
                {
                    // 項目決定
                    // NOTE: コマンドバインドが優先されるためこの処理は実行されない
                    _vm.Model.MoveTo(page);
                    e.Handled = true;
                }
            }

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        private void PageList_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.Model.MoveEnd();
        }

        private async void PageList_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                FocusSelectedItem(false);
            }
        }

        private void PageList_TargetUpdated(object? sender, DataTransferEventArgs e)
        {
            UpdateViewItems();
        }

        // 項目クリック (複数選択解除)
        private void PageList_PreviewMouseUpWithSelectionChanged(object? sender, MouseButtonEventArgs e)
        {
            if (this.ListBox.SelectedItems.Count != 1) return;

            if (this.ListBox.SelectedItem is Page page)
            {
                ClickToMove(page);
            }
        }

        // 項目クリック
        private void PageListItem_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem { Content: Page page })
            {
                ClickToMove(page);
            }
        }

        private void ClickToMove(Page page)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            _vm.Model.MoveTo(page);
        }

        private void PageListItem_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            _vm.Model.MoveEnd();
        }

        // 項目ダブルクリック
        private void PageListItem_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.Content is Page page && page.PageType.IsFolder())
            {
                BookHub.Current.RequestLoad(this, page.ArchiveEntry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
                e.Handled = true;
            }
        }


        private void PageListItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
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

            var listBox = this.ListBox;
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


        #region DragDrop

        public async ValueTask DragStartBehavior_DragBeginAsync(object? sender, Windows.DragStartEventArgs e, CancellationToken token)
        {
            var pages = this.ListBox.SelectedItems.Cast<Page>().Where(e => e.PageType != PageType.Empty).ToList();
            if (!pages.Any())
            {
                e.Cancel = true;
                return;
            }

            var isSuccess = await ClipboardUtility.TrySetDataAsync(e.Data, pages, token);
            if (!isSuccess)
            {
                e.Cancel = true;
                return;
            }

            // 全てのファイルがファイルシステムであった場合のみ
            if (pages.All(p => p.ArchiveEntry.IsFileSystem))
            {
                // 右クリックドラッグでファイル移動を許可
                if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                {
                    e.AllowedEffects |= DragDropEffects.Move;
                }

                // TODO: ドラッグ終了時にファイル移動の整合性を取る必要がある。
                // しっかり実装するならページのファイルシステムの監視が必要になる。ファイルの追加削除が自動的にページに反映するように。

                // ひとまずドラッグ完了後のページ削除を限定的に行う。
                e.DragEndAction = () => BookOperation.Current.ValidatePages(pages);
            }
        }

        #endregion

        #region UI Accessor

        public List<Page>? GetItems()
        {
            return _vm.Model.Items?.ToList();
        }

        public List<Page> GetSelectedItems()
        {
            // ListBox 生成直後でプロパティが不定の場合、モデルデータの値を返す
            if (this.ListBox.SelectedItem is null)
            {
                if (_vm.Model.SelectedItem is null)
                {
                    return new();
                }
                else
                {
                    return new() { _vm.Model.SelectedItem };
                }
            }

            return this.ListBox.SelectedItems.Cast<Page>().ToList();
        }

        public void SetSelectedItems(IEnumerable<Page> selectedItems)
        {
            var sources = GetItems();
            if (sources is null) return;

            var items = selectedItems?.Intersect(sources).ToList();
            this.ListBox.SetSelectedItems(items);
            this.ListBox.ScrollItemsIntoView(items);

            // ListBox 生成直後でプロパティが不定の場合、モデルデータにも反映
            // 個数 0 は未初期化とみなされるらしい
            if (items is null || items.Count == 0)
            {
                _vm.Model.SelectedItem = null;
            }
        }

        #endregion UI Accessor
    }


    /// <summary>
    /// Page,PageNameFormat から表示ページ名を取得
    /// </summary>
    public class PageNameFormatConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Page page && values[1] is PageNameFormat format)
            {
                return page.GetDisplayName(format);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageToNoteConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Page page)
            {
                if (page.LastWriteTime == default && page.Length == 0) return null;

                var timeString = page.LastWriteTime.ToFormatString();
                var sizeString = FileSizeToStringConverter.ByteToDisplayString(page.Length);
                return timeString + (string.IsNullOrEmpty(sizeString) ? "" : "   " + sizeString);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ArchivePageなら表示
    /// </summary>
    public class ArchivePageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Page page && page.PageType.IsFolder()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ArchivePageToFolderImageSourceConverter : IValueConverter
    {
        public ImageSource? FolderImageSource { get; set; }
        public ImageSource? FolderZipImageSource { get; set; }
        public ImageSource? FolderMediaImageSource { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Page page && page.PageType.IsFolder())
            {
                if (page.ArchiveEntry.IsDirectory)
                {
                    return FolderImageSource;
                }
                else if (page.ArchiveEntry.IsMedia())
                {
                    return FolderMediaImageSource;
                }
                else
                {
                    return FolderZipImageSource;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
