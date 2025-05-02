using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView.Collections.Generic
{
    public class TreeListNode<T> : BindableBase, IEnumerable<TreeListNode<T>>, IHasValue<T>
        where T : ICloneable
    {
        private TreeListNode<T>? _parent;
        private ObservableCollection<TreeListNode<T>> _children;
        private bool _isExpanded;
        private T _value;


        public TreeListNode(T value)
        {
            _children = new ObservableCollection<TreeListNode<T>>();
            _value = value;
        }


        public TreeListNode<T>? Parent => _parent;

        public ObservableCollection<TreeListNode<T>> Children
        {
            get => _children;
            private set => _children = value;
        }

        public TreeListNode<T>? Previous
        {
            get
            {
                if (_parent == null) return null;

                var index = _parent._children.IndexOf(this);
                return _parent.Children.ElementAtOrDefault(index - 1);
            }
        }

        public TreeListNode<T>? Next
        {
            get
            {
                if (_parent == null) return null;

                var index = _parent._children.IndexOf(this);
                return _parent.Children.ElementAtOrDefault(index + 1);
            }
        }

        /// <summary>
        /// 階層コレクション
        /// </summary>
        public IEnumerable<TreeListNode<T>> Hierarchy
        {
            get
            {
                return HierarchyReverse.Reverse();
            }
        }

        public IEnumerable<TreeListNode<T>> HierarchyReverse
        {
            get
            {
                yield return this;
                for (var parent = Parent; parent != null; parent = parent.Parent)
                {
                    yield return parent;
                }
            }
        }

        public bool CanExpand => Children.Count > 0;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }


        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public TreeListNode<T> Root => _parent == null ? this : _parent.Root;
        public int Depth => _parent == null ? 0 : _parent.Depth + 1;


        public bool ParentContains(TreeListNode<T> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return _parent != null && (_parent == target || _parent.ParentContains(target));
        }

        public TreeListNode<T>? Find(T value)
        {
            return _children.FirstOrDefault(e => EqualityComparer<T>.Default.Equals(e.Value, value));
        }

        public int GetIndex()
        {
            return _parent == null ? 0 : _parent._children.IndexOf(this);
        }

        public void Add(T value)
        {
            Add(new TreeListNode<T>(value));
        }

        public void Add(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            node._parent = this;
            _children.Add(node);
        }

        public void Insert(int index, TreeListNode<T> node)
        {
            if (index < 0 || index > _children.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            node._parent = this;
            _children.Insert(index, node);
        }

        public void Insert(TreeListNode<T> target, int direction, TreeListNode<T> node)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.Parent != this) throw new InvalidOperationException();
            if (direction != -1 && direction != +1) throw new ArgumentOutOfRangeException(nameof(direction));

            var index = _children.IndexOf(target);
            if (direction == +1)
            {
                index++;
            }

            Insert(index, node);
        }

        public void Sort(IComparer<TreeListNode<T>> comparer)
        {
            _children = new ObservableCollection<TreeListNode<T>>(_children.OrderBy(e => e, comparer));
        }

        public bool Remove(T value)
        {
            var node = Find(value);
            if (node != null)
            {
                return Remove(node);
            }
            else
            {
                return false;
            }
        }

        public bool Remove(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != this) throw new InvalidOperationException();

            var isRemoved = _children.Remove(node);
            if (isRemoved)
            {
                node._parent = null;
            }

            return isRemoved;
        }

        internal bool RemoveSelf()
        {
            if (_parent == null) return false;

            return _parent.Remove(this);
        }

        public IEnumerable<TreeListNode<T>> GetExpandedCollection()
        {
            foreach (var child in _children)
            {
                yield return child;

                if (child._isExpanded)
                {
                    foreach (var node in child.GetExpandedCollection())
                    {
                        yield return node;
                    }
                }
            }
        }

        public bool CompareOrder(TreeListNode<T> x, TreeListNode<T> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));


            var parentsX = x.Hierarchy.ToList();
            var parentsY = y.Hierarchy.ToList();

            var limit = Math.Min(parentsX.Count, parentsY.Count);

            for (int depth = 0; depth < limit; ++depth)
            {
                if (parentsX[depth] != parentsY[depth])
                {
                    if (depth == 0) throw new InvalidOperationException();

                    var parent = parentsX[depth - 1];
                    var indexX = parent.Children.IndexOf(parentsX[depth]);
                    var indexY = parent.Children.IndexOf(parentsY[depth]);
                    return indexX < indexY;
                }
            }

            return parentsX.Count < parentsY.Count;
        }

        public TreeListNode<T> Clone()
        {
            var node = new TreeListNode<T>((T)this.Value.Clone());

            foreach(var child in _children)
            {
                node.Add(child.Clone());
            }

            return node;
        }

        #region IEnumerable support

        public IEnumerator<TreeListNode<T>> GetEnumerator()
        {
            // Note: 自身のインスタンスは含まない

            foreach (var child in _children)
            {
                yield return child;

                var enumerator = child.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }


    public class TreeListNodeMemento<T>
        where T : ICloneable
    {
        public TreeListNodeMemento(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Parent == null) throw new InvalidOperationException("Parent is null.");

            Node = node;
            Parent = node.Parent;
            Index = node.GetIndex();
        }

        public TreeListNode<T> Node { get; private set; }
        public TreeListNode<T> Parent { get; private set; }
        public int Index { get; private set; }
    }

}
