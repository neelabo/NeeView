using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;


namespace NeeView.PageFrames
{
    public partial class PageFrameContainerVisiblePageWatcher
    {

        private readonly PageFrameContext _context;
        private readonly PageFrameContainerViewBox _viewBox;
        private readonly PageFrameContainerCollectionRectMath _collectionRectMath;
        private readonly PageFrameContainerLayout _layout;
        private List<PageFrameContainer> _visibleContainers = new();
        private PageRange _viewRange;
        private List<Page> _viewPages = new();


        public PageFrameContainerVisiblePageWatcher(PageFrameContext context, PageFrameContainerViewBox viewBox, PageFrameContainerCollectionRectMath collectionRectMath, PageFrameContainerLayout layout)
        {
            _context = context;
            _viewBox = viewBox;
            _collectionRectMath = collectionRectMath;
            _layout = layout;

            // TODO: Dispose
            _layout.LayoutChanged += Layout_LayoutChanged;
        }


        [Subscribable]
        public event EventHandler<VisibleContainersChangedEventArgs>? VisibleContainersChanged;


        public List<PageFrameContainer> VisibleContainers => _visibleContainers;

        public PageRange ViewRange => _viewRange;

        public List<Page> VisiblePages => _viewPages;



        // TODO: 変更通知はPropertyChangedではなく専用イベントで？

        private void Layout_LayoutChanged(object? sender, EventArgs e)
        {
            Update();
        }

        public void Reset()
        {
            SetVisibleContainers(new List<PageFrameContainer>());
        }


        /// <summary>
        /// ViewPages 更新
        /// TODO: 別管理じゃないか？
        /// </summary>
        private void Update()
        {
            // NOTE: 判定誤差を吸収するため判定表示エリアを1dot縮小
            var viewRect = _viewBox.Rect.InflateValid(-1, -1);
            Debug.Assert(!viewRect.IsEmpty);
            SetVisibleContainers(_collectionRectMath.CollectViewContainers(viewRect));
        }

        private void SetVisibleContainers(List<PageFrameContainer> visibleContainers)
        {
            // もし不連続になる場合、画面中心から遠い不連続コンテナを除外する
            if (!IsContinued(visibleContainers))
            {
                visibleContainers = SelectContainerGroup(CollectContainerGroup(visibleContainers), _viewBox.Rect);
                Debug.Assert(IsContinued(visibleContainers));
            }

            var range = GetPageRange(visibleContainers);
            var pages = CollectPages(visibleContainers);
            if (!_visibleContainers.SequenceEqual(visibleContainers) || _viewRange != range || !_viewPages.SequenceEqual(pages))
            {
                var direction = range < _viewRange ? -1 : 1;
                _visibleContainers = visibleContainers;
                _viewRange = range;
                _viewPages = pages;
                VisibleContainersChanged?.Invoke(this, new VisibleContainersChangedEventArgs(_visibleContainers, _viewRange, _viewPages, direction));
            }
        }

        private PageRange GetPageRange(List<PageFrameContainer> containers)
        {
            return PageRange.Marge(containers.Select(e => e.FrameRange));
        }

        private bool IsContinued(List<PageFrameContainer> containers)
        {
            for (int i = 0; i < containers.Count - 1; i++)
            {
                var range0 = containers[i + 0].Content.FrameRange;
                var range1 = containers[i + 1].Content.FrameRange;

                if (range0.Next() != range1.Min)
                {
                    return false;
                }
            }

            return true;
        }

        private List<List<PageFrameContainer>> CollectContainerGroup(List<PageFrameContainer> containers)
        {
            var groups = new List<List<PageFrameContainer>>();
            var group = new List<PageFrameContainer>();
            var range = PageRange.Empty;

            for (int i = 0; i < containers.Count; i++)
            {
                var container = containers[i];

                if (range.IsEmpty() || range.Next() != container.Content.FrameRange.Min)
                {
                    group = new List<PageFrameContainer>();
                    groups.Add(group);
                }

                group.Add(container);
                range = container.Content.FrameRange;
            }

            return groups;
        }

        private List<PageFrameContainer> SelectContainerGroup(List<List<PageFrameContainer>> groups, Rect viewRect)
        {
            Debug.Assert(groups.Any());
            var viewCenter = viewRect.Center();
            var containers = groups
                .Select(e => (group: e, length: e.Aggregate(double.PositiveInfinity, (result, e) => Math.Min((e.Center - viewCenter).LengthSquared, result))))
                .MinBy(e => e.length).group;
            return containers;
        }


        private List<Page> CollectPages(List<PageFrameContainer> containers)
        {
            var pages = containers
                .Select(e => e.Content)
                .OfType<PageFrameContent>()
                .SelectMany(e => e.PageFrame.Elements)
                .Select(e => e.Page)
                .Distinct()
                .ToList();

            return pages;
        }
    }

    public class VisibleContainersChangedEventArgs : EventArgs
    {
        public VisibleContainersChangedEventArgs(List<PageFrameContainer> containers, PageRange pageRange, List<Page> pages, int direction)
        {
            Containers = containers;
            PageRange = pageRange;
            Pages = pages;
            Direction = direction;
        }

        public List<PageFrameContainer> Containers { get; }
        public PageRange PageRange { get; }
        public List<Page> Pages { get; }
        public int Direction { get; }
    }
}
