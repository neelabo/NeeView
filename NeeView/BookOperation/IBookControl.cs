using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ブックに対する操作
    /// </summary>
    public interface IBookControl : INotifyPropertyChanged, IDisposable
    {
        bool IsBusy { get; }
        PageSortModeClass PageSortModeClass { get; }
        bool IsBookmark { get; }
        string? Path { get; }
        int PendingCount { get; }

        bool CanOpenBookPlace();
        void OpenBookPlace();

        bool CanOpenExternalApp(IExternalApp parameter);
        void OpenExternalApp(IExternalApp parameter);

        bool CanCopyBookToFolder(DestinationFolder parameter);
        void CopyBookToFolder(DestinationFolder parameter);

        bool CanMoveBookToFolder(DestinationFolder parameter);
        void MoveBookToFolder(DestinationFolder parameter);

        bool CanCopyBookToClipboard();
        void CopyBookToClipboard();

        bool CanCutBookToClipboard();
        void CutBookToClipboard();

        bool CanRenameBook();
        void RenameBook();

        bool CanDeleteBook();
        void DeleteBook();

        void ReLoad();

        bool CanBookmark();
        void SetBookmark(bool isBookmark, string? parent);
        void ToggleBookmark(string? parent);
        bool IsBookmarkOn(string? parent);
    }
}
