using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public partial class PageListViewModel : ObservableObject
    {
        private readonly PageList _pageList;
        private readonly PageListConfig _pageListConfig;


        public PageListViewModel(PageList pageList)
        {
            _pageList = pageList;
            _pageListConfig = Config.Current.PageList;

            _pageList.SubscribePropertyChanged(nameof(PageList.PageSortModeList),
                (s, e) => AppDispatcher.Invoke(() => OnPropertyChanged(nameof(PageSortModeList))));

            _pageList.SubscribePropertyChanged(nameof(PageList.PageSortMode),
                (s, e) => AppDispatcher.Invoke(() => OnPropertyChanged(nameof(PageSortMode))));

            _pageList.PageHistoryChanged +=
                (s, e) => AppDispatcher.Invoke(() => UpdateMoveToHistoryCommandCanExecute());

            _pageList.CollectionChanged +=
                (s, e) => AppDispatcher.Invoke(() => UpdateMoveToUpCommandCanExecute());

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

        public string MoveToPreviousCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Back.ToolTip", Key.Left, ModifierKeys.Alt);
        public string MoveToNextCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Next.ToolTip", Key.Right, ModifierKeys.Alt);
        public string MoveToUpCommandToolTip { get; } = CommandTools.CreateToolTipText("PageList.Up.ToolTip", Key.Up, ModifierKeys.Alt);


        private bool CanMoveToPrevious() => _pageList.CanMoveToPrevious();

        [RelayCommand(CanExecute = nameof(CanMoveToPrevious))]
        private void MoveToPrevious()
        {
            _pageList.MoveToPrevious();
        }

        private bool CanMoveToNext() => _pageList.CanMoveToNext();

        [RelayCommand(CanExecute = nameof(CanMoveToNext))]
        private void MoveToNext()
        {
            _pageList.MoveToNext();
        }

        [RelayCommand]
        private void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            _pageList.MoveToHistory(item);
        }

        private bool CanMoveToUp() => _pageList.CanMoveToParent();

        [RelayCommand(CanExecute = nameof(CanMoveToUp))]
        private void MoveToUp()
        {
            _pageList.MoveToParent();
        }

        [RelayCommand]
        private void SetListItemStyle(PanelListItemStyle style)
        {
            Config.Current.PageList.PanelListItemStyle = style;
        }

        [RelayCommand]
        private void ToggleVisibleFoldersTree()
        {
            _pageList.ToggleVisibleFoldersTree();
        }

        [RelayCommand]
        private void SetFolderTreeLayout(FolderTreeLayout layout)
        {
            _pageListConfig.FolderTreeLayout = layout;
            _pageListConfig.IsFolderTreeVisible = true;
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
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyleCommand, style, Config.Current.PageList);
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
            this.MoveToPreviousCommand.NotifyCanExecuteChanged();
            this.MoveToNextCommand.NotifyCanExecuteChanged();
        }

        private void UpdateMoveToUpCommandCanExecute()
        {
            this.MoveToUpCommand.NotifyCanExecuteChanged();
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
