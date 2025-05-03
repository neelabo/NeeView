using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class FolderListBoxDragReceiver : ListBoxDragReceiver
    {
        private readonly FolderListBoxViewModel _vm;

        public FolderListBoxDragReceiver(ListBox listBox, FolderListBoxViewModel vm) : base(listBox)
        {
            _vm = vm;
            SplitBrush = listBox.Foreground;
        }

        protected override void OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            base.OnPreviewDragEnter(sender, e);

            SplitBrush = _listBox.Foreground;

            Orientation = _vm.Model.PanelListItemStyle == PanelListItemStyle.Thumbnail ? Orientation.Horizontal : Orientation.Vertical;

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
        }

        protected override bool IsFolder(ListBoxItem? item)
        {
            var targetItem = item?.Content as FolderItem;
            return targetItem is not null && targetItem.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory);
        }
    }
}
