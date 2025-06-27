using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using NeeView.Windows;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// PlaylistListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistListBox : UserControl, IPageListPanel, IDisposable
    {
        private readonly PlaylistListBoxViewModel _vm;
        private readonly PlaylistListBoxInsertDropAssist _dropAssist;
        private ListBoxThumbnailLoader? _thumbnailLoader;
        private PageThumbnailJobClient? _jobClient;
        private bool _focusRequest;
        private PlaylistItem? _clickItem;

        static PlaylistListBox()
        {
            InitializeCommandStatic();
        }

        public PlaylistListBox(PlaylistListBoxViewModel vm)
        {
            InitializeComponent();
            InitializeCommand();

            _vm = vm;
            this.DataContext = vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
            this.ListBox.PreviewMouseUpWithSelectionChanged += PlaylistListBox_PreviewMouseUpWithSelectionChanged;

            this.Loaded += PlaylistListBox_Loaded;
            this.Unloaded += PlaylistListBox_Unloaded;

            _dropAssist = new PlaylistListBoxInsertDropAssist(this.ListBox, _vm);
            this.ListBox.PreviewDragEnter += ListBox_PreviewDragEnter;
            this.ListBox.PreviewDragLeave += ListBox_PreviewDragLeave;
            this.ListBox.PreviewDragOver += ListBox_PreviewDragOver;
            this.ListBox.DragOver += ListBox_DragOver;
            this.ListBox.Drop += ListBox_Drop;
        }


        #region Commands
        public readonly static RoutedCommand AddCommand = new(nameof(AddCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand MoveUpCommand = new(nameof(MoveUpCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand MoveDownCommand = new(nameof(MoveDownCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand OpenCommand = new(nameof(OpenCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand OpenSourceCommand = new(nameof(OpenSourceCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand RenameCommand = new(nameof(RenameCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand RemoveCommand = new(nameof(RemoveCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand MoveToAnotherCommand = new(nameof(MoveToAnotherCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new(nameof(OpenExplorerCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand OpenExternalAppCommand = new(nameof(OpenExternalAppCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand CutCommand = new(nameof(CutCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand CopyToFolderCommand = new(nameof(CopyToFolderCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand MoveToFolderCommand = new(nameof(MoveToFolderCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new(nameof(OpenDestinationFolderCommand), typeof(PlaylistListBox));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new(nameof(OpenExternalAppDialogCommand), typeof(PlaylistListBox));

        private readonly PlaylistItemCommandResource _commandResource = new();

        private static void InitializeCommandStatic()
        {
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            CutCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.CommandBindings.Add(new CommandBinding(AddCommand, AddCommand_Execute, AddCommand_CanExecute));
            this.CommandBindings.Add(new CommandBinding(MoveUpCommand, MoveUpCommand_Execute, MoveUpCommand_CanExecute));
            this.CommandBindings.Add(new CommandBinding(MoveDownCommand, MoveDownCommand_Execute, MoveDownCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, OpenCommand_Execute, OpenCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenSourceCommand, OpenSourceCommand_Execute, OpenSourceCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, RenameCommand_Execute, RenameCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, RemoveCommand_Execute, RemoveCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(MoveToAnotherCommand, MoveToAnotherCommand_Execute, MoveToAnotherCommand_CanExecute));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CutCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
        }


        private void AddCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.IsEditable;
        }

        private void AddCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            _vm.AddCurrentPage();
            ScrollIntoView();
        }

        private void MoveUpCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.CanMoveUp(GetOrderedSelectedItems(), GetViewItems());
        }

        private void MoveUpCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            _vm.MoveUp(GetOrderedSelectedItems(), GetViewItems());
            ScrollIntoView();
        }

        private void MoveDownCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.CanMoveDown(GetOrderedSelectedItems(), GetViewItems());
        }

        private void MoveDownCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            _vm.MoveDown(GetOrderedSelectedItems(), GetViewItems());
            ScrollIntoView();
        }

        private List<PlaylistItem> GetOrderedSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<PlaylistItem>().OrderBy(e => this.ListBox.Items.IndexOf(e)).ToList();
        }

        private List<PlaylistItem> GetViewItems()
        {
            return this.ListBox.Items.Cast<PlaylistItem>().ToList();
        }

        private void OpenCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ListBox.SelectedItem is not PlaylistItem item) return;
            _vm.Open(item);
        }

        private void OpenSourceCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenSourceCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ListBox.SelectedItem is not PlaylistItem item) return;
            _vm.OpenSource(item);
        }

        private void RenameCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.IsEditable;
        }

        private async void RenameCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ListBox.SelectedItem is not PlaylistItem item) return;

            var renamer = new PlaylistItemRenamer(this.ListBox, _vm.DetailToolTip, (s, e) => _vm.Rename(s, e));
            await renamer.RenameAsync(item);
        }

        private void RemoveCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.IsEditable;
        }

        private void RemoveCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<PlaylistItem>().ToList();
            _vm.Remove(items);
            ScrollIntoView();
            ////FocusSelectedItem(true);
        }

        private void MoveToAnotherCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.IsEditable;
        }

        private void MoveToAnotherCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<PlaylistItem>().ToList();
            if (e.Parameter is not string another) return;

            _vm.MoveToAnotherPlaylist(another, items);
            ScrollIntoView();
        }

        #endregion Commands

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _jobClient?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IPageListBox support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisible => _vm.IsThumbnailVisible;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> collection) => collection.OfType<IHasPage>();

        #endregion IPageListBox support

        #region DragDrop

        public async ValueTask DragStartBehavior_DragBeginAsync(object? sender, DragStartEventArgs e, CancellationToken token)
        {
            var items = this.ListBox.SelectedItems
                .Cast<PlaylistItem>()
                .ToList();

            if (!items.Any())
            {
                e.Cancel = true;
                return;
            }

            var collection = new PlaylistListBoxItemCollection(items);
            e.Data.SetData(collection);
            e.AllowedEffects |= DragDropEffects.Move;

            e.Data.SetQueryPathCollection(items.Select(x => new QueryPath(x.Path)));

            if (Config.Current.System.TextCopyPolicy != TextCopyPolicy.None)
            {
                var text = string.Join(System.Environment.NewLine, items.Select(e => e.Path));
                e.Data.SetText(text);
            }

            await Task.CompletedTask;
        }

        private void ListBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            _dropAssist.OnDragEnter(sender, e);

            ListBox_PreviewDragOver(sender, e);
            if (e.Handled) return;

            ListBox_DragOver(sender, e);
        }

        private void ListBox_PreviewDragLeave(object sender, DragEventArgs e)
        {
            _dropAssist.OnDragLeave(sender, e);
        }

        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Handled) return;

            var scrolled = DragDropHelper.AutoScroll(sender, e);
            if (scrolled)
            {
                _dropAssist.HideAdorner();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            var target = _dropAssist.OnDragOver(sender, e);

            if (!AcceptDrop(e, target))
            {
                _dropAssist.HideAdorner();
                return;
            }

            e.Handled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var target = _dropAssist.OnDrop(sender, e);

            if (!AcceptDrop(e, target))
            {
                return;
            }

            var targetItem = (target.Item as ListBoxItem)?.Content as PlaylistItem;
            var delta = target.Delta;
            if (targetItem is null)
            {
                delta = 0;
            }

            e.Handled = true;

            AppDispatcher.BeginInvoke(async () =>
            {
                var copyMaybe = e.Effects.HasFlag(DragDropEffects.Copy);
                var dropItems = await GetPlaylistItemsAsync(e, copyMaybe, CancellationToken.None);
                if (dropItems is not null)
                {
                    DropToPlaylist(sender, e, dropItems, targetItem, delta);
                }
            });
        }

        private bool AcceptDrop(DragEventArgs e, DropTargetItem target)
        {
            if (!_vm.IsEditable)
            {
                return false;
            }

            var entries = e.Data.GetData<PlaylistListBoxItemCollection>();
            if (entries is not null)
            {
                //e.DragEventArgs.Effects = Keyboard.Modifiers == ModifierKeys.Control ? DragDropEffects.Copy : DragDropEffects.Move;
                e.Effects = DragDropEffects.Move;

                if (!entries.Any())
                {
                    return false;
                }

                var destination = (target.Item as ListBoxItem)?.Content as PlaylistItem;

                if (_vm.IsGroupBy)
                {
                    if (destination is null)
                    {
                        return false;
                    }
                    if (entries.Select(x => x.Place).Any(x => x != destination.Place))
                    {
                        return false;
                    }
                    return !entries.Contains(destination);
                }
                else
                {
                    if (destination is null)
                    {
                        return _vm.Items is not null;
                    }
                    return !entries.Contains(destination);
                }
            }

            var queries = e.Data.GetQueryPathCollection();
            if (queries is not null)
            {
                e.Effects = DragDropEffects.Copy;
                return queries.Any();
            }

            var files = e.Data.GetFileDrop();
            if (files is not null)
            {
                e.Effects = DragDropEffects.Copy;
                return files.Any();
            }

            return false;
        }

        private async ValueTask<List<PlaylistItem>?> GetPlaylistItemsAsync(DragEventArgs e, bool copyMaybe, CancellationToken token)
        {
            var entries = e.Data.GetData<PlaylistListBoxItemCollection>();
            if (entries is not null)
            {
                if (copyMaybe)
                {
                    throw new NotImplementedException();
                    //return entries.Select(e => e.Clone()).ToList();
                }
                else
                {
                    return entries;
                }
            }

            var queries = e.Data.GetQueryPathCollection();
            if (queries is not null)
            {
                var paths = queries.Where(x => x.Scheme == QueryScheme.File).Select(x => x.SimplePath);
                return await CreatePlaylistItems(paths, token);
            }

            var files = e.Data.GetFileDrop();
            if (files is not null)
            {
                return await CreatePlaylistItems(files, token);
            }

            return null;
        }

        private async ValueTask<List<PlaylistItem>?> CreatePlaylistItems(IEnumerable<string> paths, CancellationToken token)
        {
            var validPaths = await _vm.ValidatePlaylistItemPath(paths, token);
            var collection = _vm.Items;
            return validPaths.Distinct().Where(e => collection is null || collection.All(x => x.Path != e)).Select(e => new PlaylistItem(e)).ToList();
        }

        private void DropToPlaylist(object? sender, DragEventArgs e, IEnumerable<PlaylistItem>? dropItems, PlaylistItem? targetItem, int delta)
        {
            if (_vm.Items is null)
            {
                return;
            }

            if (dropItems == null || !dropItems.Any())
            {
                return;
            }

            // 複数の移動では順番を維持する
            if (dropItems.Count() > 1)
            {
                // NOTE: 新規のエントリの場合は Index=-1 なのでソートされない
                dropItems = dropItems.OrderBy(e => _vm.Items.IndexOf(e)).ToList();
            }

            // Drop 実行
            foreach (var dropItem in dropItems)
            {
                var isSuccess = DropToPlaylist(dropItem, targetItem, delta);
                if (isSuccess)
                {
                    // 複数登録の場合の整列
                    targetItem = dropItem;
                    delta = +1;
                }
            }

            // フォーカス調整
            var selectedItems = dropItems.Select(e => FindPlaylistItem(e)).WhereNotNull().ToList();
            if (selectedItems.Any())
            {
                this.ListBox.SetSelectedItems(selectedItems);
            }
        }

        private bool DropToPlaylist(PlaylistItem dropItems, PlaylistItem? targetItem, int delta)
        {
            Debug.Assert(targetItem is null || delta is (-1) or (+1));

            if (dropItems == targetItem) return false;

            var index = GetDeltaNodeIndex(dropItems, targetItem, delta);
            return _vm.Drop(index, dropItems);
        }

        private PlaylistItem? FindPlaylistItem(PlaylistItem node)
        {
            var collection = _vm.Items;
            if (collection is null) return null;

            var item = collection.FirstOrDefault(e => e == node);
            if (item is not null) return item;

            return collection.FirstOrDefault(e => e.Path == node.Path);
        }

        private int GetDeltaNodeIndex(PlaylistItem dropItem, PlaylistItem? targetItem, int delta)
        {
            if (_vm.Items is null) throw new InvalidOperationException();

            if (targetItem is null) return -1;

            var index = _vm.Items.IndexOf(targetItem);

            //var direction = _vm.FolderOrder.IsDescending() ? -delta : delta;
            var direction = delta;

            var entryIndex = _vm.Items.IndexOf(dropItem);
            if (entryIndex >= 0 && entryIndex < index)
            {
                return (direction < 0 && index > 0) ? index - 1 : index;
            }
            else
            {
                return (direction > 0) ? index + 1 : index;
            }
        }

#endregion DragDrop

        private void PlaylistListBox_Loaded(object? sender, RoutedEventArgs e)
        {
            _jobClient = new PageThumbnailJobClient("Playlist", JobCategories.BookThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);
            _thumbnailLoader.Load();

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
        }


        private void PlaylistListBox_Unloaded(object? sender, RoutedEventArgs e)
        {
            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;

            _jobClient?.Dispose();
        }


        public void ScrollIntoView()
        {
            this.ListBox.ScrollSelectedItemsIntoView();
        }

        private void PanelListItemProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        public bool FocusSelectedItem(bool focus)
        {
            if (this.ListBox.SelectedIndex < 0) return false;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (focus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                return lbi?.Focus() ?? false;
            }
            else
            {
                return false;
            }
        }

        public void Refresh()
        {
            this.ListBox.Items.Refresh();
        }

        public void FocusAtOnce()
        {
            var focused = FocusSelectedItem(true);
            if (!focused)
            {
                _focusRequest = true;
            }
        }

        private void PlaylistListBox_PreviewMouseUpWithSelectionChanged(object? sender, MouseButtonEventArgs e)
        {
            if (this.ListBox.SelectedItems.Count != 1) return;

            if (this.ListBox.SelectedItem is PlaylistItem item)
            {
                ClickToOpen(item);
            }
        }

        private void PlaylistListItem_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem { Content: PlaylistItem item })
            {
                _clickItem = item;
            }
        }

        private void PlaylistListItem_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem { Content: PlaylistItem item })
            {
                if (_clickItem == item)
                {
                    ClickToOpen(item);
                }
            }
            _clickItem = null;
        }

        private void ClickToOpen(PlaylistItem item)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (!Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Open(item);
            }
        }

        private void PlaylistListItem_MouseDoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as PlaylistItem);
            if (item is null) return;

            if (Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Open(item);
            }
        }

        // 履歴項目決定(キー)
        private void PlaylistListItem_KeyDown(object? sender, KeyEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as PlaylistItem);
            if (item is null) return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Open(item);
                    e.Handled = true;
                }
            }
        }

        private void PlaylistItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var contextMenu = (sender as ListBoxItem)?.ContextMenu;
            if (contextMenu is null) return;

            var listBox = this.ListBox;
            contextMenu.Items.Clear();
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Open"), Command = OpenCommand });

            if (_vm.IsCurrentPlaylistBookOpened)
            {
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.OpenSource"), Command = OpenSourceCommand });
            }

            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Explorer"), Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(_commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Cut"), Command = CutCommand });
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Copy"), Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@PlaylistItem.Menu.CopyToFolder"), _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@PlaylistItem.Menu.MoveToFolder"), _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Delete"), Command = RemoveCommand });
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.Rename"), Command = RenameCommand });
            contextMenu.Items.Add(new Separator());

            var menuItem = new MenuItem() { Header = ResourceService.GetString("@PlaylistItem.Menu.MoveToAnother") };
            var paths = _vm.CollectAnotherPlaylists();
            if (paths.Any())
            {
                menuItem.IsEnabled = true;
                foreach (var path in paths)
                {
                    menuItem.Items.Add(new MenuItem()
                    {
                        Header = System.IO.Path.GetFileNameWithoutExtension(path),
                        Command = MoveToAnotherCommand,
                        CommandParameter = path
                    });
                }
            }
            else
            {
                menuItem.IsEnabled = false;
            }
            contextMenu.Items.Add(menuItem);
        }

        // リストのキ入力
        private void PlaylistListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (this.ListBox.IsSimpleTextSearchEnabled)
            {
                KeyExGesture.AddFilter(KeyExGestureFilter.TextKey);
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

        // 表示/非表示イベント
        private async void PlaylistListBox_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_vm.Visibility == Visibility.Visible)
            {
                ////_vm.UpdateItems();
                this.ListBox.UpdateLayout();

                await Task.Yield();

                if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
                FocusSelectedItem(_focusRequest);
                _focusRequest = false;
            }
        }

        private void PlaylistListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }

        // リスト全体が変化したときにサムネイルを更新する
        private void PlaylistListBox_TargetUpdated(object? sender, DataTransferEventArgs e)
        {
            AppDispatcher.BeginInvoke(() =>
            {
                this.ListBox.UpdateLayout();
                _thumbnailLoader?.Load();
            });
        }

        #region UI Accessor

        public List<PlaylistItem>? GetItems()
        {
            if (_vm is null) return null;

            return _vm.GetViewItems();
        }

        public List<PlaylistItem> GetSelectedItems()
        {
            // ListBox 生成直後でプロパティが不定の場合、モデルデータの値を返す
            if (this.ListBox.SelectedItem is null)
            {
                if (_vm.SelectedItem is null)
                {
                    return new();
                }
                else
                {
                    return new() { _vm.SelectedItem };
                }
            }

            return this.ListBox.SelectedItems.Cast<PlaylistItem>().ToList();
        }

        public void SetSelectedItems(IEnumerable<PlaylistItem> selectedItems)
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
                _vm.SelectedItem = null;
            }
        }

        public bool CanMovePrevious()
        {
            return _vm.CanMovePrevious();
        }

        public bool MovePrevious()
        {
            var result = _vm.MovePrevious();
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
            return result;
        }

        public bool CanMoveNext()
        {
            return _vm.CanMoveNext();
        }

        public bool MoveNext()
        {
            var result = _vm.MoveNext();
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
            return result;
        }

        #endregion UI Accessor
    }


    public class PlaylistItemToFolderImageSourceConverter : IValueConverter
    {
        public ImageSource? FolderImageSource { get; set; }
        public ImageSource? FolderZipImageSource { get; set; }
        public ImageSource? FolderMediaImageSource { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlaylistItem item)
            {
                return item.ArchiveType switch
                {
                    ArchiveType.None => null,
                    ArchiveType.FolderArchive => FolderImageSource,
                    ArchiveType.MediaArchive => FolderMediaImageSource,
                    _ => FolderZipImageSource,
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
