using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class PlaylistListBoxDragReceiver : ListBoxDragReceiver
    {
        private readonly PlaylistListBoxViewModel _vm;

        public PlaylistListBoxDragReceiver(ListBox listBox, PlaylistListBoxViewModel vm) : base(listBox)
        {
            _vm = vm;
            SplitBrush = listBox.Foreground;
        }

        protected override void OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            base.OnPreviewDragEnter(sender, e);

            SplitBrush = _listBox.Foreground;

            Orientation = Orientation.Vertical;
            AllowInsert = true;
            ReceiveItemType = ListBoxDragReceiveItemType.All;

            Orientation = _vm.PanelListItemStyle == PanelListItemStyle.Thumbnail ? Orientation.Horizontal : Orientation.Vertical;

#if false
            if (_vm.FolderOrder.IsEntryCategory())
            {
                AllowInsert = true;
                ReceiveItemType = ListBoxDragReceiveItemType.All;
            }
            else
            {
                AllowInsert = false;
                ReceiveItemType = ListBoxDragReceiveItemType.Folder;
            }
#endif
        }
    }
}
