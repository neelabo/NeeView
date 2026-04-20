using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class PageListViewModel : BindableBase
    {
        private readonly PageList _pageList;
        private readonly PageListConfig _pageListConfig;


        public PageListViewModel(PageList pageList)
        {
            _pageList = pageList;
            _pageListConfig = Config.Current.PageList;

            _pageList.AddPropertyChanged(nameof(PageList.PageSortModeList),
                (s, e) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(PageSortModeList))));

            _pageList.AddPropertyChanged(nameof(PageList.PageSortMode),
                (s, e) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(PageSortMode))));

            _pageList.PageHistoryChanged +=
                (s, e) => AppDispatcher.Invoke(() => UpdateMoveToHistoryCommandCanExecute());

            _pageList.CollectionChanged +=
                (s, e) => AppDispatcher.Invoke(() => UpdateMoveToUpCommandCanExecute());

            InitializeCommands();

            MoreMenuDescription = new PageListMoreMenuDescription(this);
        }


        public PageList Model => _pageList;

        public PageListConfig PageListConfig => Config.Current.PageList;

        public Dictionary<PageNameFormat, string> FormatList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageNameFormat>();

        public Dictionary<PageSortMode, string> PageSortModeList => _pageList.PageSortModeList;

        public PageSortMode PageSortMode
        {
            get => _pageList.PageSortMode;
            set => _pageList.PageSortMode = value;
        }

        public SearchBoxModel SearchBoxModel => _pageList.SearchBoxModel;

        #region Commands

        public RelayCommand MoveToPreviousCommand { get; private set; }
        public RelayCommand MoveToNextCommand { get; private set; }
        public RelayCommand<KeyValuePair<int, PageHistoryUnit>> MoveToHistoryCommand { get; private set; }
        public RelayCommand MoveToUpCommand { get; private set; }

        public string MoveToPreviousCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Back.ToolTip", Key.Left, ModifierKeys.Alt);
        public string MoveToNextCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Next.ToolTip", Key.Right, ModifierKeys.Alt);
        public string MoveToUpCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Up.ToolTip", Key.Up, ModifierKeys.Alt);


        [MemberNotNull(nameof(MoveToPreviousCommand), nameof(MoveToNextCommand), nameof(MoveToHistoryCommand), nameof(MoveToUpCommand))]
        private void InitializeCommands()
        {
            MoveToPreviousCommand = new RelayCommand(_pageList.MoveToPrevious, _pageList.CanMoveToPrevious);
            MoveToNextCommand = new RelayCommand(_pageList.MoveToNext, _pageList.CanMoveToNext);
            MoveToHistoryCommand = new RelayCommand<KeyValuePair<int, PageHistoryUnit>>(_pageList.MoveToHistory);
            MoveToUpCommand = new RelayCommand(_pageList.MoveToParent, _pageList.CanMoveToParent);
        }


        private RelayCommand<PanelListItemStyle>? _setListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.PageList.PanelListItemStyle = style;
        }

        private RelayCommand? _toggleVisibleFoldersTree;
        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _toggleVisibleFoldersTree = _toggleVisibleFoldersTree ?? new RelayCommand(_pageList.ToggleVisibleFoldersTree); }
        }

        private RelayCommand<FolderTreeLayout>? _setFolderTreeLayout;
        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _setFolderTreeLayout = _setFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _pageListConfig.FolderTreeLayout = layout;
                    _pageListConfig.IsFolderTreeVisible = true;
                }
            }
        }

        #endregion Commands

        #region MoreMenu

        public PageListMoreMenuDescription MoreMenuDescription { get; }

        public class PageListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private readonly PageListViewModel _vm;

            public PageListMoreMenuDescription(PageListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleList"), PanelListItemStyle.Normal));
                menu.Items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleContent"), PanelListItemStyle.Content));
                menu.Items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleBanner"), PanelListItemStyle.Banner));
                menu.Items.Add(CreateListItemStyleMenuItem(TextResources.GetString("Word.StyleThumbnail"), PanelListItemStyle.Thumbnail));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckMenuItem(TextResources.GetString("Menu.GroupBy"), new Binding(nameof(PageListConfig.IsGroupBy)) { Source = Config.Current.PageList }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("PageListConfig.ShowBookTitle"), new Binding(nameof(PageListConfig.ShowBookTitle)) { Source = Config.Current.PageList }));
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("PageListConfig.IsVisibleItemsCount"), new Binding(nameof(PageListConfig.IsVisibleItemsCount)) { Source = Config.Current.PageList }));
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("PageListConfig.IsVisibleSearchBox"), new Binding(nameof(PageListConfig.IsVisibleSearchBox)) { Source = Config.Current.PageList }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckableMenuItem(TextResources.GetString("PageListConfig.FocusMainView"), new Binding(nameof(PageListConfig.FocusMainView)) { Source = Config.Current.PageList }));
                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.PageList);
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

        #endregion

        public List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return _pageList.GetHistory(direction, size);
        }

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateMoveToHistoryCommandCanExecute()
        {
            this.MoveToPreviousCommand.RaiseCanExecuteChanged();
            this.MoveToNextCommand.RaiseCanExecuteChanged();
        }

        private void UpdateMoveToUpCommandCanExecute()
        {
            this.MoveToUpCommand.RaiseCanExecuteChanged();
        }
    }


    public class ViewItemsChangedEventArgs : EventArgs
    {
        public ViewItemsChangedEventArgs(List<Page> pages, int direction)
        {
            this.ViewItems = pages;
            this.Direction = direction;
        }

        public List<Page> ViewItems { get; set; }
        public int Direction { get; set; }
    }

}
