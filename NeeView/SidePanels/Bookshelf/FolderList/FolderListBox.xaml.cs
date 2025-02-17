using NeeLaboratory.Windows.Input;
using NeeView.Windows.Media;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// FolderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListBox : UserControl, IPageListPanel, IDisposable, IToolTipService
    {
        private readonly FolderListBoxViewModel _vm;
        private ListBoxThumbnailLoader? _thumbnailLoader;
        private PageThumbnailJobClient? _jobClient;


        static FolderListBox()
        {
            InitializeCommandStatic();
        }


        public FolderListBox(FolderListBoxViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
            this.ListBox.PreviewMouseUpWithSelectionChanged += ListBox_PreviewMouseUpWithSelectionChanged;

            this.Loaded += FolderListBox_Loaded;
            this.Unloaded += FolderListBox_Unloaded;

            if (_vm.FolderCollection is BookmarkFolderCollection)
            {
                var menu = new ContextMenu();
                menu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@FolderTree.Menu.AddBookmark"), Command = AddBookmarkCommand });
                menu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@Word.NewFolder"), Command = NewFolderCommand });
                this.ListBox.ContextMenu = menu;
            }
        }


        public bool IsToolTipEnabled
        {
            get { return (bool)GetValue(IsToolTipEnabledProperty); }
            set { SetValue(IsToolTipEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsToolTipEnabledProperty =
            DependencyProperty.Register("IsToolTipEnabled", typeof(bool), typeof(FolderListBox), new PropertyMetadata(true));



        // フォーカス可能フラグ
        public bool IsFocusEnabled { get; set; } = true;


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

        #region IPanelListBox Support

        //
        public ListBox PageCollectionListBox => this.ListBox;

        // サムネイルが表示されている？
        public bool IsThumbnailVisible => _vm.IsThumbnailVisible;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> enums) => enums.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand LoadWithRecursiveCommand = new("LoadWithRecursiveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenCommand = new("OpenCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenBookCommand = new("OpenBookCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new("OpenExplorerCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExternalAppCommand = new("OpenExternalAppCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CutCommand = new("CutCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyCommand = new("CopyCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyToFolderCommand = new("CopyToFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand MoveToFolderCommand = new("MoveToFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveCommand = new("RemoveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RenameCommand = new("RenameCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveHistoryCommand = new("RemoveHistoryCommand", typeof(FolderListBox));
        public static readonly RoutedCommand ToggleBookmarkCommand = new("ToggleBookmarkCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new("OpenDestinationFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new("OpenExternalAppDialogCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenInPlaylistCommand = new("OpenInPlaylistCommand", typeof(FolderListBox));

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            CutCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            ToggleBookmarkCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(LoadWithRecursiveCommand, LoadWithRecursive_Executed, LoadWithRecursive_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenBookCommand, OpenBook_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed, OpenExplorer_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExternalAppCommand, OpenExternalApp_Executed, OpenExternalApp_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(CutCommand, Cut_Executed, Cut_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyToFolderCommand, CopyToFolder_Execute, CopyToFolder_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(MoveToFolderCommand, MoveToFolder_Execute, MoveToFolder_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, Remove_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, Rename_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveHistoryCommand, RemoveHistory_Executed, RemoveHistory_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(ToggleBookmarkCommand, ToggleBookmark_Executed, ToggleBookmark_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenDestinationFolderCommand, OpenDestinationFolderDialog_Execute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExternalAppDialogCommand, OpenExternalAppDialog_Execute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenInPlaylistCommand, OpenInPlaylistCommand_Execute));
        }

        /// <summary>
        /// ブックマーク登録/解除可能？
        /// </summary>
        private void ToggleBookmark_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender is ListBox { SelectedItem: FolderItem item } && item.IsFileSystem() && !item.EntityPath.SimplePath.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal);
        }

        /// <summary>
        /// ブックマーク登録/解除
        /// </summary>
        private void ToggleBookmark_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: FolderItem item })
            {
                if (BookmarkCollection.Current.Contains(item.EntityPath.SimplePath))
                {
                    BookmarkCollectionService.Remove(item.EntityPath);
                }
                else
                {
                    BookmarkCollectionService.Add(item.EntityPath);
                }
            }
        }

        /// <summary>
        /// 履歴から削除できる？
        /// </summary>
        private void RemoveHistory_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// 履歴から削除
        /// </summary>
        private void RemoveHistory_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            BookHistoryCollection.Current.Remove(this.ListBox.SelectedItems.Cast<FolderItem>().Select(e => e.TargetPath.SimplePath));
        }

        /// <summary>
        /// サブフォルダーを読み込む？
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender is ListBox { SelectedItem: FolderItem item }
                && !item.Attributes.AnyFlag(FolderItemAttribute.Drive | FolderItemAttribute.Empty)
                && (Config.Current.System.ArchiveRecursiveMode == ArchiveEntryCollectionMode.IncludeSubArchives
                    ? (item.Attributes & (FolderItemAttribute.Directory | FolderItemAttribute.Playlist)) != 0
                    : ArchiveManager.Current.GetSupportedType(item.TargetPath.SimplePath).IsRecursiveSupported());
        }


        /// <summary>
        /// サブフォルダーを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is not FolderItem item) return;

            // サブフォルダー読み込み状態を反転する
            var option = item.IsRecursived ? BookLoadOption.NotRecursive : BookLoadOption.Recursive;
            _vm.Model.LoadBook(item, option);
        }

        /// <summary>
        /// ファイル系コマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender is ListBox { SelectedItem: FolderItem item } && item.IsEditable && Config.Current.System.IsFileWriteAccessEnabled;
        }

        /// <summary>
        /// 切り取りコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cut_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            e.CanExecute = items != null && items.All(x => x.IsEditable && x.IsFileSystem());
        }

        /// <summary>
        /// 切り取りコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Cut_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            if (items != null && items.Any())
            {
                CutToClipboard(items);
            }
        }

        /// <summary>
        /// クリップボードに切り取り
        /// </summary>
        private static void CutToClipboard(IEnumerable<FolderItem> items)
        {
            var selectedItems = items.Where(e => !e.IsEmpty() && e.IsEditable && e.IsFileSystem()).ToList();
            if (selectedItems.Count == 0)
            {
                return;
            }
            
            var collection = new System.Collections.Specialized.StringCollection();
            selectedItems.ForEach(e => collection.Add(e.EntityPath.SimplePath));

            var data = new DataObject();
            var guid = Guid.NewGuid();
            data.SetGuid(guid);
            data.SetFileDropList(collection);
            data.SetPreferredDropEffect(DragDropEffects.Move);
            Clipboard.SetDataObject(data);

            PendingItemManager.Current.AddRange(guid, selectedItems);
        }

        /// <summary>
        /// コピーコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            e.CanExecute = items != null && items.All(x => x.IsEditable);
        }

        /// <summary>
        /// コピーコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Copy_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            if (items != null && items.Any())
            {
                CopyToClipboard(items);
            }
        }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        private static void CopyToClipboard(IEnumerable<FolderItem> items)
        {
            var collection = new System.Collections.Specialized.StringCollection();
            foreach (var item in items.Where(e => !e.IsEmpty()).Select(e => e.EntityPath.SimplePath).Where(e => new QueryPath(e).Scheme == QueryScheme.File))
            {
                collection.Add(item);
            }

            if (collection.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetFileDropList(collection);
            Clipboard.SetDataObject(data);
        }

        /// <summary>
        /// フォルダーにコピーコマンド用
        /// </summary>
        private void CopyToFolder_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CopyToFolder_CanExecute();
        }

        private bool CopyToFolder_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return items != null && items.All(x => x.IsEditable);
        }

        public async void CopyToFolder_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not DestinationFolder folder) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = this.ListBox.SelectedItems.Cast<FolderItem>();
                if (items != null && items.Any())
                {
                    ////Debug.WriteLine($"CopyToFolder: to {folder.Path}");
                    await FileIO.CopyToFolderAsync(items.Select(x => x.TargetPath.SimplePath), folder.Path, CancellationToken.None);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, ResourceService.GetString("@Bookshelf.CopyToFolderFailed"), ToastIcon.Error));
            }
        }

        /// <summary>
        /// フォルダーに移動コマンド用
        /// </summary>
        private void MoveToFolder_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MoveToFolder_CanExecute();
        }

        private bool MoveToFolder_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return Config.Current.System.IsFileWriteAccessEnabled && items != null && items.All(x => x.IsEditable);
        }

        public async void MoveToFolder_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not DestinationFolder folder) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = this.ListBox.SelectedItems.Cast<FolderItem>();
                if (items != null && items.Any())
                {
                    ////Debug.WriteLine($"MoveToFolder: to {folder.Path}");
                    await FileIO.MoveToFolderAsync(items.Select(x => x.TargetPath.SimplePath), folder.Path, CancellationToken.None);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, ResourceService.GetString("@Bookshelf.Message.MoveToFolderFailed"), ToastIcon.Error));
            }
        }


        public void Remove_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            e.CanExecute = items != null && _vm.FolderCollection is not PlaylistFolderCollection && items.All(x => x.CanRemove());
        }

        public async void Remove_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>().ToList();
            await _vm.RemoveAsync(items);
            FocusSelectedItem(true);
        }


        public void Rename_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is not FolderItem item) return;
            if (_vm.FolderCollection is PlaylistFolderCollection) return;

            e.CanExecute = item.CanRename();
        }

        public async void Rename_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender != this.ListBox) return;

            await RenameAsync();
        }

        private async Task RenameAsync()
        {
            var listBox = this.ListBox;
            if (listBox.SelectedItem is not FolderItem item) return;

            var renamer = new FolderItemRenamer(listBox, this);
            renamer.SelectedItemChanged += (s, e) =>
            {
                if (listBox.SelectedItem is FolderItem item)
                {
                    _vm.Model.LoadBook(item);
                }
            };
            await renamer.RenameAsync(item);
        }


        /// <summary>
        /// エクスプローラーで開くコマンド
        /// </summary>
        private void OpenExplorer_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as ListBox)?.SelectedItem is FolderItem;
        }

        public void OpenExplorer_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: FolderItem item })
            {
                var path = item.TargetPath.SimplePath;
                path = item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiveManager.Current.GetExistPathName(path) : path;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    ExternalProcess.OpenWithFileManager(path);
                }
            }
        }

        /// <summary>
        /// 外部アプリで開く
        /// </summary>
        private void OpenExternalApp_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CopyToFolder_CanExecute();
        }

        private bool OpenExternalApp_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return items != null && items.All(x => x.IsEditable);
        }

        public void OpenExternalApp_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not ExternalApp externalApp) return;

            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            if (items != null && items.Any())
            {
                var paths = items.Select(x => x.TargetPath.SimplePath).ToList();
                externalApp.Execute(paths);
            }
        }

        //private string GetExistPathName(FolderItem item)
        //{
        //    var path = item.TargetPath.SimplePath;
        //    return item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiveManager.Current.GetExistPathName(path) : path;
        //}

        public void Open_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: FolderItem item })
            {
                _vm.MoveToSafety(item);
            }
        }

        public void OpenBook_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: FolderItem item } && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }
        }

        private void OpenDestinationFolderDialog_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            DestinationFolderDialog.ShowDialog(Window.GetWindow(this));
        }

        private void OpenExternalAppDialog_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            ExternalAppDialog.ShowDialog(Window.GetWindow(this));
        }

        private void OpenInPlaylistCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: FolderItem item } && item.IsPlaylist)
            {
                Config.Current.Playlist.CurrentPlaylist = item.EntityPath.SimplePath;
                SidePanelFrame.Current.IsVisiblePlaylist = true;
            }
        }


        private RelayCommand? _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get { return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(NewFolderCommand_Executed); }
        }

        private void NewFolderCommand_Executed()
        {
            _vm.Model.NewFolder();
        }

        private RelayCommand? _AddBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get { return _AddBookmarkCommand = _AddBookmarkCommand ?? new RelayCommand(AddBookmarkCommand_Executed); }
        }

        private void AddBookmarkCommand_Executed()
        {
            _vm.Model.AddBookmark();
        }

#endregion

        #region DragDrop

        public async Task DragStartBehavior_DragBeginAsync(object? sender, DragStartEventArgs e, CancellationToken token)
        {
            var items = this.ListBox.SelectedItems
                .Cast<FolderItem>()
                .Where(x => !x.Attributes.HasFlag(FolderItemAttribute.Empty))
                .ToList();

            if (!items.Any())
            {
                e.Cancel = true;
                return;
            }

            // List<QueryPath>
            e.Data.SetQueryPathCollection(items.Select(x => x.TargetPath));

            // bookmark?
            if (items.Any(x => x.Attributes.AnyFlag(FolderItemAttribute.Bookmark)))
            {
                var collection = items.Select(x => x.Source).OfType<TreeListNode<IBookmarkEntry>>().ToBookmarkNodeCollection();
                e.Data.SetData(collection);
                e.AllowedEffects |= DragDropEffects.Move;
            }
            // files only, no archive path
            else if (items.All(e => System.IO.Path.Exists(e.TargetPath.SimplePath)))
            {
                var collection = new System.Collections.Specialized.StringCollection();
                foreach (var path in items.Where(x => x.IsFileSystem()).Select(x => x.TargetPath.SimplePath).Distinct())
                {
                    collection.Add(path);
                }
                if (collection.Count > 0)
                {
                    e.Data.SetFileDropList(collection);

                    // 右クリックドラッグは移動を許可
                    if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                    {
                        e.AllowedEffects |= DragDropEffects.Move;
                    }
                }
            }

            // text
            if (Config.Current.System.TextCopyPolicy != TextCopyPolicy.None)
            {
                var text = string.Join(System.Environment.NewLine, items.Select(e => e.TargetPath.SimplePath));
                e.Data.SetText(text);
            }

            await Task.CompletedTask;
        }

        private void FolderList_PreviewDragEnter(object? sender, DragEventArgs e)
        {
            FolderList_PreviewDragOver(sender, e);
        }

        private void FolderList_PreviewDragOver(object? sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
        }

        private void FolderList_Drop(object? sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, true);
        }

        private void FolderList_DragDrop(object? sender, DragEventArgs e, bool isDrop)
        {
            var listBoxItem = PointToViewItem(this.ListBox, e.GetPosition(this.ListBox));

            // bookmark
            if (_vm.FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                TreeListNode<IBookmarkEntry>? bookmarkNode;
                if (listBoxItem?.Content is FolderItem target && target.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
                {
                    bookmarkNode = target.Source as TreeListNode<IBookmarkEntry>;
                }
                else
                {
                    bookmarkNode = bookmarkFolderCollection.BookmarkPlace;
                }

                if (bookmarkNode != null)
                {
                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetData<BookmarkNodeCollection>());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetQueryPathCollection());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetFileDrop());
                    if (e.Handled) return;
                }
            }
        }

        private void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<TreeListNode<IBookmarkEntry>>? bookmarkEntries)
        {
            if (bookmarkEntries == null || !bookmarkEntries.Any())
            {
                return;
            }

            e.Effects = bookmarkEntries.All(x => CanDropToBookmark(node, x)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                foreach (var bookmarkEntry in bookmarkEntries)
                {
                    DropToBookmarkExecute(node, bookmarkEntry);
                }
            }
        }

#if false
        private void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry>? bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            e.Effects = CanDropToBookmark(node, bookmarkEntry) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                DropToBookmarkExecute(node, bookmarkEntry);
            }
        }
#endif

        private static bool CanDropToBookmark(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            return !node.Children.Contains(bookmarkEntry) && !node.ParentContains(bookmarkEntry) && node != bookmarkEntry;
        }

        private void DropToBookmarkExecute(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            _vm.Model.SelectBookmark(node, true);
            BookmarkCollection.Current.MoveToChild(bookmarkEntry, node);
        }

        private void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<QueryPath>? queries)
        {
            if (queries == null || !queries.Any())
            {
                return;
            }

            foreach (var query in queries)
            {
                DropToBookmark(sender, e, isDrop, node, query);
            }
        }

        private void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (node.Value is BookmarkFolder && CanDropToBookmark(query))
            {
                if (isDrop)
                {
                    var bookmark = BookmarkCollectionService.AddToChild(node, query);
                    if (bookmark is null) return;
                    _vm.Model.SelectBookmark(bookmark, true);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static bool CanDropToBookmark(QueryPath query)
        {
            if (query.Search != null)
            {
                return false;
            }

            return query.Scheme switch
            {
                QueryScheme.File => CanDropToBookmark(query.SimplePath),
                _ => false,
            };
        }

        private void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            foreach (var fileName in fileNames)
            {
                if (CanDropToBookmark(fileName))
                {
                    if (isDrop)
                    {
                        var bookmark = BookmarkCollectionService.AddToChild(node, new QueryPath(fileName));
                        if (bookmark is null) continue;
                        _vm.Model.SelectBookmark(bookmark, true);
                    }
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private static bool CanDropToBookmark(string path)
        {
            return ArchiveManager.Current.IsSupported(path, true, true) || System.IO.Directory.Exists(path);
        }

        private static ListBoxItem? PointToViewItem(ListBox listBox, Point point)
        {
            // NOTE: リストアイテム間に隙間がある場合があるので、Y座標をずらして再検証する
            var element = VisualTreeUtility.HitTest<ListBoxItem>(listBox, point) ?? VisualTreeUtility.HitTest<ListBoxItem>(listBox, new Point(point.X, point.Y + 1));
            return element;
        }

        #endregion


        private void FolderListBox_Loaded(object? sender, RoutedEventArgs e)
        {
            _jobClient = new PageThumbnailJobClient("FolderList", JobCategories.BookThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);

            _vm.SelectedChanged += ViewModel_SelectedChanged;
            _vm.BusyChanged += ViewModel_BusyChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
        }

        private void FolderListBox_Unloaded(object? sender, RoutedEventArgs e)
        {
            _jobClient?.Dispose();

            _vm.SelectedChanged -= ViewModel_SelectedChanged;
            _vm.BusyChanged -= ViewModel_BusyChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
        }

        /// <summary>
        /// サムネイルパラメーターが変化したらアイテムをリフレッシュする
        /// </summary>
        private void PanelListItemProfile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            if (!this.ListBox.IsVisible)
            {
                return;
            }

            if (this.ListBox.SelectedIndex < 0)
            {
                this.ListBox.SelectedIndex = 0;
            }

            var needToFocus = (isFocus && this.IsFocusEnabled) || _vm.IsFocusAtOnce;

            if (this.ListBox.SelectedIndex < 0 && needToFocus)
            {
                _vm.IsFocusAtOnce = false;
                this.ListBox.Focus();
                return;
            }

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (needToFocus)
            {
                _vm.IsFocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }


        public async void ViewModel_SelectedChanged(object? sender, FolderListSelectedChangedEventArgs e)
        {
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
            this.ListBox.UpdateLayout();
            this.ListBox.FocusSelectedItem(false);

            _thumbnailLoader?.Load();

            if (e.IsNewFolder)
            {
                await RenameAsync();
            }
        }

        private void FolderList_Loaded(object? sender, RoutedEventArgs e)
        {
        }

        private async void FolderList_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                _vm.IsVisibleChanged(true);
                // NOTE: ListBoxItemの表示を確定？
                await Task.Yield();
                FocusSelectedItem(false);
            }
        }

        private void FolderList_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Home)
                {
                    _vm.MoveToHome();
                    e.Handled = true;
                }
                else if (key == Key.Up)
                {
                    _vm.MoveToUp();
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    if (sender is ListBox { SelectedItem: FolderItem item })
                    {
                        _vm.MoveToSafety(item);
                        e.Handled = true;
                    }
                }
                else if (key == Key.Left)
                {
                    _vm.MoveToPrevious();
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    _vm.MoveToNext();
                    e.Handled = true;
                }
            }
        }

        private void FolderList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (this.ListBox.IsSimpleTextSearchEnabled)
            {
                KeyExGesture.AllowSingleKey = false;
            }

            bool isLRKeyEnabled = _vm.IsLRKeyEnabled();
            if (isLRKeyEnabled && e.Key == Key.Left) // ←
            {
                _vm.MoveToUp();
                e.Handled = true;
            }
        }

        private void FolderList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }

        // 項目クリック (複数選択解除)
        private void ListBox_PreviewMouseUpWithSelectionChanged(object? sender, MouseButtonEventArgs e)
        {
            if (this.ListBox.SelectedItems.Count != 1) return;

            if (this.ListBox.SelectedItem is FolderItem item)
            {
                ClickToLoadBook(item);
            }
        }

        // 項目クリック
        private void FolderListItem_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem { Content: FolderItem item })
            {
                ClickToLoadBook(item);
            }
        }

        private void ClickToLoadBook(FolderItem item)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (!Config.Current.Panels.OpenWithDoubleClick && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }
        }

        // 項目ダブルクリック
        private void FolderListItem_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as FolderItem;
            if (Config.Current.Panels.OpenWithDoubleClick && item != null && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }

            _vm.MoveToSafety(item);

            e.Handled = true;
        }

        //
        private void FolderListItem_KeyDown(object? sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = _vm.IsLRKeyEnabled();
            if ((sender as ListBoxItem)?.Content is not FolderItem item) return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Model.LoadBook(item);
                    e.Handled = true;
                }
                else if (isLRKeyEnabled && e.Key == Key.Right) // →
                {
                    _vm.MoveToSafety(item);
                    e.Handled = true;
                }
                else if (isLRKeyEnabled && e.Key == Key.Left) // ←
                {
                    _vm.MoveToUp();
                    e.Handled = true;
                }
            }
        }


        private void FolderListItem_MouseDown(object? sender, MouseButtonEventArgs e)
        {
        }

        private void FolderListItem_MouseUp(object? sender, MouseButtonEventArgs e)
        {
        }

        private void FolderListItem_MouseMove(object? sender, MouseEventArgs e)
        {
        }


        /// <summary>
        /// コンテキストメニュー開始前イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderListItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            if (sender is not ListBoxItem container)
            {
                return;
            }

            if (container.Content is not FolderItem item)
            {
                return;
            }

            // サブフォルダー読み込みの状態を更新
            var isDefaultRecursive = _vm.FolderCollection != null && _vm.FolderCollection.FolderParameter.IsFolderRecursive;
            item.UpdateIsRecursived(isDefaultRecursive);

            // コンテキストメニュー生成

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();


            if (item.Attributes.HasFlag(FolderItemAttribute.System))
            {
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Open"), Command = OpenCommand });
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                if (item.IsDirectory)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Open"), Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Delete"), Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Rename"), Command = RenameCommand });
                }
                else
                {
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.OpenBook"), Command = OpenBookCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Explorer"), Command = OpenExplorerCommand });
                    contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(OpenExternalApp_CanExecute(), OpenExternalAppCommand, OpenExternalAppDialogCommand));
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Cut"), Command = CutCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Copy"), Command = CopyCommand });
                    contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@BookshelfItem.Menu.CopyToFolder"), CopyToFolder_CanExecute(), CopyToFolderCommand, OpenDestinationFolderCommand));
                    contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@BookshelfItem.Menu.MoveToFolder"), false, MoveToFolderCommand, OpenDestinationFolderCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.DeleteBookmark"), Command = RemoveCommand });
                }
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Empty))
            {
                bool canExplorer = _vm.FolderCollection is not BookmarkFolderCollection;
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Explorer"), Command = OpenExplorerCommand, IsEnabled = canExplorer });
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Copy"), Command = CopyCommand, IsEnabled = false });
            }
            else if (item.IsFileSystem())
            {
                if (item.IsDirectory || Config.Current.System.ArchiveRecursiveMode != ArchiveEntryCollectionMode.IncludeSubArchives)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Open"), Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                }
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.OpenBook"), Command = OpenBookCommand });
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Subfolder"), Command = LoadWithRecursiveCommand, IsChecked = item.IsRecursived });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@Word.Bookmark"), Command = ToggleBookmarkCommand, IsChecked = BookmarkCollection.Current.Contains(item.EntityPath.SimplePath) });
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.DeleteHistory"), Command = RemoveHistoryCommand });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Explorer"), Command = OpenExplorerCommand });
                contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(OpenExternalApp_CanExecute(), OpenExternalAppCommand, OpenExternalAppDialogCommand));
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Cut"), Command = CutCommand });
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Copy"), Command = CopyCommand });
                contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@BookshelfItem.Menu.CopyToFolder"), CopyToFolder_CanExecute(), CopyToFolderCommand, OpenDestinationFolderCommand));
                contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(ResourceService.GetString("@BookshelfItem.Menu.MoveToFolder"), MoveToFolder_CanExecute(), MoveToFolderCommand, OpenDestinationFolderCommand));
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Delete"), Command = RemoveCommand });
                contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.Rename"), Command = RenameCommand });
                if (item.IsPlaylist)
                {
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@BookshelfItem.Menu.OpenInPlaylist"), Command = OpenInPlaylistCommand });
                }
            }
        }

        /// <summary>
        /// リスト更新中
        /// </summary>
        private void ViewModel_BusyChanged(object? sender, ReferenceCounterChangedEventArgs e)
        {
            this.BusyFade.IsBusy = e.IsActive;
            if (e.IsActive)
            {
                RenameManager.GetRenameManager(this)?.CloseAll(false, false);
            }
        }

        public void Refresh()
        {
            this.ListBox.Items.Refresh();
        }


        #region UI Accessor

        public List<FolderItem> GetItems()
        {
            return this.ListBox.Items?.Cast<FolderItem>().ToList() ?? new List<FolderItem>();
        }

        public List<FolderItem> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<FolderItem>().ToList();
        }

        public void SetSelectedItems(IEnumerable<FolderItem> selectedItems)
        {
            var items = selectedItems?.Intersect(GetItems()).ToList() ?? new List<FolderItem>();
            this.ListBox.SetSelectedItems(items);
            this.ListBox.ScrollItemsIntoView(items);
        }

        #endregion UI Accessor
    }




    public class FolderItemToNoteConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is FolderItem item && values[1] is FolderOrder order)
            {
                return item.GetNote(order);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
