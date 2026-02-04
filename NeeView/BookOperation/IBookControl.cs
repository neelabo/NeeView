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
