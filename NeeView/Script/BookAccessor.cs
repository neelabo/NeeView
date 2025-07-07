using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

#pragma warning disable CA1822

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    [WordNodeMember]
    public class BookAccessor
    {
        private CancellationToken _cancellationToken;
        private readonly IAccessDiagnostics _accessDiagnostics;

        public BookAccessor(IAccessDiagnostics accessDiagnostics)
        {
            _accessDiagnostics = accessDiagnostics ?? throw new ArgumentNullException(nameof(accessDiagnostics));
        }

        [WordNodeMember]
        public string? Path => BookOperation.Current.Book?.Path;

        [WordNodeMember]
        public long Size => GetArchive()?.Length ?? 0;

        [WordNodeMember]
        public DateTime LastWriteTime => GetArchive()?.LastWriteTime ?? DateTime.MinValue;

        [WordNodeMember]
        public DateTime CreationTime => GetArchive()?.CreationTime ?? DateTime.MinValue;

        [WordNodeMember]
        public bool IsMedia => BookOperation.Current.Book?.IsMedia == true;

        [WordNodeMember]
        public bool IsNew => BookOperation.Current.Book?.IsNew == true;

        [WordNodeMember]
        public bool IsBookmarked
        {
            get => BookOperation.Current.BookControl.IsBookmark == true;
            set => AppDispatcher.Invoke(() => BookOperation.Current.BookControl.SetBookmark(value));
        }

        [WordNodeMember]
        public BookConfigAccessor Config { get; } = new BookConfigAccessor();

        [WordNodeMember]
        public PageAccessor[] Pages
        {
            get
            {
                return BookOperation.Current.Book?.Pages.Select(e => new PageAccessor(e)).ToArray() ?? Array.Empty<PageAccessor>();
            }
        }

        [WordNodeMember]
        public ViewPageAccessor[] ViewPages
        {
            get
            {
                return BookOperation.Current.ViewPages.Select(e => new ViewPageAccessor(e)).ToArray();
            }
        }


        [WordNodeMember]
        public void Wait()
        {
            BookOperation.Current.WaitAsync(_cancellationToken).AsTask().Wait();
        }


        internal void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        private Archive? GetArchive()
        {
            return BookOperation.Current.Book?.Source.ArchiveEntryCollection.Archive;
        }


        #region Obsolete

        [WordNodeMember]
        [Obsolete("no used"), Alternative("ViewPages.length", 38)] // ver.38
        public int PageSize
        {
            get
            {
                return _accessDiagnostics.Throw<int>(new NotSupportedException(RefrectionTools.CreatePropertyObsoleteMessage(this.GetType())));
            }
        }

        [WordNodeMember]
        [Obsolete("no used"), Alternative("Pages.length", 38)] // ver.38
        public int ViewPageSize
        {
            get
            {
                return _accessDiagnostics.Throw<int>(new NotSupportedException(RefrectionTools.CreatePropertyObsoleteMessage(this.GetType())));
            }
        }

        [WordNodeMember]
        [Obsolete("no used"), Alternative("Pages[]", 38)] // ver.38
        public PageAccessor? Page(int index)
        {
            return _accessDiagnostics.Throw<PageAccessor>(new NotSupportedException(RefrectionTools.CreateMethodObsoleteMessage(this.GetType())));
        }

        [WordNodeMember]
        [Obsolete("no used"), Alternative("ViewPages[]", 38)] // ver.38
        public PageAccessor? ViewPage(int index)
        {
            return _accessDiagnostics.Throw<PageAccessor>(new NotSupportedException(RefrectionTools.CreateMethodObsoleteMessage(this.GetType())));
        }

        #endregion Obsoletet
    }
}
