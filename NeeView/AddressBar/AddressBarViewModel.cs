using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Windows.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// AddressBar : ViewModel
    /// </summary>
    public class AddressBarViewModel : BindableBase
    {
        private AddressBar _model;
        private RelayCommand<KeyValuePair<int, QueryPath>>? _moveToHistory;
        private RelayCommand? _toggleBookLockCommand;
        private RelayCommand? _togglePageModeCommand;
        private RelayCommand? _toggleBookmarkCommand;
        private RelayCommand? _moveToParentBookCommand;
        private RelayCommand? _toggleSettingWindowCommand;
        private readonly DelayValue<bool> _isLoading;


        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            _isLoading = new DelayValue<bool>();
            _isLoading.ValueChanged += (s, e) => RaisePropertyChanged(nameof(IsLoading));
            PageFrameBoxPresenter.Current.Loading += Presenter_Loading;
            BookOperation.Current.BookChanged += (s, e) =>
            {
                ToggleBookmarkCommand.RaiseCanExecuteChanged();
                MoveToParentBookCommand.RaiseCanExecuteChanged();
            };
        }


        public AddressBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public Config Config => Config.Current;

        public bool IsLoading => _isLoading.Value;

        public Dictionary<string, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }

        public BookSettingConfig BookSetting
        {
            get { return Config.Current.BookSetting; }
        }


        public List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
        }


        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(MoveToHistory_Executed); }
        }

        public RelayCommand ToggleBookLockCommand
        {
            get { return _toggleBookLockCommand = _toggleBookLockCommand ?? new RelayCommand(ToggleBookLockCommand_Execute); }
        }

        public RelayCommand TogglePageModeCommand
        {
            get { return _togglePageModeCommand = _togglePageModeCommand ?? new RelayCommand(TogglePageModeCommand_Execute); }
        }

        public RelayCommand ToggleBookmarkCommand
        {
            get { return _toggleBookmarkCommand = _toggleBookmarkCommand ?? new RelayCommand(ToggleBookmarkCommand_Execute, ToggleBookmarkCommand_CanExecute); }
        }

        public RelayCommand MoveToParentBookCommand
        {
            get { return _moveToParentBookCommand = _moveToParentBookCommand ?? new RelayCommand(MoveToParentBookCommand_Execute, MoveToParentBookCommand_CanExecute); }
        }

        public RelayCommand ToggleSettingWindowCommand
        {
            get { return _toggleSettingWindowCommand ??= new RelayCommand(ToggleSettingWindowCommand_Execute); }
        }


        private void Presenter_Loading(object? sender, BookPathEventArgs e)
        {
            if (e.Path != null)
            {
                _isLoading.SetValue(true, 1000);
            }
            else
            {
                _isLoading.SetValue(false, 0);
            }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }

        private void ToggleBookLockCommand_Execute()
        {
            _model.IsBookLocked = !_model.IsBookLocked;
        }

        private void TogglePageModeCommand_Execute()
        {
            BookSettings.Current.TogglePageMode(+1, true);
        }

        private bool ToggleBookmarkCommand_CanExecute()
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }

        private void ToggleBookmarkCommand_Execute()
        {
            BookOperation.Current.BookControl.ToggleBookmark();
        }

        private bool MoveToParentBookCommand_CanExecute()
        {
            return BookHub.Current.CanLoadParent();
        }

        private void MoveToParentBookCommand_Execute()
        {
            BookHub.Current.RequestLoadParent(this);
        }

        private void ToggleSettingWindowCommand_Execute()
        {
            MainWindowModel.Current.ToggleSettingWindow();
        }
    }
}
