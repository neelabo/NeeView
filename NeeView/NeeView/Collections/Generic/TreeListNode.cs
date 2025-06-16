using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NeeView.Collections.Generic
{
    public interface ITreeListNode : ICloneable, IHasName, INotifyPropertyChanged
    {
    }

    public partial class TreeListNode<T> : BindableBase, IEnumerable<TreeListNode<T>>, IHasValue<T>, ITreeViewNode, IRenameable
        where T : ITreeListNode
    {
        private TreeListNode<T>? _parent;
        private readonly ObservableCollection<TreeListNode<T>> _children;
        private bool _isSelected;
        private bool _isExpanded;
        private T _value;

#if DEBUG
        private static int _serialCounter;
        private readonly int _serial = _serialCounter++;
#endif

        public TreeListNode(T value)
        {
            _children = new ObservableCollection<TreeListNode<T>>();
            SetValue(value);

            this.PropertyChanged += This_PropertyChanged;
            _children.CollectionChanged += Children_CollectionChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? RoutedPropertyChanged;

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? RoutedCollectionChanged;


        public TreeListNode<T>? Parent => _parent;

        public ObservableCollection<TreeListNode<T>> Children => _children;

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

        public string Name => Value?.Name ?? "";

        public string Path => string.Join("\\", Hierarchy.Select(e => e.Name));

        public bool CanExpand => Children.Count > 0;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    SetValue(value);
                    RaisePropertyChanged();
                }
            }
        }

        [MemberNotNull(nameof(_value))]
        private void SetValue(T value)
        {
            if (_value is not null)
            {
                _value.PropertyChanged -= Value_PropertyChanged;
            }
            _value = value;
            _value.PropertyChanged += Value_PropertyChanged;
        }

        private void Value_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnRoutedPropertyChanged(sender, e);
        }

        public TreeListNode<T> Root => _parent == null ? this : _parent.Root;

        public int Depth => _parent == null ? 0 : _parent.Depth + 1;

        ITreeNode? ITreeNode.Parent => Parent;

        IEnumerable<ITreeNode>? ITreeNode.Children => Children;

        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnRoutedPropertyChanged(this, e);
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnRoutedCollectionChanged(this, e);
        }

        public void OnRoutedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RoutedPropertyChanged?.Invoke(sender, e);
            _parent?.OnRoutedPropertyChanged(sender, e);
        }

        public void OnRoutedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RoutedCollectionChanged?.Invoke(sender, e);
            _parent?.OnRoutedCollectionChanged(sender, e);
        }

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

        public void Clear()
        {
            foreach (var node in _children)
            {
                node._parent = null;
            }
            _children.Clear();
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

            foreach (var child in _children)
            {
                node.Add(child.Clone());
            }

            return node;
        }

        public override string ToString()
        {
            return $"{Path}";
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

        public string GetRenameText()
        {
            return Value is IRenameable renamable ? renamable.GetRenameText() : "";
        }

        public bool CanRename()
        {
            return Value is IRenameable renamable ? renamable.CanRename() : false;
        }

        public ValueTask<bool> RenameAsync(string name)
        {
            return Value is IRenameable renamable ? renamable.RenameAsync(name) : ValueTask.FromResult(false);
        }

        #endregion
    }


    public class TreeListNodeMemento<T>
        where T : ITreeListNode
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
