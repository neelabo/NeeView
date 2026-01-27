using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class PlaylistListBoxInsertDropAssist : InsertDropAssist
    {
        private readonly ListBox _listBox;
        private readonly PlaylistListBoxViewModel _vm;

        public PlaylistListBoxInsertDropAssist(ListBox listBox, PlaylistListBoxViewModel vm)
            : base(listBox, new ListBoxDropAssistProfile())
        {
            _listBox = listBox;
            _vm = vm;
        }

        public sealed override void OnDragEnter(object? sender, DragEventArgs e)
        {
            SplitBrush = _listBox.Foreground;

            Orientation = Orientation.Vertical;
            AllowInsert = true;
            ReceiveItemType = InsertDropItemType.All;

            Orientation = _vm.PanelListItemStyle == PanelListItemStyle.Thumbnail ? Orientation.Horizontal : Orientation.Vertical;

            base.OnDragEnter(sender, e);
        }

    }
}
