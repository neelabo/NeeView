using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;
using NeeView.Collections.ObjectModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    public partial class HistoryList : BindableBase
    {
        static HistoryList() => Current = new HistoryList();
        public static HistoryList Current { get; }

        private readonly BookHub _bookHub;
        private string? _filterPath;
        private readonly CollectionViewSource _collectionViewSource = new();
        private BookHistory? _selectedItem;
        private string _searchKeyword = "";
        private readonly Searcher _searcher;
        private SearcherFilter _searcherFilter;


        private HistoryList()
        {
            var searchContext = new SearchContext()
                .AddProfile(new DateSearchProfile())
                .AddProfile(new SizeSearchProfile())
                .AddProfile(new BookSearchProfile());
            _searcher = new Searcher(searchContext);
            _searcherFilter = _searcher.CreateFilter("");

            _bookHub = BookHub.Current;

            BookOperation.Current.BookChanged += BookOperation_BookChanged;

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.IsCurrentFolder),
                (s, e) => UpdateFilterPath());

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.IsGroupBy),
                (s, e) => UpdateGroupBy());

            BookHub.Current.HistoryListSync += BookHub_HistoryListSync;

            this.SearchBoxModel = new SearchBoxModel(new HistorySearchBoxComponent(this));

            // 内部履歴の並びを反転する。SortDescriptions での並び替えより軽い。
            _collectionViewSource.Source = new ReverseObservableCollection<BookHistory>(BookHistoryCollection.Current.Items);
            _collectionViewSource.Culture = TextResources.Culture;
            _collectionViewSource.Filter += CollectionViewSource_Filter;
            _collectionViewSource.LiveGroupingProperties.Add(nameof(BookHistory.LastAccessTime));

            UpdateFilterPath();
            UpdateGroupBy();

            _collectionViewSource.View.Refresh();

            _collectionViewSource.View.CollectionChanged
                += (s, e) => RaisePropertyChanged(nameof(ViewItemsCount));
        }


        // 検索ボックスにフォーカスを
        [Subscribable]
        public event EventHandler? SearchBoxFocus;


        public SearchBoxModel SearchBoxModel { get; }

        public bool IsGroupBy => Config.Current.History.IsGroupBy;

        public bool IsThumbnailVisible
        {
            get
            {
                return Config.Current.History.PanelListItemStyle switch
                {
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0,
                    _ => false,
                };
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.History.PanelListItemStyle;
            set => Config.Current.History.PanelListItemStyle = value;
        }

        /// <summary>
        /// パスフィルター
        /// </summary>
        public string? FilterPath
        {
            get { return _filterPath; }
            set
            {
                if (SetProperty(ref _filterPath, value))
                {
                    UpdateFilter();
                }
            }
        }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    UpdateFilter();
                }
            }
        }

        /// <summary>
        /// 履歴リスト (ViewSource)
        /// </summary>
        public CollectionViewSource CollectionViewSource => _collectionViewSource;

        /// <summary>
        /// 選択項目
        /// </summary>
        public BookHistory? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// 表示項目数
        /// </summary>
        public int ViewItemsCount => _collectionViewSource.View is CollectionView collectionView ? collectionView.Count : -1;


        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is BookHistory item)
            {
                e.Accepted = Filter(item);
            }
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            UpdateFilterPath();
        }

        /// <summary>
        /// 履歴同期イベント
        /// </summary>
        private void BookHub_HistoryListSync(object? sender, BookPathEventArgs e)
        {
            if (e.Path is null) return;

            AppDispatcher.BeginInvoke(() => SelectedItem = _collectionViewSource.View.Cast<BookHistory>().FirstOrDefault(x => x.Path == e.Path));
        }

        /// <summary>
        /// グループ更新
        /// </summary>
        private void UpdateGroupBy()
        {
            if (IsGroupBy)
            {
                _collectionViewSource.GroupDescriptions.Clear();
                _collectionViewSource.GroupDescriptions.Add(new HistoryListGroupDescription());
                _collectionViewSource.IsLiveGroupingRequested = true;
            }
            else
            {
                _collectionViewSource.GroupDescriptions.Clear();
                _collectionViewSource.IsLiveGroupingRequested = false;
            }

            RaisePropertyChanged(nameof(IsGroupBy));
        }

        /// <summary>
        /// パスフィルター更新
        /// </summary>
        private void UpdateFilterPath()
        {
            FilterPath = Config.Current.History.IsCurrentFolder ? LoosePath.GetDirectoryName(BookOperation.Current.Address) : "";
        }

        /// <summary>
        /// フィルター更新
        /// </summary>
        private void UpdateFilter()
        {
            try
            {
                _searcherFilter = _searcher.CreateFilter(_searchKeyword);
                _collectionViewSource.View.Refresh();
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, "", ToastIcon.Error));
            }
        }

        private bool Filter(BookHistory item)
        {
            if (!string.IsNullOrEmpty(FilterPath))
            {
                if (FilterPath != LoosePath.GetDirectoryName(item.Path)) return false;
            }

            return _searcherFilter.Filter(item);
        }

        /// <summary>
        /// 最新の履歴リスト取得
        /// </summary>
        private List<BookHistory> GetViewItems()
        {
            return _collectionViewSource.View.Cast<BookHistory>().ToList();
        }

        /// <summary>
        /// 履歴を戻ることができる？
        /// </summary>
        public bool CanPrevHistory()
        {
            var items = GetViewItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);

            if (index < 0)
            {
                return items.Any();
            }
            else
            {
                return index < items.Count - 1;
            }
        }

        /// <summary>
        /// 履歴を戻る
        /// </summary>
        public void PrevHistory()
        {
            var items = GetViewItems();

            if (_bookHub.IsLoading || items.Count <= 0) return;

            var index = items.FindIndex(e => e.Path == _bookHub.Address);

            var prev = index < 0
                ? items.First()
                : index < items.Count - 1 ? items[index + 1] : null;

            if (prev != null)
            {
                _bookHub.RequestLoad(this, prev.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.HistoryTerminal"));
            }
        }

        /// <summary>
        /// 履歴を進めることができる？
        /// </summary>
        public bool CanNextHistory()
        {
            var items = GetViewItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);
            return index > 0;
        }

        /// <summary>
        /// 履歴を進める
        /// </summary>
        public void NextHistory()
        {
            var items = GetViewItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);
            if (index > 0)
            {
                var next = items[index - 1];
                _bookHub.RequestLoad(this, next.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.HistoryLatest"));
            }
        }

        /// <summary>
        /// 履歴削除
        /// </summary>
        /// <param name="items">削除項目</param>
        public void Remove(IEnumerable<BookHistory> items)
        {
            if (items == null) return;

            BookHistoryCollection.Current.Remove(items.Select(e => e.Path));
        }

        public SearchKeywordAnalyzeResult SearchKeywordAnalyze(string keyword)
        {
            try
            {
                return new SearchKeywordAnalyzeResult(_searcher.Analyze(keyword));
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
        public class HistorySearchBoxComponent : ISearchBoxComponent
        {
            private readonly HistoryList _self;

            public HistorySearchBoxComponent(HistoryList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.BookHistorySearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.SearchKeyword = keyword;
        }
    }


    /// <summary>
    /// 日付でグループ化
    /// </summary>
    public class HistoryListGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is not BookHistory data) return ResourceService.GetString("@Word.ItemNone");

            var today = DateTime.Today;
            if (data.LastAccessTime.Date == today)
            {
                return ResourceService.GetString("@Word.Today");
            }
            else if (data.LastAccessTime.Date == today.AddDays(-1))
            {
                return ResourceService.GetString("@Word.Yesterday");
            }
            else
            {
                return data.LastAccessTime.Date.ToString("D", culture);
            }
        }
    }

}
