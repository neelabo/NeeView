using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace NeeView.IO
{
    /// <summary>
    /// IFileOperationProgressSink のすべてのメソッドをデフォルト実装（何もしない）したクラス。
    /// 必要なメソッドだけを継承先で override して使用します。
    /// </summary>
    internal abstract class FileOperationProgressSinkBase : IFileOperationProgressSink
    {
        public virtual void StartOperations() { }
        public virtual void FinishOperations(HRESULT hrStatus) { }

        public virtual void PreRenameItem(uint dwFlags, IShellItem psiItem, PCWSTR pszNewName) { }
        public virtual void PostRenameItem(uint dwFlags, IShellItem psiItem, PCWSTR pszNewName, HRESULT hrRename, IShellItem psiNewlyCreated) { }

        public virtual void PreMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName) { }
        public virtual void PostMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName, HRESULT hrMove, IShellItem psiNewlyCreated) { }

        public virtual void PreCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName) { }
        public virtual void PostCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, PCWSTR pszNewName, HRESULT hrCopy, IShellItem psiNewlyCreated) { }

        public virtual void PreDeleteItem(uint dwFlags, IShellItem psiItem) { }
        public virtual void PostDeleteItem(uint dwFlags, IShellItem psiItem, HRESULT hrDelete, IShellItem psiNewlyCreated) { }

        public virtual void PreNewItem(uint dwFlags, IShellItem psiDestinationFolder, PCWSTR pszNewName) { }
        public virtual void PostNewItem(uint dwFlags, IShellItem psiDestinationFolder, PCWSTR pszNewName, PCWSTR pszTemplateName, uint dwFileAttributes, HRESULT hrNew, IShellItem psiNewlyCreated) { }

        public virtual void UpdateProgress(uint uWorkComplete, uint uWorkTotal) { }
        public virtual void ResetTimer() { }
        public virtual void PauseTimer() { }
        public virtual void ResumeTimer() { }
    }
}

