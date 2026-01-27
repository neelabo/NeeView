using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 現在ブック情報
    /// </summary>
    [NotifyPropertyChanged]
    public partial class BookContext : INotifyPropertyChanged, IDisposable, IBookPageContext, IBookPageAccessor
    {
        private readonly Book _book;
        private readonly BookPageAccessor _accessor;
        private PageRange _selectedRange;
        private List<Page> _selectedPages = new();
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private ReferenceCounter _isSortBusyCounter = new();

        public BookContext(Book book)
        {
            _book = book;
            _disposables.Add(_book.SubscribePagesChanged(Book_PagesChanged));

            _disposables.Add(_book.Pages.SubscribePropertyChanged(nameof(BookPageCollection.SortMode),
                (s, e) => RaisePropertyChanged(nameof(SortMode))));

            _disposables.Add(_book.Pages.SubscribePagesSorting((s, e) => _isSortBusyCounter.Increment()));
            _disposables.Add(_book.Pages.SubscribePagesSorted((s, e) => _isSortBusyCounter.Decrement()));
            _isSortBusyCounter.Changed += IsSortBusyCounter_Changed;

            _accessor = new BookPageAccessor(_book.Pages);
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler<IsSortBusyChangedEventArgs>? IsSortBusyChanged;

        public Book Book => _book;

        public bool IsEnabled => _book.Pages.Any();

        public bool IsMedia => _book.IsMedia;

        public IReadOnlyList<Page> Pages => _book.Pages;

        public PageSortMode SortMode => _book.Pages.SortMode;


        public PageRange SelectedRange
        {
            get { return _selectedRange; }
            set
            {
                var pages = GetPages(value);
                if (_selectedRange != value || !pages.SequenceEqual(_selectedPages))
                {
                    var oldRange = _selectedRange;
                    _selectedRange = value;
                    _selectedPages = pages;
                    _book.SetCurrentPages(pages);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedPages));
                    SelectedRangeChanged?.Invoke(this, new PageRangeChangedEventArgs(_selectedRange, oldRange));
                }
            }
        }

        public IReadOnlyList<Page> SelectedPages => _selectedPages;


        // NOTE: これは Book で保持する必要ある？ -> Page生成時に必要なのだ...
        public BookMemoryService BookMemoryService => _book.BookMemoryService;

        public int FirstIndex => _accessor.FirstIndex;

        public int LastIndex => _accessor.LastIndex;

        public PagePosition FirstPosition => _accessor.FirstPosition;

        public PagePosition LastPosition => _accessor.LastPosition;

        public PageRange PageRange => _accessor.PageRange;

        public Page? First => _accessor.First;

        public Page? Last => _accessor.Last;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    // 検索状態終了を通知
                    IsSortBusyChanged?.Invoke(this, new IsSortBusyChangedEventArgs(false));
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private void IsSortBusyCounter_Changed(object? sender, ReferenceCounterChangedEventArgs e)
        {
            IsSortBusyChanged?.Invoke(this, new IsSortBusyChangedEventArgs(e.IsActive));
        }

        private void Book_PagesChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Pages));
            PagesChanged?.Invoke(this, e);
        }

        public bool ContainsIndex(int index)
        {
            return _accessor.ContainsIndex(index);
        }

        public int ClampIndex(int index)
        {
            return _accessor.ClampIndex(index);
        }

        public int NormalizeIndex(int index)
        {
            return _accessor.NormalizeIndex(index);
        }

        public PagePosition ClampPosition(PagePosition position)
        {
            return _accessor.ClampPosition(position);
        }

        public PagePosition NormalizePosition(PagePosition position)
        {
            return _accessor.NormalizePosition(position);
        }

        public PagePosition ValidatePosition(PagePosition position, bool normalized = false)
        {
            return _accessor.ValidatePosition(position, normalized);
        }

        public Page? GetPage(int index, bool normalized = false)
        {
            return _accessor.GetPage(index, normalized);
        }

        private List<Page> GetPages(PageRange range)
        {
            return range.CollectPositions().Select(e => Pages[_accessor.NormalizeIndex(e.Index)]).Distinct().ToList();
        }
    }
}
