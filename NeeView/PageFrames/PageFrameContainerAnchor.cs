using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeeView.PageFrames
{
    public class PageFrameContainerAnchor
    {
        private PageFrameContainerCollection _containers;
        private LinkedListNode<PageFrameContainer> _node;
        private LinkedListDirection _direction;


        public PageFrameContainerAnchor(PageFrameContainerCollection containers)
        {
            _containers = containers;

            Set(null, LinkedListDirection.Next);
        }


        public LinkedListNode<PageFrameContainer> Node
        {
            get
            {
                if (!_containers.ContainsNode(_node))
                {
                    _node = GetAnchorNode(_direction);
                }
                return _node;
            }
        }

        public LinkedListDirection Direction => _direction;
        public PageFrameContainer Container => Node.Value;


        public void Set(LinkedListNode<PageFrameContainer> node)
        {
            Debug.Assert(_containers.ContainsNode(node));

            _node = node;
        }

        [MemberNotNull(nameof(_node))]
        public void Set(LinkedListNode<PageFrameContainer>? node, LinkedListDirection direction)
        {
            Debug.Assert(node is null || _containers.ContainsNode(node));

            _node = node ?? GetAnchorNode(direction);
            _direction = direction;
        }

        private LinkedListNode<PageFrameContainer> GetAnchorNode(LinkedListDirection direction)
        {
            return direction switch
            {
                LinkedListDirection.Previous => _containers.CollectNode<PageFrameContent>().LastOrDefault() ?? _containers.CollectNode().Last(),
                LinkedListDirection.Next => _containers.CollectNode<PageFrameContent>().FirstOrDefault() ?? _containers.CollectNode().First(),
                _ => throw new InvalidOperationException("containers is empty.")
            };
        }

        // フレーム破綻がないようにアンカーの方向を調整
        // ページサイズ１のみ補正。ページサイズ２の場合表示が変化してしまう可能性がある。
        public void FixDirection()
        {
            if (_node.Value.FrameRange.PageSize == 1)
            {
                var position = _node.Value.FrameRange.Top(_direction.ToSign());
                _direction = position.Part == 0 ? LinkedListDirection.Next : LinkedListDirection.Previous;
            }
        }

        public override string ToString()
        {
            return $"{Container}, {Direction}";
        }
    }
}
