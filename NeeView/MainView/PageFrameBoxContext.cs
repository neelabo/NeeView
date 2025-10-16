//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// PageFrameBox コンテキスト
    /// </summary>
    /// <remarks>
    /// この単位で切り替える
    /// </remarks>
    [LocalDebug]
    public partial class PageFrameBoxContext : IDisposable, IDragTransformContextFactory
    {
        private readonly Book _book;
        private readonly PageFrameContext _context;
        private readonly BookContext _bookContext;
        private readonly PageFrameBox _box;
        private readonly BookMementoControl _bookMementoControl;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public PageFrameBoxContext(BookShareContext shareContext, Book book)
        {
            _book = book;

            _context = new PageFrameContext(shareContext, book.IsMedia);
            _disposables.Add(_context);

            _bookContext = new BookContext(book);
            _disposables.Add(_bookContext);
            _disposables.Add(_bookContext.SubscribeIsSortBusyChanged((s, e) => IsSortBusyChanged?.Invoke(s, e)));

            _box = new PageFrameBox(_context, _bookContext);
            _disposables.Add(_box);
            _disposables.Add(_box.SubscribePagesChanged((s, e) => PagesChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeSelectedRangeChanged((s, e) => SelectedRangeChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeViewContentChanged((s, e) => ViewContentChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeTransformChanged((s, e) => TransformChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeStretchChanged((s, e) => StretchChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeSelectedContainerLayoutChanged((s, e) => SelectedContainerLayoutChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeSelectedContentSizeChanged((s, e) => SelectedContentSizeChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribeSizeChanged((s, e) => ViewSizeChanged?.Invoke(s, e)));
            _disposables.Add(_box.SubscribePageTerminated((s, e) => PageTerminated?.Invoke(s, e)));

            _bookMementoControl = new BookMementoControl(book, BookHistoryCollection.Current);
            _disposables.Add(_bookMementoControl);

            _disposables.Add(Config.Current.BookSetting.SubscribePropertyChanged((s, e) => _bookMementoControl.RequestSaveBookMemento(false)));

            _disposables.Add(_box.SubscribePageTerminated((s, e) =>
            {
                LocalDebug.WriteLine($"Event.PageTerminated: {e.IsMedia}, {e.Direction}");
                var checkHistoryRemove = e.IsMedia;
                _bookMementoControl.RequestSaveBookMemento(checkHistoryRemove);
            }));

            _disposables.Add(_box.SubscribePageChangeAction((s, e) =>
            {
                Debug.Assert(e.IsMedia, "The current version only supports media.");
                LocalDebug.WriteLine($"Event.PageAction: {e.IsMedia}, {e.Action}");
                var checkHistoryRemove = e.IsMedia && (e.Action == PageChangeAction.Reward || e.Action == PageChangeAction.PlayTimeElapsed);
                _bookMementoControl.RequestSaveBookMemento(checkHistoryRemove);
            }));
        }

        [Subscribable]
        public event EventHandler<IsSortBusyChangedEventArgs>? IsSortBusyChanged;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;

        [Subscribable]
        public event EventHandler? StretchChanged;

        [Subscribable]
        public event EventHandler? SelectedContentSizeChanged;

        [Subscribable]
        public event EventHandler? SelectedContainerLayoutChanged;

        [Subscribable]
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event SizeChangedEventHandler? ViewSizeChanged;

        [Subscribable]
        public event EventHandler<PageTerminatedEventArgs>? PageTerminated;


        public Book Book => _book;

        public PageFrameBox Box => _box;

        public IReadOnlyList<Page> Pages => _box.Pages;

        public PageRange SelectedRange => _box.SelectedRange;

        public BookMementoControl BookMementoControl => _bookMementoControl;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Debug.Assert(!_book.IsDisposed);
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(bool isPointContainer)
        {
            return _box.CreateContentDragTransformContext(isPointContainer);
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(PageFrameContainer container)
        {
            return _box.CreateContentDragTransformContext(container);
        }

        public LoupeDragTransformContext? CreateLoupeDragTransformContext()
        {
            return _box.CreateLoupeDragTransformContext();
        }
    }

}
