using NeeView.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView.PageFrames
{
    // TODO: PageFrameViewRect を参照しても良い
    public class PageFrameContainerCleaner
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainerCollection _containers;
        private readonly PageFrameRectMath _math;


        public PageFrameContainerCleaner(PageFrameContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;

            _math = new PageFrameRectMath(_context);
        }


        /// <summary>
        /// 表示領域外のコンテナを削除
        /// </summary>
        public void Cleanup(Rect viewRect)
        {
            Cleanup(viewRect, LinkedListDirection.Previous);
            Cleanup(viewRect, LinkedListDirection.Next);
            //_layout.Layout(Anchor.Node); .. これいる？
        }

        private void Cleanup(Rect viewRect, LinkedListDirection direction)
        {
            var node = _containers.CollectNode(direction).Where(e => e.Value.Content is PageFrameContent).FirstOrDefault(e => _math.GetConflict(e.Value.Rect, viewRect).IsOver(direction.ToSign()));
            RemoveContainers(node, direction);
        }

        private bool critical;

        private void RemoveContainers(LinkedListNode<PageFrameContainer>? node, LinkedListDirection direction)
        {
            if (node is null) return;

            Debug.Assert(!critical);
            critical = true;

            while (node is not null)
            {
                var next = node.GetNext(direction);

                if (node == _containers.Anchor.Node)
                {
                    var prev = node.GetPrevious(direction);
                    if (prev?.Value.Content is PageFrameContent)
                    {
                        _containers.Anchor.Set(prev);
                        //Debug.WriteLine($"# Anchor.Change: {_anchor.Value}");
                    }
                }

                if (!node.Value.IsLocked && node != _containers.Anchor.Node) // Anchor も削除しない。が、これは SelectedItem として別に管理すべき？
                {
                    _containers.RemoveContainerNode(node);
                }
                node = next;
            }

            critical = false;
        }

        public void ClearAll()
        {
            var node = _containers.CollectNode().First();
            while (node is not null)
            {
                var next = node.GetNext(LinkedListDirection.Next);
                if (node.Value.Content is PageFrameContent)
                {
                    _containers.RemoveContainerNode(node);
                }
                else
                {
                    node.Value.X = 0.0;
                    node.Value.Y = 0.0;
                }
                node = next;
            }
        }
    }

}
