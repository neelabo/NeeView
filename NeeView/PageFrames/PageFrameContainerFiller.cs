using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    public class PageFrameContainerFiller
    {
        private readonly PageFrameContext _context;
        private readonly BookContext _bookContext;
        private readonly PageFrameContainerCollection _containers;
        private readonly PageFrameContainerCollectionRectMath _rectMath;
        private readonly PageFrameRectMath _math;


        public PageFrameContainerFiller(PageFrameContext context, BookContext bookContext, PageFrameContainerCollection containers, PageFrameContainerCollectionRectMath rectMath)
        {
            _context = context;
            _bookContext = bookContext;
            _containers = containers;
            _rectMath = rectMath;

            _math = new PageFrameRectMath(_context);
        }


        /// <summary>
        /// 画面表示に必要なコンテナの作成。
        /// アンカーを基準に画面を埋める
        /// </summary>
        public void FillContainers(Rect viewRect, LinkedListNode<PageFrameContainer> anchor)
        {
            _containers.UpdateContainer(anchor);
            FillContainers(anchor, GetViewSpace(viewRect, anchor));
        }


        /// <summary>
        /// 画面表示に必要なコンテナの作成。
        /// スクロール後を想定した範囲。
        /// </summary>
        /// <param name="anchor">規準となる中央に表示するコンテナ</param>
        /// <param name="direction"></param>
        public void FillContainersWhenAligned(Rect viewRect, LinkedListNode<PageFrameContainer> anchor, LinkedListDirection direction)
        {
            var alignment = _rectMath.GetContainerAlignment(viewRect, anchor, direction);
            FillContainers(anchor, GetViewSpaceWhenAligned(viewRect, anchor, alignment));
        }


        private void FillContainers(LinkedListNode<PageFrameContainer> anchor, BlankSpace space)
        {
            if (!_context.IsLoopPage)
            {
                _containers.RemoveOutRangeContainers(_bookContext.PageRange);
            }

            FillContainers(anchor, LinkedListDirection.Previous, space.Previous);
            FillContainers(anchor, LinkedListDirection.Next, space.Next);
        }

        private void FillContainers(LinkedListNode<PageFrameContainer> anchor, LinkedListDirection direction, double rest)
        {
            LinkedListNode<PageFrameContainer>? node = anchor;
            while (0.0 < rest)
            {
                var pos = node.Value.FrameRange.Next(direction.ToSign());
                node = node.GetNext(direction);
                // NOTE: 連続性に問題があったり更新が必要である場合は生成する
                if (node?.Value.Content is not PageFrameContent item || item.IsDirty || item.FrameRange.Top(direction.ToSign()) != pos || !IsValidContainerFormat(node))
                {

                    if (node?.Value.Content is PageFrameContent)
                    {
                        SetContainerAlignment(anchor, node);
                    }
                    node = _containers.EnsureLatestContainerNode(pos, direction, CreateContainerNodeOptions.Default);
                }
                if (node is null) break;
                rest -= GetContainerSpan(node.Value);
            }
        }

        // モードに適したパートサイズ？
        private bool IsValidContainerFormat(LinkedListNode<PageFrameContainer> node)
        {
            if (node.Value.Content is not PageFrameContent item) return true;

            var allowPartPage = _context.PageMode == PageMode.SinglePage && _context.IsSupportedDividePage;
            var partSize = item.FrameRange.PartSize;

            var result = allowPartPage || partSize == 2;
            //Debug.Assert(result);
            return result;
        }


        private void SetContainerAlignment(LinkedListNode<PageFrameContainer> anchor, LinkedListNode<PageFrameContainer> node)
        {
            switch (node.Value.CompareTo(anchor.Value))
            {
                case < 0:
                    node.Value.HorizontalAlignment = _context.FrameOrientation == PageFrameOrientation.Horizontal ? HorizontalAlignment.Right : HorizontalAlignment.Center;
                    node.Value.VerticalAlignment = _context.FrameOrientation == PageFrameOrientation.Horizontal ? VerticalAlignment.Center : VerticalAlignment.Bottom;
                    break;
                case > 0:
                    node.Value.HorizontalAlignment = _context.FrameOrientation == PageFrameOrientation.Horizontal ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                    node.Value.VerticalAlignment = _context.FrameOrientation == PageFrameOrientation.Horizontal ? VerticalAlignment.Center : VerticalAlignment.Top;
                    break;
                default:
                    node.Value.HorizontalAlignment = HorizontalAlignment.Center;
                    node.Value.VerticalAlignment = VerticalAlignment.Center;
                    break;
            }
        }

        private BlankSpace GetViewSpaceWhenAligned(Rect viewRect, LinkedListNode<PageFrameContainer> anchor, PageFrameAlignment alignment)
        {
            double rest = _math.GetWidth(viewRect) - GetContainerSpan(anchor.Value);

            return alignment switch
            {
                PageFrameAlignment.Min => new BlankSpace(0.0, rest),
                PageFrameAlignment.Center => new BlankSpace(rest * 0.5, rest * 0.5),
                PageFrameAlignment.Max => new BlankSpace(rest, 0.0),
                _ => throw new InvalidEnumArgumentException(nameof(alignment)),
            };
        }

        private BlankSpace GetViewSpace(Rect viewRect, LinkedListNode<PageFrameContainer> anchor)
        {
            var conflict = _math.GetConflict(anchor.Value.Rect, viewRect);

            var restPrevious = conflict.GetDistance(LinkedListDirection.Previous.ToSign()) - _context.FrameMargin;
            var restNext = conflict.GetDistance(LinkedListDirection.Next.ToSign()) - _context.FrameMargin;

            return new BlankSpace(restPrevious, restNext);
        }

        private double GetContainerSpan(PageFrameContainer container)
        {
            return _math.GetWidth(container.Rect) + _context.FrameMargin;
        }


        /// <summary>
        /// [inner class] 余白データ
        /// </summary>
        /// <param name="Previous"></param>
        /// <param name="Next"></param>
        private record struct BlankSpace(double Previous, double Next)
        {
            public readonly double GetSpace(LinkedListDirection direction)
            {
                return direction == LinkedListDirection.Previous ? Previous : Next;
            }
        }
    }
}
