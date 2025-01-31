using System;
using System.Collections.Generic;
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

        void DisposeViewContent(IEnumerable<Page> pages);
        bool CanDeleteBook();
        void DeleteBook();
        void ReLoad();
        void ValidateRemoveFile(IEnumerable<Page> pages);

        bool CanBookmark();
        void SetBookmark(bool isBookmark);
        void ToggleBookmark();
    }
}
