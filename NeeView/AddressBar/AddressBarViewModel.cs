using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Windows.Data;
using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// AddressBar : ViewModel
    /// </summary>
    public partial class AddressBarViewModel : ObservableObject
    {
        private AddressBar _model;
        private readonly DelayValue<bool> _isLoading;


        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            _isLoading = new DelayValue<bool>();
            _isLoading.ValueChanged += (s, e) => OnPropertyChanged(nameof(IsLoading));
            PageFrameBoxPresenter.Current.Loading += Presenter_Loading;
            BookOperation.Current.BookChanged += (s, e) =>
            {
                ToggleBookmarkCommand.NotifyCanExecuteChanged();
                MoveToParentBookCommand.NotifyCanExecuteChanged();
            };
        }


        public AddressBar Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public Config Config => Config.Current;

        public SlideShow SlideShow => SlideShow.Current;

        public bool IsLoading => _isLoading.Value;

        public Dictionary<string, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }

        public BookSettingConfig BookSetting
        {
            get { return Config.Current.BookSetting; }
        }

        public string PanoramaOff => CommandTable.Current.GetElement("ToggleIsPanorama").GetStateExecuteMessage(false);
        public string PanoramaOn => CommandTable.Current.GetElement("ToggleIsPanorama").GetStateExecuteMessage(true);


        public List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
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

        [RelayCommand]
        private void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }

        [RelayCommand]
        private void ToggleBookLock()
        {
            _model.IsBookLocked = !_model.IsBookLocked;
        }

        [RelayCommand]
        private void TogglePageMode()
        {
            BookSettings.Current.TogglePageMode(+1, true);
        }

        private bool CanToggleBookmark()
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }

        [RelayCommand(CanExecute = nameof(CanToggleBookmark))]
        private void ToggleBookmark()
        {
            BookOperation.Current.BookControl.ToggleBookmark();
        }

        private bool CanMoveToParentBook()
        {
            return BookHub.Current.CanLoadParent();
        }

        [RelayCommand(CanExecute = nameof(CanMoveToParentBook))]
        private void MoveToParentBook()
        {
            BookHub.Current.RequestLoadParent(this);
        }

        [RelayCommand]
        private void ToggleSettingWindow()
        {
            MainWindowModel.Current.ToggleSettingWindow();
        }
    }
}
