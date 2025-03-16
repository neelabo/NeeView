using System.ComponentModel;

namespace NeeView
{
    public class PageListPresenter
    {
        private readonly LazyEx<PageListView> _pageListView;
        private readonly PageList _pageList;
        private readonly PageListBoxViewModel _listBoxViewModel;
        private PageListBox? _pageListBox;


        public PageListPresenter(LazyEx<PageListView> pageListView, PageList pageList)
        {
            _pageListView = pageListView;
            _pageList = pageList;

            _listBoxViewModel = new PageListBoxViewModel(_pageList);

            Config.Current.PageList.AddPropertyChanged(nameof(PageListConfig.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            _pageListView.Created += (s, e) => UpdateListBoxContent();
        }


        public PageList PageList => _pageList;

        public PageListView PageListView => _pageListView.Value;

        public PageListBox? PageListBox => _pageListBox ?? _pageListView.Value.ListBoxContent.Content as PageListBox;


        private void UpdateListBoxContent(bool rebuild = true)
        {
            if (rebuild || _pageListBox is null)
            {
                _pageListBox = new PageListBox(_listBoxViewModel);
            }
            if (_pageListView.IsValueCreated)
            {
                _pageListView.Value.ListBoxContent.Content = _pageListBox;
            }
        }

        public void FocusAtOnce()
        {
            _listBoxViewModel.FocusAtOnce = true;
        }

    }
}
