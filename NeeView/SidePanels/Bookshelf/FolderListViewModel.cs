using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FolderList : ViewModel
    /// </summary>
    public class FolderListViewModel : BindableBase
    {
        private readonly BookshelfFolderList _model;
        private Dictionary<FolderOrder, string> _folderOrderList = AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();
        private double _dpi = 1.0;


        public FolderListViewModel(BookshelfFolderList model)
        {
            _model = model;

            _model.History.Changed +=
                (s, e) => AppDispatcher.Invoke(() => UpdateCommandCanExecute());

            _model.PlaceChanged +=
                (s, e) => AppDispatcher.Invoke(() => MoveToUp.RaiseCanExecuteChanged());

            _model.CollectionChanged +=
                (s, e) => AppDispatcher.Invoke(() => Model_CollectionChanged(s, e));

            _model.AddPropertyChanged(nameof(_model.IsFolderTreeVisible),
                (s, e) => RaisePropertyChanged(nameof(IsFolderTreeVisible)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeLayout),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeLayout)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeAreaWidth),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeAreaWidth)));

            _model.AddPropertyChanged(nameof(_model.FolderTreeAreaHeight),
                (s, e) => RaisePropertyChanged(nameof(FolderTreeAreaHeight)));

            _model.AddPropertyChanged(nameof(_model.SearchBoxModel),
                (s, e) => RaisePropertyChanged(nameof(SearchBoxModel)));

            MoreMenuDescription = new FolderListMoreMenuDescription(this);
        }


        public BookshelfConfig BookshelfConfig => Config.Current.Bookshelf;

        public FolderCollection? FolderCollection => _model.FolderCollection;

        public BookshelfFolderList Model => _model;

        public SearchBoxModel? SearchBoxModel => _model.SearchBoxModel;


        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList
        {
            get { return _folderOrderList; }
            set { SetProperty(ref _folderOrderList, value); }
        }

        public FolderOrder FolderOrder
        {
            get { return FolderCollection != null ? FolderCollection.FolderParameter.FolderOrder : default; }
            set { if (FolderCollection != null) { FolderCollection.FolderParameter.FolderOrder = value; } }
        }

        public double Dpi
        {
            get { return _dpi; }
            set { SetProperty(ref _dpi, value); }
        }

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

        private RelayCommand? _setHome;
        private RelayCommand? _moveToHome;
        private RelayCommand<QueryPath>? _moveTo;
        private RelayCommand? _moveToPrevious;
        private RelayCommand? _moveToNext;
        private RelayCommand<KeyValuePair<int, QueryPath>>? _moveToHistory;
        private RelayCommand? _moveToUp;
        private RelayCommand? _sync;
        private RelayCommand? _toggleFolderRecursive;
        private RelayCommand? _addQuickAccess;
        private RelayCommand<FolderTreeLayout>? _setFolderTreeLayout;
        private RelayCommand? _newFolderCommand;
        private RelayCommand? _addBookmarkCommand;
        private RelayCommand<PanelListItemStyle>? _setListItemStyle;
        private RelayCommand? _toggleVisibleFoldersTree;
        private RelayCommand? _clearHistoryInPlace;

        public string MoveToHomeToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Home.ToolTip", Key.Home, ModifierKeys.Alt);
        public string MoveToPreviousToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Back.ToolTip", Key.Left, ModifierKeys.Alt);
        public string MoveToNextToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Next.ToolTip", Key.Right, ModifierKeys.Alt);
        public string MoveToUpToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Up.ToolTip", Key.Up, ModifierKeys.Alt);


        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _toggleVisibleFoldersTree = _toggleVisibleFoldersTree ?? new RelayCommand(_model.ToggleVisibleFoldersTree); }
        }

        public RelayCommand SetHome
        {
            get { return _setHome = _setHome ?? new RelayCommand(_model.SetHome, _model.CanSetHome); }
        }

        public RelayCommand MoveToHome
        {
            get { return _moveToHome = _moveToHome ?? new RelayCommand(_model.MoveToHome); }
        }

        public RelayCommand<QueryPath> MoveTo
        {
            get { return _moveTo = _moveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        public RelayCommand MoveToPrevious
        {
            get { return _moveToPrevious = _moveToPrevious ?? new RelayCommand(_model.MoveToPrevious, _model.CanMoveToPrevious); }
        }

        public RelayCommand MoveToNext
        {
            get { return _moveToNext = _moveToNext ?? new RelayCommand(_model.MoveToNext, _model.CanMoveToNext); }
        }

        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(_model.MoveToHistory); }
        }

        public RelayCommand MoveToUp
        {
            get { return _moveToUp = _moveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

        public RelayCommand Sync
        {
            get { return _sync = _sync ?? new RelayCommand(_model.Sync); }
        }

        public RelayCommand ToggleFolderRecursive
        {
            get { return _toggleFolderRecursive = _toggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive); }
        }

        public RelayCommand AddQuickAccess
        {
            get
            {
                return _addQuickAccess = _addQuickAccess ?? new RelayCommand(Execute, CanExecute);

                bool CanExecute()
                {
                    return _model.Place != null;
                }

                void Execute()
                {
                    _model.AddQuickAccess();
                }
            }
        }

        public RelayCommand ClearHistoryInPlace
        {
            get { return _clearHistoryInPlace = _clearHistoryInPlace ?? new RelayCommand(_model.ClearHistoryInPlace); }
        }

        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _setFolderTreeLayout = _setFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderListConfig.FolderTreeLayout = layout;
                    SidePanelFrame.Current.SetVisibleBookshelfFolderTree(true, true);
                }
            }
        }

        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand = _newFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.NewFolder();
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
                    _model.AddBookmark();
                }

            }
        }

        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get
            {
                return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(Execute);

                void Execute(PanelListItemStyle style)
                {
                    _model.FolderListConfig.PanelListItemStyle = style;
                }
            }
        }


        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }

        #endregion Commands

        #region MoreMenu

        public FolderListMoreMenuDescription MoreMenuDescription { get; }

        public class FolderListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private readonly FolderListViewModel _vm;

            public FolderListMoreMenuDescription(FolderListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                return Update(new ContextMenu());
            }

            [return: NotNullIfNotNull("menu")]
            public override ContextMenu Update(ContextMenu menu)
            {
                var items = menu.Items;

                items.Clear();
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleList"), PanelListItemStyle.Normal));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleContent"), PanelListItemStyle.Content));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleBanner"), PanelListItemStyle.Banner));
                items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleThumbnail"), PanelListItemStyle.Thumbnail));
                items.Add(new Separator());
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("BookshelfConfig.IsVisibleItemsCount"), new Binding(nameof(BookshelfConfig.IsVisibleItemsCount)) { Source = Config.Current.Bookshelf }));
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("BookshelfConfig.IsVisibleSearchBox"), new Binding(nameof(BookshelfConfig.IsVisibleSearchBox)) { Source = Config.Current.Bookshelf }));

                if (_vm._model.IsFolderSearchEnabled)
                {
                    var subItem = new MenuItem() { Header = TextResources.GetString("Bookshelf.MoreMenu.SearchOptions") };
                    //subItem.Items.Add(CreateCheckMenuItem(TextResources.GetString("Bookshelf.MoreMenu.SearchIncremental"), new Binding(nameof(SystemConfig.IsIncrementalSearchEnabled)) { Source = Config.Current.System }));
                    subItem.Items.Add(CreateCheckMenuItem(TextResources.GetString("Bookshelf.MoreMenu.SearchIncludeSubdirectories"), new Binding(nameof(BookshelfConfig.IsSearchIncludeSubdirectories)) { Source = Config.Current.Bookshelf }));
                    items.Add(subItem);
                }

                items.Add(new Separator());
                items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.AddQuickAccess"), _vm.AddQuickAccess));
                items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.ClearHistory"), "ClearHistoryInPlace"));

                switch (_vm._model.FolderCollection)
                {
                    case FolderEntryCollection:
                        items.Add(new Separator());
                        items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.Subfolder"), _vm.ToggleFolderRecursive, new Binding("FolderCollection.FolderParameter.IsFolderRecursive") { Source = _vm._model }));
                        break;

                    case FolderArchiveCollection:
                        break;

                    case FolderSearchCollection:
                        break;

                    case BookmarkFolderCollection:
                        items.Add(new Separator());
                        items.Add(CreateCommandMenuItem(TextResources.GetString("Word.NewFolder"), _vm.NewFolderCommand));
                        items.Add(CreateCommandMenuItem(TextResources.GetString("FolderTree.Menu.AddBookmark"), _vm.AddBookmarkCommand));
                        break;
                }

                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, _vm._model.FolderListConfig);
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


        /// <summary>
        /// Model CollectionChanged event
        /// </summary>
        private void Model_CollectionChanged(object? sender, EventArgs e)
        {
            UpdateFolderOrderList();
            RaisePropertyChanged(nameof(FolderCollection));
        }

        /// <summary>
        /// 前履歴取得
        /// </summary>
        internal List<KeyValuePair<int, QueryPath>> GetPreviousHistory()
        {
            return _model.GetPreviousHistory();
        }

        /// <summary>
        /// 次履歴取得
        /// </summary>
        internal List<KeyValuePair<int, QueryPath>> GetNextHistory()
        {
            return _model.GetNextHistory();
        }

        /// <summary>
        /// 並び順リスト更新
        /// </summary>
        public void UpdateFolderOrderList()
        {
            if (FolderCollection is null) return;

            FolderOrderList = FolderCollection.FolderOrderClass.GetFolderOrderMap();
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="folderInfo"></param>
        public void MoveToSafety(FolderItem folderInfo)
        {
            if (folderInfo != null && folderInfo.CanOpenFolder())
            {
                _model.MoveTo(folderInfo.TargetPath);
            }
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }
    }


}
