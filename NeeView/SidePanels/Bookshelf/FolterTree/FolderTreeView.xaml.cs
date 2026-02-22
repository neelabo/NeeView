using NeeLaboratory.Windows.Input;
using NeeView.Collections.Generic;
using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FolderTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderTreeView : UserControl, INavigateControl
    {
        private readonly FolderTreeViewModel _vm;
        private readonly SimpleTextSearch _textSearch = new();
        private readonly FolderTreeViewDropAssist _dropAssist;

        private RelayCommand? _addQuickAccessCommand;
        private RelayCommand? _removeCommand;
        private RelayCommand? _propertyCommand;
        private RelayCommand? _refreshFolderCommand;
        private RelayCommand? _openExplorerCommand;
        private RelayCommand? _newFolderCommand;
        private RelayCommand? _renameCommand;
        private RelayCommand? _addBookmarkCommand;


        public FolderTreeView()
        {
            InitializeComponent();

            _vm = new FolderTreeViewModel();
            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            // タッチスクロール操作の終端挙動抑制
            this.TreeView.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.Root.DataContext = _vm;

            this.Loaded += FolderTreeView_Loaded;
            this.Unloaded += FolderTreeView_Unloaded;

            _dropAssist = new FolderTreeViewDropAssist(this.TreeView, _vm);
        }


        public object SelectedItem => this.TreeView.SelectedItem;


        #region Dependency Properties

        public FolderTreeModel Model
        {
            get { return (FolderTreeModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(FolderTreeModel), typeof(FolderTreeView), new PropertyMetadata(null, ModelPropertyChanged));

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderTreeView control)
            {
                control.UpdateModel();
            }
        }

        #endregion


        #region Commands

        public RelayCommand AddQuickAccessCommand
        {
            get
            {
                return _addQuickAccessCommand = _addQuickAccessCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem as TreeListNode<QuickAccessEntry>;
                    if (item != null)
                    {
                        _vm.AddCurrentPlaceQuickAccess(item);
                    }
                }
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand = _removeCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case TreeListNode<QuickAccessEntry> quickAccess:
                            _vm.RemoveQuickAccess(quickAccess);
                            break;

                        case RootBookmarkFolderNode rootBookmarkFolder:
                            break;

                        case BookmarkFolderNode bookmarkFolder:
                            _vm.RemoveBookmarkFolder(bookmarkFolder);
                            break;
                    }
                }
            }
        }

        public RelayCommand PropertyCommand
        {
            get
            {
                return _propertyCommand = _propertyCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case TreeListNode<QuickAccessEntry> { Value: QuickAccess quickAccess }:
                            {
                                var work = (QuickAccess)quickAccess.Clone();
                                var dialog = new QuickAccessPropertyDialog(work)
                                {
                                    Owner = Window.GetWindow(this),
                                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                };
                                var result = dialog.ShowDialog();
                                if (result == true)
                                {
                                    quickAccess.Restore(work.CreateMemento());
                                }
                            }
                            break;
                    }
                }
            }
        }

        public RelayCommand RefreshFolderCommand
        {
            get
            {
                return _refreshFolderCommand = _refreshFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _vm.RefreshFolder();
                }
            }
        }

        public RelayCommand OpenExplorerCommand
        {
            get
            {
                return _openExplorerCommand = _openExplorerCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    if (this.TreeView.SelectedItem is DirectoryNode item)
                    {
                        ExternalProcess.OpenWithFileManager(item.Path, true);
                    }
                }
            }
        }

        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand = _newFolderCommand ?? new RelayCommand(Execute);

                async void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } quickAccess:
                            {
                                var newItem = _vm.NewQuickAccessFolder(quickAccess);
                                if (newItem != null)
                                {
                                    this.TreeView.UpdateLayout();
                                    await RenameQuickAccess(newItem);
                                }
                            }
                            break;

                        case BookmarkFolderNode bookmarkFolderNode:
                            {
                                var newItem = _vm.NewBookmarkFolder(bookmarkFolderNode);
                                if (newItem != null)
                                {
                                    this.TreeView.UpdateLayout();
                                    await RenameBookmarkFolder(newItem);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public RelayCommand RenameCommand
        {
            get
            {
                return _renameCommand = _renameCommand ?? new RelayCommand(Execute);

                async void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case TreeListNode<QuickAccessEntry> quickAccess:
                            await RenameQuickAccess(quickAccess);
                            break;

                        case BookmarkFolderNode bookmarkFolderNode:
                            await RenameBookmarkFolder(bookmarkFolderNode);
                            break;
                    }
                }
            }
        }

        public RelayCommand AddBookmarkCommand
        {
            get
            {
                return _addBookmarkCommand = _addBookmarkCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    if (this.TreeView.SelectedItem is BookmarkFolderNode item)
                    {
                        _vm.AddBookmarkTo(item);
                    }
                }
            }
        }

#endregion


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _vm.DpiChanged(oldDpi, newDpi);
        }

        private void UpdateModel()
        {
            _vm.Model = Model;
            this.TreeView.ItemsSource = Model?.Items;
            TreeViewItemsSource.SetMultiRoot(this.TreeView, true);
        }

        private void FolderTreeView_Loaded(object? sender, RoutedEventArgs e)
        {
            FocusSelectedItem();
        }

        private void FolderTreeView_Unloaded(object? sender, RoutedEventArgs e)
        {
        }

        private async ValueTask RenameQuickAccess(TreeListNode<QuickAccessEntry> item)
        {
            if (!item.CanRename()) return;

            var renamer = new FolderTreeQuickAccessRenamer(this.TreeView, null);
            await renamer.RenameAsync(item);
        }

        private async ValueTask RenameBookmarkFolder(BookmarkFolderNode item)
        {
            var renamer = new FolderTreeItemRenamer(this.TreeView, null);
            await renamer.RenameAsync(item);
        }

        public void FocusSelectedItem()
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            if (this.TreeView.SelectedItem == null)
            {
                _vm.SelectFirstItem();
            }

            if (_vm.Model.IsFocusAtOnce)
            {
                _vm.Model.IsFocusAtOnce = false;
                ScrollIntoViewSelectedItem(true);
            }
        }

        private void ScrollIntoViewSelectedItem(bool isFocus)
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            if (!this.TreeView.IsVisible)
            {
                return;
            }

            var selectedItem = _vm.Model.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var container = this.TreeView.ScrollIntoView(selectedItem);
            if (container is not null && isFocus)
            {
                container.Focus();
                ////Debug.WriteLine($"FolderTree.Focused: {isFocused}");
            }

            _vm.Model.SelectedItem = selectedItem;
        }

        private void ViewModel_SelectedItemChanged(object? sender, EventArgs e)
        {
            ScrollIntoViewSelectedItem(false);
        }

        private void TreeView_SelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_vm.IsValid) return;
            if (_vm.Model is null) return;

            _vm.Model.SelectedItem = this.TreeView.SelectedItem as ITreeViewNode;
        }

        private async void TreeView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_vm.IsValid) return;

            var isVisible = (bool)e.NewValue;
            _vm.IsVisibleChanged(isVisible);
            if (isVisible)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }

        private void TreeViewItem_Selected(object? sender, RoutedEventArgs e)
        {
        }

        private void TreeViewItem_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }


        private void TreeViewItem_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (sender is TreeViewItem viewItem)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (sender is not TreeViewItem viewItem)
            {
                return;
            }

            if (e.Key == Key.Return)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (viewItem.IsSelected)
                {
                    RemoveCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                if (viewItem.IsSelected)
                {
                    RenameCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            if (sender is not TreeViewItem viewItem)
            {
                return;
            }

            if (!viewItem.IsSelected)
            {
                return;
            }

            var contextMenu = viewItem.ContextMenu;
            contextMenu.Items.Clear();

            switch (viewItem.DataContext)
            {
                case TreeListNode<QuickAccessEntry> entry:
                    switch (entry.Value)
                    {
                        case QuickAccessRoot:
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.AddCurrentQuickAccess"), AddQuickAccessCommand));
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                            break;

                        case QuickAccessFolder:
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.AddCurrentQuickAccess"), AddQuickAccessCommand));
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                            contextMenu.Items.Add(new Separator());
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                            break;

                        case QuickAccess:
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                            contextMenu.Items.Add(new Separator());
                            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Property"), PropertyCommand));
                            break;
                    }
                    break;

                case RootDirectoryNode:
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.RefreshFolder"), RefreshFolderCommand));
                    break;

                case DirectoryNode:
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Explorer"), OpenExplorerCommand));
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.AddQuickAccess"), AddQuickAccessCommand));
                    break;

                case RootBookmarkFolderNode:
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.AddBookmark"), AddBookmarkCommand));
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    break;

                case BookmarkFolderNode:
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.AddBookmark"), AddBookmarkCommand));
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        private static MenuItem CreateMenuItem(string header, ICommand command)
        {
            return new MenuItem() { Header = header, Command = command };
        }

        private static MenuItem CreateMenuItem(string header, ICommand command, string inputGestureText)
        {
            return new MenuItem() { Header = header, Command = command, InputGestureText = inputGestureText };
        }

        #region DragDrop

        public async ValueTask DragStartBehavior_DragBeginAsync(object? sender, DragStartEventArgs e, CancellationToken token)
        {
            if (e.DragItem is not TreeViewItem data)
            {
                e.Cancel = true;
                return;
            }

            switch (data.DataContext)
            {
                case TreeListNode<QuickAccessEntry> quickAccessEntry:
                    e.Data.SetData(quickAccessEntry);
                    if (quickAccessEntry.Value is QuickAccess quickAccess)
                    {
                        e.Data.SetText(quickAccess.Path);
                    }
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    break;

                case DirectoryNode directory:
                    if (System.IO.Path.Exists(directory.Path))
                    {
                        e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { directory.Path });
                    }
                    e.Data.SetFileTextList([new QueryPath(directory.Path)]);
                    e.AllowedEffects = DragDropEffects.Copy;
                    break;

                //case RootBookmarkFolderNode rootBookmarkFolder:
                //    break;

                case BookmarkFolderNode bookmarkFolder:
                    var bookmarkNodeCollection = new BookmarkNodeCollection() { bookmarkFolder.BookmarkSource };
                    e.Data.SetData(bookmarkNodeCollection);
                    e.Data.SetText(bookmarkFolder.Path);
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    break;

                default:
                    e.Cancel = true;
                    break;
            }

            await Task.CompletedTask;
        }

        private void TreeView_PreviewDragEnter(object? sender, DragEventArgs e)
        {
            _dropAssist.OnDragEnter(sender, e);

            TreeView_PreviewDragOver(sender, e);
        }

        private void TreeView_PreviewDragLeave(object? sender, DragEventArgs e)
        {
            _dropAssist.OnDragLeave(sender, e);
        }

        private void TreeView_PreviewDragOver(object? sender, DragEventArgs e)
        {
            var scrolled = DragDropHelper.AutoScroll(sender, e);
            if (scrolled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            if (!e.Handled)
            {
                var target = _dropAssist.OnDragOver(sender, e);
                TreeView_DragDrop(sender, e, target, false);
            }

            if (e.Effects == DragDropEffects.None)
            {
                _dropAssist.HideAdorner();
            }
        }

        private void TreeView_Drop(object? sender, DragEventArgs e)
        {
            var target = _dropAssist.OnDrop(sender, e);

            TreeView_DragDrop(sender, e, target, true);
        }

        private void TreeView_DragDrop(object? sender, DragEventArgs e, DropTargetItem target, bool isDrop)
        {
            if (!_vm.IsValid) return;

            var treeViewItem = target.Item;
            if (treeViewItem != null)
            {
                switch (treeViewItem.DataContext)
                {
                    case TreeListNode<QuickAccessEntry> quickAccessTarget:
                        {
                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, target.Delta, e.Data.GetData<TreeListNode<QuickAccessEntry>>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, target.Delta, e.Data.GetData<BookmarkNodeCollection>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, target.Delta, e.Data.GetQueryPathCollection());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, target.Delta, e.Data.GetNormalizedFileDrop());
                            if (e.Handled) return;
                        }
                        break;

                    case BookmarkFolderNode bookmarkFolderTarget:
                        {
                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetData<BookmarkNodeCollection>());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetQueryPathCollection());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetNormalizedFileDrop());
                            if (e.Handled) return;
                        }
                        break;
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, TreeListNode<QuickAccessEntry>? quickAccess)
        {
            if (quickAccessTarget == null || quickAccess == null)
            {
                return;
            }

            if (!CanDropToQuickAccess(quickAccessTarget, delta, quickAccess))
            {
                return;
            }

            if (quickAccess != quickAccessTarget)
            {
                if (isDrop)
                {
                    _vm.MoveQuickAccess(quickAccess, quickAccessTarget, delta);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, IEnumerable<TreeListNode<IBookmarkEntry>>? bookmarkEntries)
        {
            // QuickAccessは大量操作できないので先頭１項目だけ処理する
            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, delta, bookmarkEntries?.FirstOrDefault());
        }


        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, TreeListNode<IBookmarkEntry>? bookmarkEntry)
        {
            if (_vm.Model is null) return;
            if (bookmarkEntry is null) return;

            if (bookmarkEntry.Value is BookmarkFolder)
            {
                if (isDrop)
                {
                    var query = bookmarkEntry.CreateQuery(QueryScheme.Bookmark);
                    _vm.Model.InsertQuickAccess(null, quickAccessTarget, delta, query.SimplePath);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, IEnumerable<QueryPath>? queries)
        {
            // QuickAccessは大量操作できないので先頭１項目だけ処理する
            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, delta, queries?.FirstOrDefault());
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, QueryPath? query)
        {
            if (query == null) return;
            if (_vm.Model is null) return;

            if ((query.Scheme == QueryScheme.File && (Directory.Exists(query.SimplePath) || IsPlaylistFile(query.SimplePath)))
                || (query.Scheme == QueryScheme.Bookmark && BookmarkCollection.Current.FindNode(query)?.Value is BookmarkFolder))
            {
                if (isDrop)
                {
                    _vm.Model.InsertQuickAccess(null, quickAccessTarget, delta, query.SimpleQuery);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object? sender, DragEventArgs e, bool isDrop, TreeListNode<QuickAccessEntry>? quickAccessTarget, int delta, string[] fileNames)
        {
            if (_vm.Model is null) return;

            if (fileNames == null || fileNames.Length == 0)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (Directory.Exists(fileName) || IsPlaylistFile(fileName))
                {
                    if (isDrop)
                    {
                        _vm.Model.InsertQuickAccess(null, quickAccessTarget, delta, fileName);
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static bool CanDropToQuickAccess(TreeListNode<QuickAccessEntry> target, int delta, TreeListNode<QuickAccessEntry> node)
        {
            var targetNode = target;

            if (targetNode == node) return false;

            if (delta == 0)
            {
                return !targetNode.ParentContains(node) && !targetNode.Contains(node);
            }
            else
            {
                return !targetNode.ParentContains(node);
            }
        }

        private static bool IsPlaylistFile(string path)
        {
            return File.Exists(path) && PlaylistArchive.IsSupportExtension(path);
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, IEnumerable<TreeListNode<IBookmarkEntry>>? bookmarkEntries)
        {
            if (bookmarkEntries == null || !bookmarkEntries.Any())
            {
                return;
            }

            e.Effects = bookmarkEntries.All(x => CanDropToBookmark(bookmarkFolderTarget, x))
                ? (Keyboard.Modifiers == ModifierKeys.Control ? DragDropEffects.Copy : DragDropEffects.Move)
                : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects != DragDropEffects.None)
            {
                if (e.Effects == DragDropEffects.Copy)
                {
                    bookmarkEntries = bookmarkEntries.Select(e => e.Clone());
                }

                foreach (var bookmarkEntry in bookmarkEntries)
                {
                    DropToBookmarkExecute(bookmarkFolderTarget, bookmarkEntry);
                }
            }
        }

        private static bool CanDropToBookmark(BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry.Value is BookmarkFolder)
            {
                var node = bookmarkFolderTarget.BookmarkSource;
                return !node.Contains(bookmarkEntry) && !node.ParentContains(bookmarkEntry) && node != bookmarkEntry;
            }
            else
            {
                return true;
            }
        }

        private static void DropToBookmarkExecute(BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            BookmarkCollection.Current.MoveToChild(bookmarkEntry, bookmarkFolderTarget.BookmarkSource);
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, IEnumerable<QueryPath>? queries)
        {
            if (queries == null || !queries.Any())
            {
                return;
            }

            foreach (var query in queries)
            {
                DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, query);
            }
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (query.Search == null && query.Scheme == QueryScheme.File && CanDropToBookmark(query.SimplePath))
            {
                if (isDrop)
                {
                    BookmarkCollectionService.Add(query, bookmarkFolderTarget.BookmarkSource, null, false);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static void DropToBookmark(object? sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }


            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (CanDropToBookmark(fileName))
                {
                    if (isDrop)
                    {
                        BookmarkCollectionService.Add(new QueryPath(fileName), bookmarkFolderTarget.BookmarkSource, null, false);
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private static bool CanDropToBookmark(string path)
        {
            return ArchiveManager.Current.IsSupported(path, true, true) || System.IO.Directory.Exists(path);
        }

        #endregion DragDrop

        #region TextSearch

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (Config.Current.Panels.IsTextSearchEnabled)
            {
                KeyExGesture.AddFilter(KeyExGestureFilter.TextKey);
            }
        }

        private void TreeView_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (Config.Current.Panels.IsTextSearchEnabled)
            {
                var item = this.TreeView.SelectedItem as ITreeViewNode;
                var roots = item?.GetRoot()?.Children;
                roots = _vm.Model?.Items;
                if (roots is not null)
                {
                    var source = new TreeViewTextSearchCollection(this, roots);
                    _textSearch.DoSearch(source, e.Text);
                    e.Handled = true;
                }
            }
        }

        public void NavigateToItem(object item)
        {
            if (item is not ITreeViewNode itemData) return;

            this.TreeView.ScrollIntoView(itemData);
            itemData.IsSelected = true;
        }

        #endregion TextSearch
    }


    public class FolderTreeQuickAccessRenamer : TreeViewItemRenamer<TreeListNode<QuickAccessEntry>>
    {
        public FolderTreeQuickAccessRenamer(TreeView treeView, IToolTipService? toolTipService) : base(treeView, toolTipService)
        {
        }

        protected override RenameControl CreateRenameControl(TreeView treeView, TreeListNode<QuickAccessEntry> item)
        {
            var control = base.CreateRenameControl(treeView, item);
            control.IsInvalidSeparatorChars = true;
            return control;
        }
    }


    public class FolderTreeItemRenamer : TreeViewItemRenamer<FolderTreeNodeBase>
    {
        public FolderTreeItemRenamer(TreeView treeView, IToolTipService? toolTipService) : base(treeView, toolTipService)
        {
        }

        protected override RenameControl CreateRenameControl(TreeView treeView, FolderTreeNodeBase item)
        {
            var control = base.CreateRenameControl(treeView, item);
            control.IsInvalidSeparatorChars = true;
            return control;
        }
    }


    public class FolderTreeItemTemplateSelector : DataTemplateSelector
    {
        public HierarchicalDataTemplate? Default { get; set; }
        public HierarchicalDataTemplate? QuickAccess { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var template = (item is TreeListNode<QuickAccessEntry>) ? QuickAccess : Default;
            return template ?? base.SelectTemplate(item, container);
        }
    }
}
