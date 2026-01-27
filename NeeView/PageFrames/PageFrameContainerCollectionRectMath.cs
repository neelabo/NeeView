using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrameContainerCollection の表示領域に冠する処理
    /// </summary>
    public class PageFrameContainerCollectionRectMath
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainerCollection _containers;
        private readonly PageFrameRectMath _math;

        public PageFrameContainerCollectionRectMath(PageFrameContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;

            _math = new PageFrameRectMath(_context);
        }


        /// <summary>
        /// コンテナの表示領域判定
        /// TODO: ViewContainers から判別できるよね？
        /// </summary>
        public bool WithinView(Rect viewRect, LinkedListNode<PageFrameContainer> node)
        {
            return _math.GetConflict(node.Value.Rect, viewRect).IsConflict();
        }

        /// <summary>
        /// 表示されているコンテナを収集
        /// </summary>
        /// <returns></returns>
        public List<PageFrameContainer> CollectViewContainers(Rect viewRect)
        {
            return _containers.Collect<PageFrameContent>()
                .Where(e => _math.GetConflict(e.Rect, viewRect).IsConflict())
                .ToList();
        }

        /// <summary>
        /// 表示の中心に最も近いコンテナを取得 
        /// ... 用途が？
        /// </summary>
        public LinkedListNode<PageFrameContainer>? GetViewCenterContainer(Rect viewRect)
        {
            return GetViewCenterContainer(viewRect, _containers.CollectNode<PageFrameContent>());
        }

        /// <summary>
        /// 表示の中心に最も近いコンテナを取得 
        /// </summary>
        public LinkedListNode<PageFrameContainer>? GetViewCenterContainer(Rect viewRect, IEnumerable<LinkedListNode<PageFrameContainer>> nodes)
        {
            var node = nodes.FirstOrDefault(e => _math.GetConflict(e.Value.Rect, viewRect).IsCentered())
                ?? nodes.MinBy(e => GetCenterDistance(e.Value));

            return node;

            double GetCenterDistance(PageFrameContainer container)
            {
                var rect = container.Rect;
                var center = _math.GetCenter(viewRect);
                return Math.Min(Math.Abs(_math.GetMin(rect) - center), Math.Abs(_math.GetMax(rect) - center));
            }
        }

        /// <summary>
        /// 表示の中でもっともアンカーに近いコンテンなを取得。
        /// 表示の更新の規準に使用する。
        /// </summary>
        /// <param name="viewRect"></param>
        /// <param name="filterItem"></param>
        /// <returns></returns>
        public LinkedListNode<PageFrameContainer>? GetViewAnchorContainer(Rect viewRect)
        {
            // TODO: ViewRect内のコンテナを取得する処理が PageFrameContainersArea と重複している
            var nodes = _containers.CollectNode<PageFrameContent>()
                .Where(e => _math.GetConflict(e.Value.Rect, viewRect).IsConflict())
                .ToList();

            if (!nodes.Any())
            {
                return _containers.Anchor.Node;
            }

            if (nodes.Contains(_containers.Anchor.Node))
            {
                return _containers.Anchor.Node;
            }

            var node = nodes.MinBy(e => GetContainerDistance(_containers.Anchor.Container, e.Value));
            return node;

            static double GetContainerDistance(PageFrameContainer c1, PageFrameContainer c2)
            {
                return Math.Abs(c1.Identifier.Value - c2.Identifier.Value);
            }
        }

        /// <summary>
        /// コンテナのアライメントを計算。ページ移動時の初期位置用
        /// 表示幅より大きい場合は移動方向に依存したアライメントになる。
        /// TODO: ここで実装はおかしい？
        /// </summary>
        /// <param name="container"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public PageFrameAlignment GetContainerAlignment(Rect viewRect, LinkedListNode<PageFrameContainer> container, LinkedListDirection direction)
        {
            if (_math.GetWidth(container.Value.Rect) <= _math.GetWidth(viewRect))
            {
                return PageFrameAlignment.Center;
            }
            else
            {
                return direction == LinkedListDirection.Next ? PageFrameAlignment.Min : PageFrameAlignment.Max;
            }
        }

        /// <summary>
        /// 現在のすべてのコンテナを含むエリアを求める。
        /// スクロールエリア用
        /// </summary>
        public Rect GetContainersRect(Rect viewRect)
        {
            var containers = _containers.Collect<PageFrameContent>(); // CollectViewContainers(viewRect);
            if (!containers.Any()) return viewRect;

            var contentRect = containers.Aggregate(containers.First().Rect, (n, next) => Rect.Union(n, next.Rect));

            // 表領域の半分をマージンとして追加
            double marginRate = 0.5;
            if (_context.FrameOrientation == PageFrameOrientation.Horizontal)
            {
                contentRect.Inflate(viewRect.Width * marginRate + _context.FrameMargin, 0.0);
            }
            else
            {
                contentRect.Inflate(0.0, viewRect.Height * marginRate + _context.FrameMargin);
            }

            return contentRect;
        }
    }
}
