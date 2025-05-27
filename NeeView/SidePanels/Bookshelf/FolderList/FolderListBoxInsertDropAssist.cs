using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class FolderListBoxInsertDropAssist : InsertDropAssist
    {
        private readonly ListBox _listBox;
        private readonly FolderListBoxViewModel _vm;

        public FolderListBoxInsertDropAssist(ListBox listBox, FolderListBoxViewModel vm)
            : base(listBox, new FolderListBoxDropAssistProfile())
        {
            _listBox = listBox;
            _vm = vm;
        }

        public sealed override void OnDragEnter(object? sender, DragEventArgs e)
        {
            SplitBrush = _listBox.Foreground;

            Orientation = _vm.Model.PanelListItemStyle == PanelListItemStyle.Thumbnail ? Orientation.Horizontal : Orientation.Vertical;

            if (_vm.FolderOrder.IsEntryCategory())
            {
                AllowInsert = true;
                ReceiveItemType = InsertDropItemType.All;
            }
            else
            {
                AllowInsert = false;
                ReceiveItemType = InsertDropItemType.Folder;
            }

            base.OnDragEnter(sender, e);
        }
    }


    public class FolderListBoxDropAssistProfile : ListBoxDropAssistProfile
    {
        public override bool IsFolder(FrameworkElement? item)
        {
            var targetItem = (item as ListBoxItem)?.Content as FolderItem;
            return targetItem is not null && targetItem.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory);
        }
    }
}
