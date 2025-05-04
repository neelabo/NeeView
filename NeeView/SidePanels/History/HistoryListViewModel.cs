using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeeLaboratory.Windows.Input;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryListViewModel : BindableBase
    {
        private readonly HistoryList _model;


        public HistoryListViewModel(HistoryList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(HistoryList.FilterPath), (s, e) => RaisePropertyChanged(nameof(FilterPath)));

            MoreMenuDescription = new HistoryListMoreMenuDescription(this);
        }


        public HistoryList Model => _model;

        public string FilterPath => string.IsNullOrEmpty(_model.FilterPath) ? Properties.TextResources.GetString("Word.AllHistory") : _model.FilterPath;

        public SearchBoxModel SearchBoxModel => _model.SearchBoxModel;


        #region MoreMenu

        public HistoryListMoreMenuDescription MoreMenuDescription { get; }

        public class HistoryListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private readonly HistoryListViewModel _vm;

            public HistoryListMoreMenuDescription(HistoryListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.TextResources.GetString("Word.StyleList"), PanelListItemStyle.Normal));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.TextResources.GetString("Word.StyleContent"), PanelListItemStyle.Content));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.TextResources.GetString("Word.StyleBanner"), PanelListItemStyle.Banner));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.TextResources.GetString("Word.StyleThumbnail"), PanelListItemStyle.Thumbnail));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckMenuItem(Properties.TextResources.GetString("History.MoreMenu.GroupBy"), new Binding(nameof(HistoryConfig.IsGroupBy)) { Source = Config.Current.History }));
                menu.Items.Add(CreateCheckMenuItem(Properties.TextResources.GetString("History.MoreMenu.IsCurrentFolder"), new Binding(nameof(HistoryConfig.IsCurrentFolder)) { Source = Config.Current.History }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(Properties.TextResources.GetString("History.MoreMenu.DeleteInvalid"), _vm.RemoveUnlinkedCommand));
                menu.Items.Add(CreateCommandMenuItem(Properties.TextResources.GetString("History.MoreMenu.DeleteAll"), _vm.RemoveAllCommand));
                return  menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.History);
            }
        }

        #endregion

        #region Commands

        private CancellationTokenSource? _removeUnlinkedCommandCancellationToken;
        private RelayCommand<PanelListItemStyle>? _setListItemStyle;
        private RelayCommand? _removeAllCommand;
        private RelayCommand? _removeUnlinkedCommand;

        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.History.PanelListItemStyle = style;
        }

        /// <summary>
        /// RemoveAllCommand command
        /// </summary>
        public RelayCommand RemoveAllCommand
        {
            get { return _removeAllCommand = _removeAllCommand ?? new RelayCommand(RemoveAll_Executed); }
        }

        private void RemoveAll_Executed()
        {
            if (BookHistoryCollection.Current.Items.Any())
            {
                var dialog = new MessageDialog(Properties.TextResources.GetString("HistoryDeleteAllDialog.Message"), Properties.TextResources.GetString("HistoryDeleteAllDialog.Title"));
                dialog.Commands.Add(UICommands.Delete);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer.Command != UICommands.Delete) return;
            }

            BookHistoryCollection.Current.Clear();
        }

        /// <summary>
        /// RemoveUnlinkedCommand command.
        /// </summary>
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        private async void RemoveUnlinkedCommand_Executed()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCommandCancellationToken?.Cancel();
            _removeUnlinkedCommandCancellationToken = new CancellationTokenSource();
            int count = await BookHistoryCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationToken.Token);
            BookHistoryCollection.Current.ShowRemovedMessage(count);
        }

        #endregion
    }
}
