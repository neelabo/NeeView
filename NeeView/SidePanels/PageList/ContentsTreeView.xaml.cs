using CommunityToolkit.Mvvm.Input;
using NeeView.Properties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    public partial class ContentsTreeView : UserControl
    {
        private readonly ContentsTreeViewModel _vm;

        public ContentsTreeView()
        {
            InitializeComponent();

            _vm = new ContentsTreeViewModel();
            this.Root.DataContext = _vm;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                    return;
                }
            }

            base.OnKeyDown(e);
        }

        public void FocusAtOnce()
        {
            Dispatcher.BeginInvoke(() => this.ContentsTree.Focus(), DispatcherPriority.Input);
        }

        private void ContentsTree_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool isVisible)
            {
                _vm.IsVisible = isVisible;
            }
        }

        private void ContentsTree_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.ContentsTree.SelectedItem is ContentsPageNode node)
            {
                node.IsSelected = false;
            }
        }

        private void ContentsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ContentsPageNode node)
            {
                _vm.SelectIndex(node);
            }
        }

        private bool CanOpenFolderBook()
        {
            if (this.ContentsTree.SelectedItem is ContentsPageNode node && node.Page is not null)
            {
                return !string.IsNullOrEmpty(GetFolderBookPath(node));
            }
            else
            {
                return false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanOpenFolderBook))]
        private void OpenFolderBook()
        {
            if (this.ContentsTree.SelectedItem is ContentsPageNode node && node.Page is not null)
            {
                var path = GetFolderBookPath(node);
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                var options = BookSettings.Current.IsRecursiveFolder ? BookLoadOption.Recursive : BookLoadOption.None;
                BookHub.Current.RequestLoad(this, path, null, BookLoadOption.IsBook | options, true);
            }
        }

        private static string? GetFolderBookPath(ContentsPageNode node)
        {
            if (node.IsRoot)
            {
                return null;
            }

            if (node.Page is null)
            {
                return null;
            }

            var page = node.Page;
            var archivePath = page.ArchiveEntry.Archive.SystemPath;
            var folderPath = LoosePath.GetDirectoryName(page.ArchiveEntry.EntryName);
            var path =  LoosePath.Combine(archivePath, folderPath);
            if (path == page.BookPath)
            {
                return null;
            }

            return path;
        }

        private void TreeViewItem_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
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

            contextMenu.Items.Add(CreateMenuItem(TextResources.GetString("Menu.OpenAsBook"), OpenFolderBookCommand));
        }

        private static MenuItem CreateMenuItem(string header, ICommand command)
        {
            return new MenuItem() { Header = header, Command = command };
        }

    }
}
