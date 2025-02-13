using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : BindableBase
    {
        static PageList() => Current = new PageList();
        public static PageList Current { get; }


        private PageSortMode _pageSortMode;
        private PageSortModeClass _pageSortModeClass = PageSortModeClass.Full;
        private Page? _selectedItem;
        private List<Page>? _selectedItems;
        private List<Page> _viewItems = new();
        private ObservableCollection<Page>? _items;
        private bool _isPageMoved;
        private Book? _book;

        private PageList()
        {
            BookOperation.Current.BookChanged += BookOperation_BookChanged;
            BookOperation.Current.Control.PagesChanged += BookOperation_PageListChanged;

            PageHistory.Current.Changed += (s, e) => PageHistoryChanged?.Invoke(s, e);

            this.SearchBoxModel = new SearchBoxModel(new PageListSearchBoxComponent(this));
        }


        /// <summary>
        /// ページコレクションの変更通知
        /// </summary>
        public event EventHandler? CollectionChanging;
        public event EventHandler? CollectionChanged;

        public event EventHandler? PageHistoryChanged;

        /// <summary>
        ///  表示ページの変更通知
        /// </summary>
        public event EventHandler<ViewItemsChangedEventArgs>? ViewItemsChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler? SearchBoxFocus;


        public SearchBoxModel SearchBoxModel { get; }

        // サムネイル画像が表示される？？
        public bool IsThumbnailVisible
        {
            get
            {
                return Config.Current.PageList.PanelListItemStyle switch
                {
                    PanelListItemStyle.Thumbnail => true,
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0,
                    _ => false,
                };
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.PageList.PanelListItemStyle;
            set => Config.Current.PageList.PanelListItemStyle = value;
        }

        /// <summary>
        /// サイドパネルでの場所表示用
        /// </summary>
        public string PlaceDisplayString
        {
            get { return LoosePath.GetFileName(BookOperation.Current.Address); }
        }

        /// <summary>
        /// ブックのパス
        /// </summary>
        public string? Path
        {
            get { return BookOperation.Current.Address; }
        }

        /// <summary>
        /// 並び順
        /// </summary>
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; BookSettings.Current.SetSortMode(value); }
        }

        public Dictionary<PageSortMode, string> PageSortModeList
        {
            get { return _pageSortModeClass.GetPageSortModeMap(); }
        }

        public PageSortModeClass PageSortModeClass
        {
            get { return _pageSortModeClass; }
            set
            {
                if (SetProperty(ref _pageSortModeClass, value))
                {
                    RaisePropertyChanged(nameof(PageSortModeList));
                }
            }
        }



        // ページリスト(表示部用)
        public ObservableCollection<Page>? Items
        {
            get { return _items; }
            private set
            {
                if (SetProperty(ref _items, value))
                {
                    RaisePropertyChanged(nameof(ValidCount));
                    ValidateViewItems();
                }
            }
        }

        public Page? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public List<Page>? SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }

        public List<Page> ViewItems
        {
            get { return _viewItems; }
            set
            {
                if (_viewItems.SequenceEqual(value)) return;

                var removes = _viewItems.Where(e => !value.Contains(e));
                var direction = removes.Any() && value.Any() ? removes.First().Index < value.First().Index ? +1 : -1 : 0;

                _viewItems = value;

                ViewItemsChanged?.Invoke(this, new ViewItemsChangedEventArgs(_viewItems, direction));
            }
        }

        /// <summary>
        /// 選択項目
        /// </summary>
        public int ValidCount => IsEmpty() ? 0 : _items.Count;


        [MemberNotNullWhen(false, nameof(_items))]
        public bool IsEmpty()
        {
            return _items == null
                || _items.Count == 0
                || _items.Count == 1 && _items[0].Content is EmptyPageContent;
        }

        private void ValidateViewItems()
        {
            var viewItems = _viewItems.Where(e => _items?.Contains(e) ?? false).ToList();
            ViewItems = viewItems;
        }

        public void Loaded()
        {
            BookOperation.Current.Control.SelectedRangeChanged += BookOperation_SelectedRangeChanged;
            RefreshSelectedItem();
        }


        public void Unloaded()
        {
            BookOperation.Current.Control.SelectedRangeChanged -= BookOperation_SelectedRangeChanged;
        }

        /// <summary>
        /// ブックが変更された時の処理
        /// </summary>
        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            RefreshCollection();
            SearchBoxModel.Keyword = _book?.Pages.SearchKeyword;
        }

        private void BookOperation_PageListChanged(object? sender, EventArgs e)
        {
            if (BookOperation.Current.IsLoading) return;

            RefreshCollection();
        }

        /// <summary>
        /// ブックのページが切り替わったときの処理
        /// </summary>
        //private void BookOperation_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        //{
        //    RefreshSelectedItem();
        //}

        private void BookOperation_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            RefreshSelectedItem();
        }


        /// <summary>
        /// ページコレクション更新
        /// </summary>
        private void RefreshCollection()
        {
            CollectionChanging?.Invoke(this, EventArgs.Empty);

            _book = BookOperation.Current.Book;

            var pages = BookOperation.Current.Control.Pages;
            Items = new ObservableCollection<Page>(pages);

            RaisePropertyChanged(nameof(PlaceDisplayString));

            PageSortModeClass = _book != null ? _book.PageSortModeClass : PageSortModeClass.Full;
            PageSortMode = PageSortModeClass.ValidatePageSortMode(Config.Current.BookSetting.SortMode);
            RaisePropertyChanged(nameof(PageSortMode));

            RefreshSelectedItem();

            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 表示マークと選択項目をブックにあわせる
        /// </summary>
        private void RefreshSelectedItem()
        {
            var book = BookOperation.Current.Book;
            if (book is null) return;

            var range = BookOperation.Current.Control.SelectedRange;
            var pages = CollectPage(book, range);
            if (pages == null) return;

            var viewPages = pages.Where(i => i != null).ToList();

            var page = viewPages.FirstOrDefault();
            if (page is null) return;

            if (SelectedItems == null || SelectedItems.Count <= 1 || !SelectedItems.Contains(page))
            {
                this.SelectedItem = page;
            }

            this.ViewItems = viewPages;
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


        /// <summary>
        /// ページ移動フラグクリア
        /// </summary>
        public void ResetMoveFlag()
        {
            _isPageMoved = false;
        }

        /// <summary>
        /// ページ移動
        /// </summary>
        /// <param name="page">移動先ページ</param>
        public void MoveTo(Page page)
        {
            // MainViewWindow の最小化を解除
            // TODO: これはもっと先でもよいかも？。 MainViewBayで最小化されている場合をどうするか。 MainViewBay タッチで開く？
            var isRecovery = MainViewManager.Current.RecoveryFloating();

            // ページ移動
            _isPageMoved = true;
            BookOperation.Current.JumpPage(this, page);
            if (isRecovery)
            {
                PageFrameBoxPresenter.Current.FlushLayout();
            }
        }

        /// <summary>
        /// ページ移動終了処理
        /// </summary>
        public void MoveEnd()
        {
            if (!_isPageMoved) return;
            _isPageMoved = false;

            // 必要に応じてメインビューにフォーカスを移動する
            if (!Config.Current.PageList.FocusMainView) return;
            MainViewComponent.Current.RaiseFocusMainViewRequest();
        }

        public List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return PageHistory.Current.GetHistory(direction, size);
        }

        public bool CanMoveToPrevious()
        {
            return PageHistory.Current.CanMoveToPrevious();
        }

        public void MoveToPrevious()
        {
            PageHistory.Current.MoveToPrevious();
        }

        public bool CanMoveToNext()
        {
            return PageHistory.Current.CanMoveToNext();
        }

        public void MoveToNext()
        {
            PageHistory.Current.MoveToNext();
        }

        public void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            PageHistory.Current.MoveToHistory(item);
        }

        public bool CanMoveToParent()
        {
            return BookHub.Current.CanLoadParent();
        }

        public void MoveToParent()
        {
            BookHub.Current.RequestLoadParent(this);
        }

        public void SetSearchKeyword(string keyword)
        {
            if (_book is not null)
            {
                _book.Pages.SearchKeyword = keyword;
            }
        }

        public SearchKeywordAnalyzeResult SearchKeywordAnalyze(string keyword)
        {
            try
            {
                return new SearchKeywordAnalyzeResult(_book?.Pages.SearchKeywordAnalyze(keyword) ?? Array.Empty<SearchKey>());
            }
            catch (Exception ex)
            {
                return new SearchKeywordAnalyzeResult(ex);
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            SearchBoxFocus?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 検索ボックスコンポーネント
        /// </summary>
        public class PageListSearchBoxComponent : ISearchBoxComponent
        {
            private readonly PageList _self;

            public PageListSearchBoxComponent(PageList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.PageListSearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.SetSearchKeyword(keyword);
        }
    }
}
