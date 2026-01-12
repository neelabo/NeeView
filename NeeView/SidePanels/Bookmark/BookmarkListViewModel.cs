using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    public class BookmarkListViewModel : BindableBase
    {
        private readonly DpiScaleProvider _dpiProvider = new();
        private readonly BookmarkFolderList _model;
        private ContextMenu? _moreMenu;


        public BookmarkListViewModel(BookmarkFolderList model)
        {
            _model = model;

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(FolderCollection));
                    RaisePropertyChanged(nameof(FullPath));
                };

            _dpiProvider.DpiChanged +=
                (s, e) => RaisePropertyChanged(nameof(DpiScale));

            _model.AddPropertyChanged(nameof(_model.IsFolderTreeVisible),
                (s, e) => RaisePropertyChanged(nameof(IsFolderTreeVisible)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeLayout),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeLayout)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeAreaWidth),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeAreaWidth)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeAreaHeight),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeAreaHeight)));

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
            set { if (_moreMenu != value) { _moreMenu = value; RaisePropertyChanged(); } }
        }

        public DpiScale DpiScale => _dpiProvider.DpiScale;

        public bool IsFolderTreeVisible
        {
            get => _model.IsFolderTreeVisible;
            set => _model.IsFolderTreeVisible = value;
        }

        public FolderTreeLayout FolderTreeLayout
        {
            get => _model.FolderTreeLayout;
            set => _model.FolderTreeLayout = value;
        }

        public GridLength FolderTreeAreaWidth
        {
            get => new(_model.FolderTreeAreaWidth);
            set => _model.FolderTreeAreaWidth = value.Value;
        }

        public GridLength FolderTreeAreaHeight
        {
            get => new(_model.FolderTreeAreaHeight);
            set => _model.FolderTreeAreaHeight = value.Value;
        }


        #region Commands

        public string MoveToUpToolTip { get; } = CommandTools.CreateToolTipText("Bookmark.Up.ToolTip", Key.Up, ModifierKeys.Alt);

        /// <summary>
        /// MoveTo command.
        /// </summary>
        private RelayCommand<QueryPath>? _MoveTo;
        public RelayCommand<QueryPath> MoveTo
        {
            get { return _MoveTo = _MoveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand? _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

        private RelayCommand<FolderTreeLayout>? _SetFolderTreeLayout;
        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _SetFolderTreeLayout = _SetFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderListConfig.FolderTreeLayout = layout;
                    SidePanelFrame.Current.SetVisibleBookmarkFolderTree(true, true);
                }
            }
        }

        private RelayCommand? _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get { return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(NewFolderCommand_Executed); }
        }

        private void NewFolderCommand_Executed()
        {
            _model.NewFolder();
        }


        private RelayCommand? _AddBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get { return _AddBookmarkCommand = _AddBookmarkCommand ?? new RelayCommand(AddBookmarkCommand_Executed); }
        }

        private void AddBookmarkCommand_Executed()
        {
            _model.AddBookmark();
        }


        private RelayCommand? _deleteInvalidBookmarkCommand;
        public RelayCommand DeleteInvalidBookmarkCommand
        {
            get { return _deleteInvalidBookmarkCommand = _deleteInvalidBookmarkCommand ?? new RelayCommand(DeleteInvalidBookmark); }
        }

        private async void DeleteInvalidBookmark()
        {
            await _model.DeleteInvalidBookmark();
        }

        private RelayCommand? _ToggleVisibleFoldersTree;
        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _ToggleVisibleFoldersTree = _ToggleVisibleFoldersTree ?? new RelayCommand(ToggleVisibleFoldersTree_Executed); }
        }

        private void ToggleVisibleFoldersTree_Executed()
        {
            _model.FolderListConfig.IsFolderTreeVisible = !_model.FolderListConfig.IsFolderTreeVisible;
        }

        private RelayCommand<PanelListItemStyle>? _SetListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.FolderListConfig.PanelListItemStyle = style;
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
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, _vm.Model.FolderListConfig);
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
