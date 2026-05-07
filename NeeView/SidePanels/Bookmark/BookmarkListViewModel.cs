using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Properties;
using NeeView.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// BookmarkList : ViewModel
    /// </summary>
    public partial class BookmarkListViewModel : ObservableObject
    {
        private readonly DpiScaleProvider _dpiProvider = new();
        private readonly BookmarkFolderList _model;
        private ContextMenu? _moreMenu;


        public BookmarkListViewModel(BookmarkFolderList model)
        {
            _model = model;

            _model.PlaceChanged +=
                (s, e) => MoveToUpCommand.NotifyCanExecuteChanged();

            _model.CollectionChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(FolderCollection));
                    OnPropertyChanged(nameof(FullPath));
                };

            _dpiProvider.DpiChanged +=
                (s, e) => OnPropertyChanged(nameof(DpiScale));

            MoreMenuDescription = new BookmarkListMoreMenu(this);
        }


        public BookmarkConfig BookmarkConfig => Config.Current.Bookmark;

        public FolderCollection? FolderCollection => _model.FolderCollection;

        public string FullPath
        {
            get { return _model.FolderCollection?.Place.FullPath ?? ""; }
            set { _model.MoveTo(new QueryPath(value)); }
        }

        public FolderList Model => _model;

        public SearchBoxModel? SearchBoxModel => _model.SearchBoxModel;

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList => AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();

        /// <summary>
        /// MoreMenu property.
        /// </summary>
        public ContextMenu? MoreMenu
        {
            get { return _moreMenu; }
            set { SetProperty(ref _moreMenu, value); }
        }

        public DpiScale DpiScale => _dpiProvider.DpiScale;

        #region Commands

        public string MoveToUpToolTip { get; } = CommandTools.CreateToolTipText("Bookmark.Up.ToolTip", Key.Up, ModifierKeys.Alt);


        [RelayCommand]
        private void MoveTo(QueryPath path)
        {
            _model.MoveTo(path);
        }

        private bool CanMoveToUp() => _model.CanMoveToParent();

        [RelayCommand(CanExecute = nameof(CanMoveToUp))]
        private void MoveToUp()
        {
            _model.MoveToParent();
        }

        [RelayCommand]
        private void SetFolderTreeLayout(FolderTreeLayout layout)
        {
            _model.FolderListConfig.FolderTreeLayout = layout;
            SidePanelFrame.Current.SetVisibleBookmarkFolderTree(true, true);
        }

        [RelayCommand]
        private void NewFolder()
        {
            _model.NewFolder();
        }

        [RelayCommand]
        private void AddBookmark()
        {
            _model.AddBookmark();
        }

        [RelayCommand]
        private async Task DeleteInvalidBookmark()
        {
            await _model.DeleteInvalidBookmark();
        }

        [RelayCommand]
        private void ToggleVisibleFoldersTree()
        {
            _model.FolderListConfig.IsFolderTreeVisible = !_model.FolderListConfig.IsFolderTreeVisible;
        }

        [RelayCommand]
        private void SetListItemStyle(PanelListItemStyle style)
        {
            _model.FolderListConfig.PanelListItemStyle = style;
        }

        [RelayCommand]
        private async Task Sync()
        {
            await _model.SyncAsync();
        }

        #endregion Commands

        #region MoreMenu

        public BookmarkListMoreMenu MoreMenuDescription { get; }

        public class BookmarkListMoreMenu : ItemsListMoreMenuDescription
        {
            private readonly BookmarkListViewModel _vm;

            public BookmarkListMoreMenu(BookmarkListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                var items = menu.Items;

                items.Clear();
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleList"), PanelListItemStyle.Normal));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleContent"), PanelListItemStyle.Content));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleBanner"), PanelListItemStyle.Banner));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleThumbnail"), PanelListItemStyle.Thumbnail));
                items.Add(new Separator());
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("BookmarkConfig.IsVisibleItemsCount"), new Binding(nameof(BookmarkConfig.IsVisibleItemsCount)) { Source = Config.Current.Bookmark }));
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("BookmarkConfig.IsVisibleSearchBox"), new Binding(nameof(BookmarkConfig.IsVisibleSearchBox)) { Source = Config.Current.Bookmark }));

                var subItem = new MenuItem() { Header = TextResources.GetString("Bookshelf.MoreMenu.SearchOptions") };
                subItem.Items.Add(CreateCheckMenuItem(TextResources.GetString("Bookshelf.MoreMenu.SearchIncludeSubdirectories"), new Binding(nameof(BookmarkConfig.IsSearchIncludeSubdirectories)) { Source = Config.Current.Bookmark }));
                items.Add(subItem);

                items.Add(new Separator());
                items.Add(CreateCommandMenuItem(TextResources.GetString("FolderTree.Menu.DeleteInvalidBookmark"), _vm.DeleteInvalidBookmarkCommand));
                items.Add(new Separator());
                items.Add(CreateCommandMenuItem(TextResources.GetString("Word.NewFolder"), _vm.NewFolderCommand));
                items.Add(CreateCommandMenuItem(TextResources.GetString("FolderTree.Menu.AddBookmark"), _vm.AddBookmarkCommand));
                items.Add(new Separator());
                items.Add(CreateCheckMenuItem(TextResources.GetString("BookmarkList.MoreMenu.SyncBookshelf"), new Binding(nameof(BookmarkConfig.IsSyncBookshelfEnabled)) { Source = Config.Current.Bookmark }));

                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyleCommand, style, _vm.Model.FolderListConfig);
            }

            private MenuItem CreateCheckableMenuItem(string header, Binding binding)
            {
                var menuItem = new MenuItem()
                {
                    Header = header,
                    IsCheckable = true,
                };
                menuItem.SetBinding(MenuItem.IsCheckedProperty, binding);
                return menuItem;
            }
        }

        #endregion MoreMenu


        public void SetDpiScale(DpiScale dpiScale)
        {
            _dpiProvider.SetDipScale(dpiScale);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }

    }
}
