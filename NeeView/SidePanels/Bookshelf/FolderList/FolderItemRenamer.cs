using NeeView.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    public class FolderItemRenamer : ListBoxItemRenamer<FolderItem>
    {
        public FolderItemRenamer(ListBox listBox, IToolTipService? toolTipService) : base(listBox, toolTipService)
        {
        }

        protected override RenameControl CreateRenameControl(ListBox listBox, FolderItem item)
        {
            return new FolderItemRenameControl(listBox, item);
        }
    }

    public class FolderItemRenameControl : ListBoxItemRenameControl<FolderItem>
    {
        public FolderItemRenameControl(ListBox listBox, FolderItem item) : base(listBox, item)
        {
            if (item.IsFileSystem())
            {
                this.IsInvalidFileNameChars = true;
                this.IsSelectFileNameBody = !item.IsDirectory;
                this.IsHideExtension = item.IsHideExtension();
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
            {
                this.IsInvalidSeparatorChars = true;
            }
        }

        protected override async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            if (oldValue == newValue) return true;

            return await _item.RenameAsync(newValue);
        }
    }


}
