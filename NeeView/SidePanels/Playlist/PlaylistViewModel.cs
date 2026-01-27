using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class PlaylistViewModel : BindableBase
    {
        private readonly PlaylistHub _model;


        public PlaylistViewModel(PlaylistHub model)
        {
            _model = model;

            MoreMenuDescription = new PlaylistMoreMenuDescription(this);

            _model.AddPropertyChanged(nameof(_model.PlaylistFiles), Model_PlaylistFilesChanged);
            _model.AddPropertyChanged(nameof(_model.SelectedItem), Model_SelectedItemChanged);
            _model.AddPropertyChanged(nameof(_model.FilterMessage), (s, e) => RaisePropertyChanged(nameof(FilterMessage)));
        }


        public void UpdatePlaylistCollection()
        {
            _model.UpdatePlaylistCollection();
        }


        public EventHandler? RenameRequest;


        public List<object> PlaylistFiles
        {
            get => _model.PlaylistFiles;
        }

        public string SelectedItem
        {
            get => _model.SelectedItem;
            set => _model.SelectedItem = value;
        }

        public string? FilterMessage
        {
            get => _model.FilterMessage;
        }



        private void Model_PlaylistFilesChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PlaylistFiles));
        }

        private void Model_SelectedItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedItem));
            DeleteCommand.RaiseCanExecuteChanged();
            RenameCommand.RaiseCanExecuteChanged();
        }



        #region MoreMenu

        public PlaylistMoreMenuDescription MoreMenuDescription { get; }

        public class PlaylistMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private readonly PlaylistViewModel _vm;

            public PlaylistMoreMenuDescription(PlaylistViewModel vm)
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
                menu.Items.Add(CreateCheckMenuItem(TextResources.GetString("Menu.GroupBy"), new Binding(nameof(PlaylistConfig.IsGroupBy)) { Source = Config.Current.Playlist }));
                menu.Items.Add(CreateCheckMenuItem(TextResources.GetString("Playlist.MoreMenu.CurrentBook"), new Binding(nameof(PlaylistConfig.IsCurrentBookFilterEnabled)) { Source = Config.Current.Playlist }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.New"), _vm.NewCommand));
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.Open"), _vm.OpenCommand));
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.Delete"), _vm.DeleteCommand));
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.Rename"), _vm.RenameCommand));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.DeleteInvalid"), _vm.DeleteInvalidItemsCommand));
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.Sort"), _vm.SortItemsCommand));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(TextResources.GetString("Playlist.MoreMenu.OpenAsBook"), _vm.OpenAsBookCommand));

                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.Playlist);
            }
        }

        #endregion

        #region Commands

        private RelayCommand<PanelListItemStyle>? _setListItemStyle;

        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.Playlist.PanelListItemStyle = style;
        }


        private RelayCommand? _NewCommand;
        public RelayCommand NewCommand
        {
            get { return _NewCommand = _NewCommand ?? new RelayCommand(NewCommand_Execute); }
        }

        private void NewCommand_Execute()
        {
            _model.CreateNew();
            ////RenameRequest?.Invoke(this, null);
        }


        private RelayCommand? _OpenCommand;
        public RelayCommand OpenCommand
        {
            get { return _OpenCommand = _OpenCommand ?? new RelayCommand(OpenCommand_Execute); }
        }

        private void OpenCommand_Execute()
        {
            _model.Open();
        }

        private RelayCommand? _DeleteCommand;
        public RelayCommand DeleteCommand
        {
            get { return _DeleteCommand = _DeleteCommand ?? new RelayCommand(DeleteCommand_Execute, DeleteCommand_CanExecute); }
        }

        private bool DeleteCommand_CanExecute()
        {
            return _model.CanDelete();
        }

        private async void DeleteCommand_Execute()
        {
            await _model.DeleteAsync();
        }


        private RelayCommand? _RenameCommand;
        public RelayCommand RenameCommand
        {
            get { return _RenameCommand = _RenameCommand ?? new RelayCommand(RenameCommand_Execute, RenameCommand_CanExecute); }
        }

        private bool RenameCommand_CanExecute()
        {
            return _model.CanRename();
        }

        private void RenameCommand_Execute()
        {
            RenameRequest?.Invoke(this, EventArgs.Empty);
        }

        public bool Rename(string newName)
        {
            return _model.Rename(newName);
        }


        private RelayCommand? _OpenAsBookCommand;
        public RelayCommand OpenAsBookCommand
        {
            get { return _OpenAsBookCommand = _OpenAsBookCommand ?? new RelayCommand(OpenAsBookCommand_Execute); }
        }

        private void OpenAsBookCommand_Execute()
        {
            _model.OpenAsBook();
        }

        private RelayCommand? _DeleteInvalidItemsCommand;
        public RelayCommand DeleteInvalidItemsCommand
        {
            get { return _DeleteInvalidItemsCommand = _DeleteInvalidItemsCommand ?? new RelayCommand(DeleteInvalidItemsCommand_Execute); }
        }

        private async void DeleteInvalidItemsCommand_Execute()
        {
            await _model.DeleteInvalidItemsAsync();
        }


        private RelayCommand? _SortItemsCommand;
        public RelayCommand SortItemsCommand
        {
            get { return _SortItemsCommand = _SortItemsCommand ?? new RelayCommand(SortItemsCommand_Execute); }
        }

        private void SortItemsCommand_Execute()
        {
            var dialog = new MessageDialog(TextResources.GetString("PlaylistSortDialog.Message"), TextResources.GetString("PlaylistSortDialog.Title"));
            dialog.Commands.AddRange(UICommands.OKCancel);
            var result = dialog.ShowDialog();
            if (!result.IsPossible)
            {
                return;
            }

            _model.SortItems();
        }

        #endregion
    }
}
