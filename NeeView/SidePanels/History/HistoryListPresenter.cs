namespace NeeView
{
    public class HistoryListPresenter
    {
        private readonly LazyEx<HistoryListView> _historyListView;
        private readonly HistoryList _historyList;
        private readonly HistoryListBoxViewModel _historyListBoxViewModel;
        private HistoryListBox? _historyListBox;


        public HistoryListPresenter(LazyEx<HistoryListView> historyListView, HistoryList historyList)
        {
            _historyListView = historyListView;
            _historyList = historyList;

            _historyListBoxViewModel = new HistoryListBoxViewModel(historyList);

            _historyListView.Created +=
                (s, e) => UpdateListBoxContent(false);

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.PanelListItemStyle),
                (s, e) => UpdateListBoxContent());
        }


        public HistoryListView HistoryListView => _historyListView.Value;
        public HistoryListBox? HistoryListBox => _historyListBox ?? _historyListView.Value.ListBoxContent.Content as HistoryListBox;
        public HistoryList HistoryList => _historyList;


        private void UpdateListBoxContent(bool rebuild = true)
        {
            if (rebuild || _historyListBox is null)
            {
                _historyListBox = new HistoryListBox(_historyListBoxViewModel);
            }
            if (_historyListView.IsValueCreated)
            {
                _historyListView.Value.ListBoxContent.Content = _historyListBox;
            }
        }

        public void Refresh()
        {
            HistoryListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            HistoryListBox?.FocusAtOnce();
        }
    }
}
