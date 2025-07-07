using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public class HistoryPanelAccessor : LayoutPanelAccessor
    {
        private readonly HistoryPanel _panel;
        private readonly HistoryList _model;


        public HistoryPanelAccessor() : base(nameof(HistoryPanel))
        {
            _panel = (HistoryPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(HistoryPanel));
            _model = _panel.Presenter.HistoryList;
        }


        [WordNodeMember]
        public string SearchWord
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.HistoryListView.GetSearchBoxText()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.HistoryListView.SetSearchBoxText(value)); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.PanelListItemStyle = value.ToEnum<PanelListItemStyle>()); }
        }

        [WordNodeMember]
        public HistoryItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public HistoryItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private HistoryItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.HistoryListBox?.GetItems());
        }

        private HistoryItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.HistoryListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(HistoryItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? Array.Empty<HistoryItemAccessor>();
            _panel.Presenter.HistoryListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private static HistoryItemAccessor[] ToStringArray(IEnumerable<BookHistory>? items)
        {
            return items?.Select(e => new HistoryItemAccessor(e)).ToArray() ?? Array.Empty<HistoryItemAccessor>();
        }

    }
}
