using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    internal class DeleteItemsSink : FileOperationProgressSinkBase
    {
        public List<FolderOperatonItemResult> Results { get; } = new();

        public override void PostDeleteItem(uint dwFlags, IShellItem psiItem, HRESULT hrDelete, IShellItem psiNewlyCreated)
        {
            if (hrDelete.Failed) return;

            var sourcePath = psiItem.GetPath();
            var destPath = psiNewlyCreated.GetPath() ?? "";

            if (sourcePath != null)
            {
                Results.Add(new FolderOperatonItemResult(sourcePath, destPath));
            }
        }
    }
}

