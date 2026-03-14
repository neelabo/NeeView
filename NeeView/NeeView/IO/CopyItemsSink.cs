using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    internal class CopyItemsSink : FileOperationProgressSinkBase
    {
        public List<FolderOperatonItemResult> Results { get; } = new();

        public override void PostCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName, HRESULT hrCopy, IShellItem psiNewlyCreated)
        {
            if (hrCopy.Failed) return;

            var sourcePath = psiItem.GetPath();
            var destPath = psiNewlyCreated.GetPath();

            if (sourcePath != null && destPath != null)
            {
                Results.Add(new FolderOperatonItemResult(sourcePath, destPath));
            }
        }
    }
}

