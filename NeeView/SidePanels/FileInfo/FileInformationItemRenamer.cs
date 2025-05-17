using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// 情報ページ用名前変更機能
    /// </summary>
    public class FileInformationItemRenamer : ListBoxItemRenamer<FileInformationSource>
    {
        public FileInformationItemRenamer(ListBox listBox, IToolTipService? toolTipService) : base(listBox, toolTipService, false)
        {
        }

        public event EventHandler? ArchiveChanged;

        protected override RenameControl CreateRenameControl(ListBox listBox, FileInformationSource item)
        {
            var control = new FileInformationItemRenameControl(listBox, item);
            control.ArchiveChanged += (s, e) => ArchiveChanged?.Invoke(s, e);
            return control;
        }
    }


    public class FileInformationItemRenameControl : ListBoxItemRenameControl<FileInformationSource>
    {
        private readonly bool _isFileSystem;

        public FileInformationItemRenameControl(ListBox listBox, FileInformationSource item) : base(listBox, item, false)
        {
            if (item.Page.ArchiveEntry.IsFileSystem)
            {
                this.IsSelectFileNameBody = !item.Page.ArchiveEntry.IsDirectory;
                this.IsInvalidFileNameChars = true;
                _isFileSystem = true;
            }
            else
            {
                this.IsInvalidSeparatorChars = true;
                _isFileSystem = false;
            }
        }

        public event EventHandler? ArchiveChanged;

        protected override async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            Debug.Assert(oldValue != newValue);
            var isSuccess = await _item.RenameAsync(newValue);
            if (isSuccess && !_isFileSystem)
            {
                ArchiveChanged?.Invoke(this, EventArgs.Empty);
            }
            return isSuccess;
        }
    }
}
