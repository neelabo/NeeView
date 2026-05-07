using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FolderList : ViewModel
    /// </summary>
    public partial class FolderListViewModel : ObservableObject
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
                (s, e) => AppDispatcher.Invoke(() => MoveToUpCommand.NotifyCanExecuteChanged());

            _model.CollectionChanged +=
                (s, e) => AppDispatcher.Invoke(() => Model_CollectionChanged(s, e));

            _model.SubscribePropertyChanged(nameof(_model.SearchBoxModel),
                (s, e) => OnPropertyChanged(nameof(SearchBoxModel)));

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

        #region Commands

        public string MoveToHomeToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Home.ToolTip", Key.Home, ModifierKeys.Alt);
        public string MoveToPreviousToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Back.ToolTip", Key.Left, ModifierKeys.Alt);
        public string MoveToNextToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Next.ToolTip", Key.Right, ModifierKeys.Alt);
        public string MoveToUpToolTip { get; } = CommandTools.CreateToolTipText("Bookshelf.Up.ToolTip", Key.Up, ModifierKeys.Alt);


        [RelayCommand]
        private void ToggleVisibleFoldersTree()
        {
            _model.ToggleVisibleFoldersTree();
        }

        private bool CanSetHome() => _model.CanSetHome();

        [RelayCommand(CanExecute = nameof(CanSetHome))]
        private void SetHome()
        {
            _model.SetHome();
        }

        [RelayCommand]
        private void MoveToHome()
        {
            _model.MoveToHome();
        }

        [RelayCommand]
        private void MoveTo(QueryPath path)
        {
            _model.MoveTo(path);
        }

        private bool CanMoveToPrevious() => _model.CanMoveToPrevious();

        [RelayCommand(CanExecute = nameof(CanMoveToPrevious))]
        private void MoveToPrevious()
        {
            _model.MoveToPrevious();
        }

        private bool CanMoveToNext() => _model.CanMoveToNext();

        [RelayCommand(CanExecute = nameof(CanMoveToNext))]
        private void MoveToNext()
        {
            _model.MoveToNext();
        }

        [RelayCommand]
        private void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            _model.MoveToHistory(item);
        }

        private bool CanMoveToParent() => _model.CanMoveToParent();

        [RelayCommand(CanExecute = nameof(CanMoveToParent))]
        private void MoveToUp()
        {
            _model.MoveToParent();
        }

        [RelayCommand]
        private async Task Sync()
        {
            await _model.SyncAsync();
        }

        [RelayCommand]
        private void ToggleFolderRecursive()
        {
            _model.ToggleFolderRecursive();
        }

        private bool CanAddQuickAccess() => _model.Place != null;

        [RelayCommand(CanExecute = nameof(CanAddQuickAccess))]
        private void AddQuickAccess()
        {
            _model.AddQuickAccess();
        }

        [RelayCommand]
        private void ClearHistoryInPlace()
        {
            _model.ClearHistoryInPlace();
        }

        [RelayCommand]
        private void SetFolderTreeLayout(FolderTreeLayout layout)
        {
            _model.FolderListConfig.FolderTreeLayout = layout;
            SidePanelFrame.Current.SetVisibleBookshelfFolderTree(true, true);
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
        private void SetListItemStyle(PanelListItemStyle style)
        {
            _model.FolderListConfig.PanelListItemStyle = style;
        }

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPreviousCommand.NotifyCanExecuteChanged();
            this.MoveToNextCommand.NotifyCanExecuteChanged();
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
                items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.AddQuickAccess"), _vm.AddQuickAccessCommand));
                items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.ClearHistory"), "ClearHistoryInPlace"));

                switch (_vm._model.FolderCollection)
                {
                    case FolderEntryCollection:
                        items.Add(new Separator());
                        items.Add(CreateCommandMenuItem(TextResources.GetString("Bookshelf.MoreMenu.Subfolder"), _vm.ToggleFolderRecursiveCommand, new Binding("FolderCollection.FolderParameter.IsFolderRecursive") { Source = _vm._model }));
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
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyleCommand, style, _vm._model.FolderListConfig);
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
            OnPropertyChanged(nameof(FolderCollection));
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
            OnPropertyChanged(nameof(FolderOrder));
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
