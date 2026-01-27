using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public class PlaylistPanelAccessor : LayoutPanelAccessor
    {
        private readonly PlaylistPanel _panel;
        private readonly PlaylistHub _model;


        public PlaylistPanelAccessor() : base(nameof(PlaylistPanel))
        {
            _panel = (PlaylistPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(PlaylistPanel));
            _model = _panel.Presenter.PlaylistHub;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _model.SelectedItem; }
            set { AppDispatcher.Invoke(() => _model.SelectedItem = value); }
        }

        [WordNodeMember]
        public string Name
        {
            get { return _model.SelectedItemName; }
            set { AppDispatcher.Invoke(() => _model.SelectedItemName = value); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return Config.Current.Playlist.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => Config.Current.Playlist.PanelListItemStyle = value.ToEnum<PanelListItemStyle>()); }
        }

        [WordNodeMember]
        public PlaylistItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public PlaylistItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private PlaylistItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.PlaylistListBox?.GetItems());
        }

        private PlaylistItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.PlaylistListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(PlaylistItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? Array.Empty<PlaylistItemAccessor>();
            _panel.Presenter.PlaylistListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private static PlaylistItemAccessor[] ToStringArray(IEnumerable<PlaylistItem>? items)
        {
            return items?.Select(e => new PlaylistItemAccessor(e)).ToArray() ?? Array.Empty<PlaylistItemAccessor>();
        }

    }

}
