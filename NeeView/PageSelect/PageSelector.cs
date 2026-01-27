using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// スライダーやフィルムストリップと連動したページ選択の提供
    /// </summary>
    public partial class PageSelector : BindableBase
    {
        static PageSelector() => Current = new PageSelector();
        public static PageSelector Current { get; }


        private int _selectedIndex;


        private PageSelector()
        {
            BookOperation.Current.BookChanging += BookOperation_BookChanging;
            BookOperation.Current.BookChanged += BookOperation_BookChanged;
            BookOperation.Current.Control.PagesChanged += BookOperation_PageListChanged;
            //BookOperation.Current.Property.ViewContentsChanged += BookOperation_ViewContentsChanged;
            BookOperation.Current.Control.SelectedRangeChanged += BookOperation_SelectedRangeChanged;
        }



        // NOTE: ChangingとChangedは必ずしもペアではない
        [Subscribable]
        public event EventHandler? CollectionChanging;

        [Subscribable]
        public event EventHandler? CollectionChanged;

        [Subscribable]
        public event EventHandler? SelectionChanged;

        [Subscribable]
        public event EventHandler<SelectedPagesChangedEventArgs>? SelectedPagesChanged;



        public PageMode PageMode => BookOperation.Current.Book?.Setting.PageMode ?? PageMode.SinglePage;

        public bool IsSupportedSingleFirstPage => BookOperation.Current.Book?.Setting.IsSupportedSingleFirstPage ?? false;

        public bool IsSupportedSingleLastPage => BookOperation.Current.Book?.Setting.IsSupportedSingleLastPage ?? false;

        public int ViewPageCount => BookOperation.Current.Book?.CurrentPages.Count ?? 0;

        public int MaxIndex => Math.Max(BookOperation.Current.Control.Pages.Count - 1, 0);

        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        public Page? SelectedItem
        {
            get
            {
                var book = BookOperation.Current.Book;
                if (book is null || _selectedIndex < 0 || book.Pages.Count <= _selectedIndex) return null;
                return book.Pages[_selectedIndex];
            }
        }


        internal void FlushSelectedIndex(object sender)
        {
            SetSelectedIndex(sender, BookOperation.Current.Control.SelectedRange.Min.Index, true);
        }

        public bool SetSelectedIndex(object? sender, int value, bool raiseChangedEvent)
        {
            if (SetProperty(ref _selectedIndex, value, nameof(SelectedIndex)))
            {
                ////Debug.WriteLine($"> PageSelector.SelectedIndex={_selectedIndex}");

                if (raiseChangedEvent)
                {
                    SelectionChanged?.Invoke(sender, EventArgs.Empty);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Jump(object sender)
        {
            ////Debug.WriteLine($"Jump: {_selectedIndex}");
            BookOperation.Current.Control.MoveTo(sender, _selectedIndex);
        }

        private void BookOperation_BookChanging(object? sender, BookChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, EventArgs.Empty);
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            UpdateCollection(sender);
        }

        private void BookOperation_PageListChanged(object? sender, EventArgs e)
        {
            if (BookOperation.Current.IsLoading) return;
            UpdateCollection(sender);
        }

        private void UpdateCollection(object? sender)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(MaxIndex));
            //RaiseViewContentsChanged(sender, BookOperation.Current.Book?.Viewer.ViewPageCollection, true);
            RaiseViewContentsChanged(sender, BookOperation.Current.Control.SelectedRange, true);
        }

        //private void BookOperation_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        //{
        //    RaiseViewContentsChanged(sender, e?.ViewPageCollection, false);
        //}

        private void BookOperation_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            RaiseViewContentsChanged(sender, BookOperation.Current.Control.SelectedRange, false);
        }

        private void RaiseViewContentsChanged(object? sender, PageRange range, bool isBookOpen)
        {
            var book = BookOperation.Current.Book;
            if (book is null) return;
            if (!book.Pages.Any()) return;

            var pages = CollectPage(book, range);
            if (pages is null) return;

            SelectedPagesChanged?.Invoke(sender, new SelectedPagesChangedEventArgs(pages));

            var index = MathUtility.NormalizeLoopRange(range.Min.Index, 0, book.Pages.Count - 1);
            SetSelectedIndex(sender, index, false);
            SelectionChanged?.Invoke(sender, EventArgs.Empty);
        }

        // PageRange に含まれる Page を収集
        private List<Page>? CollectPage(Book? book, PageRange range)
        {
            if (book is null) return null;
            if (!book.Pages.Any()) return null;

            var indexes = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1)
                .Select(e => MathUtility.NormalizeLoopRange(e, 0, book.Pages.Count - 1));
            return indexes.Where(e => book.Pages.IsValidIndex(e)).Select(e => book.Pages[e]).ToList();
        }

    }

}

