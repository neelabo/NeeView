using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Setting;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using NeeLaboratory.IO.Search;
using System.Threading;
using NeeView.Properties;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定画面 Model
    /// </summary>
    public class SettingWindowModel : BindableBase
    {
        private class SettingItemRecord : ISearchItem
        {
            public SettingItemRecord(SettingPage page, SettingItemSection section, SettingItem item)
            {
                Debug.Assert(page != null && section != null && item != null);
                Page = page;
                Section = section;
                Item = item;
            }

            public SettingPage Page { get; }

            public SettingItemSection Section { get; }

            public SettingItem Item { get; }


            public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
            {
                switch (profile.Name)
                {
                    case "text":
                        return new StringSearchValue(GetSearchText());
                    default:
                        throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
                }
            }

            public string GetSearchText()
            {
                return Page.GetSearchText() + " " + Section.GetSearchText() + " " + Item.GetSearchText();
            }
        }


        private static Type? _latestSelectedPageType;

        private readonly List<SettingPage> _pages;
        private readonly List<SettingItemRecord> _records;
        private string _searchKeyword = "";
        private SettingPage? _currentPage;
        private SettingPage? _lastPage;
        private readonly Searcher _searcher;

        public SettingWindowModel()
        {
            _pages = new List<SettingPage>
            {
                new SettingPageGeneral(),
                new SettingPageFileTypes(),
                new SettingPageBook(),
                new SettingPageWindow(),
                new SettingPageMainView(),
                new SettingPagePanels(),
                new SettingPageSlider(),
                new SettingPageCommand()
            };

            _latestSelectedPageType = _latestSelectedPageType ?? typeof(SettingPageGeneral);
            var page = GetSettingPagesEnumerator(_pages).FirstOrDefault(e => e.GetType() == _latestSelectedPageType);
            if (page != null)
            {
                page.IsSelected = true;
            }

            _records = CreateSettingItemRecordList(_pages);

            _searcher = new Searcher(new SearchContext());

            this.SearchBoxModel = new SearchBoxModel(new SettingWindowSearchBoxComponent(this));
        }


        public SearchBoxModel SearchBoxModel { get; }

        public bool IsSearchPageSelected
        {
            get { return _currentPage == SearchPage; }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    if (!string.IsNullOrWhiteSpace(_searchKeyword))
                    {
                        ClearPageContentCache();
                        UpdateSearchPage(_searchKeyword);
                        CurrentPage = SearchPage;
                    }
                    else
                    {
                        if (IsSearchPageSelected && _lastPage != null)
                        {
                            _lastPage.IsSelected = true;
                        }
                    }
                }
            }
        }

        public SettingPage? CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    SetSelectedPage(_currentPage);
                }
            }
        }

        public List<SettingPage> Pages
        {
            get { return _pages; }
        }

        public SettingPage SearchPage { get; } = new SettingPage(TextResources.GetString("SettingPage.SearchResult"));



        public void SelectedItemChanged(SettingPage settingPage)
        {
            if (settingPage != null)
            {
                CurrentPage = settingPage;
                _lastPage = settingPage;
                SearchKeyword = "";
            }
        }

        private List<SettingItemRecord> CreateSettingItemRecordList(IEnumerable<SettingPage> pages)
        {
            var list = new List<SettingItemRecord>();

            foreach (var page in GetSettingPagesEnumerator(pages))
            {
                if (page.Items != null)
                {
                    foreach (var item in page.Items)
                    {
                        var section = item as SettingItemSection;
                        Debug.Assert(section != null);

                        foreach (var child in section.Children)
                        {
                            list.Add(new SettingItemRecord(page, section, child));
                        }
                    }
                }
            }

            return list;
        }


        public void SetSelectedPage(SettingPage? page)
        {
            if (page == null) return;
            _latestSelectedPageType = page != SearchPage ? page.GetType() : null;
        }

        private IEnumerable<SettingPage> GetSettingPagesEnumerator()
        {
            return GetSettingPagesEnumerator(_pages);
        }

        private IEnumerable<SettingPage> GetSettingPagesEnumerator(IEnumerable<SettingPage> pages)
        {
            if (pages == null) yield break;

            foreach (var page in pages)
            {
                yield return page;
                if (page.Children != null)
                {
                    foreach (var child in GetSettingPagesEnumerator(page.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

        public void ClearPageContentCache()
        {
            SearchPage.ClearContentCache();

            foreach (var page in GetSettingPagesEnumerator())
            {
                page.ClearContentCache();
            }
        }

        public void UpdateSearchPage(string keyword)
        {
            List<SettingItemRecord> records;
            try
            {
                records = _searcher.Search(keyword, _records, CancellationToken.None).Cast<SettingItemRecord>().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            var items = new List<SettingItem>();
            if (records.Any())
            {
                items.Add(new SettingItemSection(TextResources.GetString("SettingPage.SearchResult")));
                foreach (var group in records.GroupBy(e => e.Section))
                {
                    var section = new SettingItemSection(group.Key.Header, group.Key.Tips);
                    section.Children.AddRange(group.Select(e => e.Item.SearchResultItem));
                    items.Add(section);
                }
            }
            else
            {
                items.Add(new SettingItemSection(TextResources.GetString("SettingPage.SearchResult.NotFound")));
            }

            SearchPage.SetItems(items);
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
        /// 検索ボックスコンポーネント
        /// </summary>
        public class SettingWindowSearchBoxComponent : ISearchBoxComponent
        {
            private readonly SettingWindowModel _self;

            public SettingWindowSearchBoxComponent(SettingWindowModel self)
            {
                _self = self;
            }

            public HistoryStringCollection? History { get; } = new HistoryStringCollection();

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.SearchKeyword = keyword;
        }
    }
}
