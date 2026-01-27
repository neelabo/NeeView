using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.PageFrames
{
    /// <summary>
    /// コンテナの配置
    /// </summary>
    public class PageFrameContainerLayout
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainerCollection _containers;


        public PageFrameContainerLayout(PageFrameContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;
        }


        public event EventHandler? LayoutChanged;


        /// <summary>
        /// レイアウトアニメーションをキャンセルして表示を確定する
        /// </summary>
        public void Flush()
        {
            foreach (var container in _containers)
            {
                container.FlushLayout();
            }
        }

        /// <summary>
        /// アンカーを基準に再配置
        /// </summary>
        public void Layout()
        {
            Layout(_containers.Anchor.Node);
        }

        /// <summary>
        /// 基準コンテナを指定してすべてのコンテナを配置
        /// </summary>
        /// <param name="anchor"></param>
        public void Layout(LinkedListNode<PageFrameContainer> anchor)
        {
            Layout(anchor.Next, LinkedListDirection.Next);
            Layout(anchor.Previous, LinkedListDirection.Previous);

            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 開始コンテナを指定して指定方向すべてのコンテナを配置
        /// </summary>
        /// <param name="node"></param>
        /// <param name="direction"></param>
        private void Layout(LinkedListNode<PageFrameContainer>? node, LinkedListDirection direction)
        {
            while (node != null)
            {
                Layout(node.Value, node.GetPrevious(direction)?.Value, direction);
                node = node.GetNext(direction);
            }
        }

        /// <summary>
        /// コンテナ配置
        /// </summary>
        /// <param name="element">配置するコンテナ</param>
        /// <param name="referenceElement">配置の規準となるコンテナ</param>
        /// <param name="direction">配置方向</param>
        private void Layout(PageFrameContainer element, PageFrameContainer? referenceElement, LinkedListDirection direction)
        {
            if (referenceElement is null) return;

            //Debug.Assert(element != _anchor.Value);

            var layoutDirection = _context.FrameOrientation == PageFrameOrientation.Horizontal
                ? (_context.ReadOrder == PageReadOrder.LeftToRight ? direction : direction.Reverse()) == LinkedListDirection.Previous ? PageFrameDirection.Left : PageFrameDirection.Right
                : direction == LinkedListDirection.Previous ? PageFrameDirection.Up : PageFrameDirection.Down;

            switch (layoutDirection)
            {
                case PageFrameDirection.Left:
                    element.X = referenceElement.X - element.Width - _context.FrameMargin;
                    element.Y = -element.Height * 0.5;
                    //element.Y = 0.0;
                    break;
                case PageFrameDirection.Right:
                    element.X = referenceElement.X + referenceElement.Width + _context.FrameMargin;
                    element.Y = -element.Height * 0.5;
                    //element.Y = 0.0;
                    break;
                case PageFrameDirection.Up:
                    //element.X = 0.0;
                    element.X = -element.Width * 0.5;
                    element.Y = referenceElement.Y - element.Height - _context.FrameMargin;
                    break;
                case PageFrameDirection.Down:
                    //element.X = 0.0;
                    element.X = -element.Width * 0.5;
                    element.Y = referenceElement.Y + referenceElement.Height + _context.FrameMargin;
                    break;
            }

            if (!element.IsLayouted)
            {
                element.FlushLayout();
                element.IsLayouted = true;

                // 初期配置後、変形アニメーション適用
                //element.Duration = 500.0;
                element.Duration = 250.0;
                element.IsHorizontalAnimationEnabled = _context.FrameOrientation == PageFrameOrientation.Horizontal;
                element.IsVerticalAnimationEnabled = _context.FrameOrientation == PageFrameOrientation.Vertical;
            }

            //Debug.WriteLine($"Layout: {referenceElement} -> {element}, {direction}");
        }

    }
}
