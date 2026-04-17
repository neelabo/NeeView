//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.Threading;
using NeeView.Windows.Media;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxのページサムネイルを読み込む機能
    /// </summary>
    [LocalDebug]
    public partial class ListBoxThumbnailLoader
    {
        private readonly IPageListPanel _panel;
        private readonly PageThumbnailJobClient _jobClient;
        private readonly ConstDelayAction _delayAction = new(200);
        private List<Page> _pages = [];

        public ListBoxThumbnailLoader(IPageListPanel panelListBox, PageThumbnailJobClient jobClient)
        {
            _panel = panelListBox;
            _jobClient = jobClient;

            _panel.PageCollectionListBox.Loaded += ListBox_Loaded; ;
            _panel.PageCollectionListBox.IsVisibleChanged += ListBox_IsVisibleChanged;
            _panel.PageCollectionListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
            ((INotifyCollectionChanged)_panel.PageCollectionListBox.Items).CollectionChanged += ListBox_CollectionChanged;
        }

        private void ListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Load();
            }
            else
            {
                Unload();
            }
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Load();
        }

        private void ListBox_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // コレクションが変更されてもスクロールビュー位置が変更されないことがある問題の対処
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    _panel.PageCollectionListBox.UpdateLayout();
                    Load();
                    break;
            }
        }

        public void Load()
        {
            if (!_panel.IsThumbnailVisible)
            {
                return;
            }

            if (!_panel.PageCollectionListBox.IsVisible)
            {
                return;
            }

            var listBoxItems = VisualTreeUtility.FindVisualChildren<ListBoxItem>(_panel.PageCollectionListBox);
            if (listBoxItems == null || listBoxItems.Count <= 0)
            {
                return;
            }

            var items = _panel
                .CollectPageList(listBoxItems.Select(i => i.DataContext))
                .Select(e => e.GetPage())
                .WhereNotNull();

            LoadCore(items);
        }

        private void LoadCore(IEnumerable<Page> items)
        {
            var pages = items.ToList();
            if (pages.Count == 0)
            {
                return;
            }

            if (pages.SequenceEqual(_pages))
            {
                LocalDebug.WriteLine($"{System.Environment.TickCount}: skip");
                return;
            }

            LocalDebug.WriteLine($"{System.Environment.TickCount}: ready..");

            _delayAction.Request(() =>
            {
                LocalDebug.WriteLine($"{System.Environment.TickCount}: {pages.MinBy(e => e.Index)?.Index}-{pages.MaxBy(e => e.Index)?.Index} ({pages.Count})");
                _pages = pages;
                _jobClient?.Order(_pages.Cast<IPageThumbnailLoader>().ToList());
            });
        }

        public void Unload()
        {
            _pages = [];
            _delayAction.Cancel();
            _jobClient?.CancelOrder();
        }
    }
}
