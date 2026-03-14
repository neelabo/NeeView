using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    internal class MoveItemsSink : FileOperationProgressSinkBase
    {
        public List<FolderOperatonItemResult> Results { get; } = new();

        public override void PostMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName, HRESULT hrMove, IShellItem psiNewlyCreated)
        {
            if (hrMove.Failed) return;

            var sourcePath = psiItem.GetPath();
            var destPath = psiNewlyCreated.GetPath();

            if (sourcePath != null && destPath != null)
            {
                Results.Add(new FolderOperatonItemResult(sourcePath, destPath));
            }
        }
    }
}

