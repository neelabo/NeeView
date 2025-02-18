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

        bool CanDeleteBook();
        void DeleteBook();
        void ReLoad();

        bool CanBookmark();
        void SetBookmark(bool isBookmark);
        void ToggleBookmark();
    }
}
