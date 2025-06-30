using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.ObjectModel;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView.Collections.Generic
{
    public interface ITreeListNode : ICloneable, IHasName, INotifyPropertyChanged
    {
    }

    public partial class TreeListNode<T> : BindableBase, IHasValue<T>, ITreeViewNode, IRenameable, IList<TreeListNode<T>>
        where T : ITreeListNode
    {
        private TreeListNode<T>? _parent;
        private readonly ObservableCollectionEx<TreeListNode<T>> _children;
        private bool _isSelected;
        private bool _isExpanded;
        private T _value;

        // NOTE: Enumerator をロックできるように旧式のロックオブジェクトを使用する
        private readonly object _lock = new();

#if DEBUG
        private static int _serialCounter;
        private readonly int _serial = _serialCounter++;
#endif

        public TreeListNode(T value)
        {
            _children = new ObservableCollectionEx<TreeListNode<T>>();

            SetValue(value);

            this.PropertyChanged += This_PropertyChanged;
            _children.CollectionChanged += Children_CollectionChanged;

            this.Children = new ReadOnlyObservableCollection<TreeListNode<T>>(_children);
            BindingOperations.EnableCollectionSynchronization(this.Children, new object());
        }


        [Subscribable]
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { _children.CollectionChanged += value; }
            remove { _children.CollectionChanged -= value; }
        }

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? RoutedCollectionChanged;

        [Subscribable]
        public event PropertyChangedEventHandler? RoutedPropertyChanged;

        [Subscribable]
        public event PropertyChangedEventHandler? RoutedValuePropertyChanged;


        public TreeListNode<T>? Parent => _parent;

        public ReadOnlyObservableCollection<TreeListNode<T>> Children { get; }

        public TreeListNode<T>? Previous
        {
            get
            {
                if (_parent == null) return null;

                var index = _parent._children.IndexOf(this);
                return _parent._children.ElementAtOrDefault(index - 1);
            }
        }

        public TreeListNode<T>? Next
        {
            get
            {
                if (_parent == null) return null;

                var index = _parent._children.IndexOf(this);
                return _parent._children.ElementAtOrDefault(index + 1);
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

        public bool CanExpand => _children.Count > 0;

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


        public TreeListNode<T> Root => _parent == null ? this : _parent.Root;

        public int Depth => _parent == null ? 0 : _parent.Depth + 1;

        ITreeNode? ITreeNode.Parent => Parent;

        IEnumerable<ITreeNode>? ITreeNode.Children => Children;

        ITreeViewNode? ITreeViewNode.Parent => Parent;

        IEnumerable<ITreeViewNode>? ITreeViewNode.Children => Children;

        public int Count => _children.Count;

        public bool IsReadOnly => ((ICollection<TreeListNode<T>>)_children).IsReadOnly;

        public TreeListNode<T> this[int index]
        {
            get { return _children[index]; }
            set
            {
                if (value._parent != null) throw new InvalidOperationException("Already registered.");
                lock (_lock)
                {
                    _children[index]._parent = null;
                    value._parent = this;
                    _children[index] = value;
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

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnRoutedCollectionChanged(this, e);
        }

        public void OnRoutedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RoutedCollectionChanged?.Invoke(sender, e);
            _parent?.OnRoutedCollectionChanged(sender, e);
        }

        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnRoutedPropertyChanged(this, e);
        }

        public void OnRoutedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RoutedPropertyChanged?.Invoke(sender, e);
            _parent?.OnRoutedPropertyChanged(sender, e);
        }

        private void Value_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnRoutedValuePropertyChanged(sender, e);
        }

        public void OnRoutedValuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RoutedValuePropertyChanged?.Invoke(sender, e);
            _parent?.OnRoutedValuePropertyChanged(sender, e);
        }

        /// <summary>
        /// ツリー構造の次のノードを取得します。必要に応じて展開された子ノードも考慮します。
        /// </summary>
        public TreeListNode<T>? GetNext(bool isUseExpand = true)
        {
            // TODO: 再設計が必要

            lock (_lock)
            {
                var parent = this.Parent;
                if (parent == null) return null;
                if (parent._children is null) throw new InvalidOperationException();

                if (isUseExpand && this.IsExpanded && _children is not null)
                {
                    return _children.First();
                }
                else if (parent._children.Last() == this)
                {
                    return parent.GetNext(false);
                }
                else
                {
                    int index = parent._children.IndexOf(this);
                    return parent._children[index + 1];
                }
            }
        }

        /// <summary>
        /// ツリー構造の前のノードを取得します。
        /// </summary>
        public TreeListNode<T>? GetPrev()
        {
            // TODO: 再設計が必要

            lock (_lock)
            {
                var parent = Parent;
                if (parent == null) return null;
                if (parent._children is null) throw new InvalidOperationException();

                if (parent._children.First() == this)
                {
                    return parent;
                }
                else
                {
                    int index = parent._children.IndexOf(this);
                    var prev = parent._children[index - 1];
                    return prev.GetLastChild();
                }
            }
        }

        private TreeListNode<T>? GetLastChild()
        {
            if (!this.IsExpanded) return this;
            if (_children is null) return this;

            lock (_lock)
            {
                return _children.Last().GetLastChild();
            }
        }

        public bool ParentContains(TreeListNode<T> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            lock (_lock)
            {
                return _parent != null && (_parent == target || _parent.ParentContains(target));
            }
        }

        /// <summary>
        /// 指定パスの<see cref="TreeListNode{T}"/>を取得
        /// </summary>
        /// <param name="path">指定パス</param>
        /// <param name="asFarAsPossible">指定パスが存在しない場合、存在する上位ノードを返す</param>
        /// <returns></returns>
        public TreeListNode<T>? GetNode(string? path, bool asFarAsPossible)
        {
            if (path == null) return null;

            var pathTokens = path.Trim(LoosePath.Separators).Split(LoosePath.Separators);
            return GetNode(pathTokens, asFarAsPossible);
        }

        /// <summary>
        /// 指定パスの<see cref="TreeListNode{T}"/>を取得
        /// </summary>
        public TreeListNode<T>? GetNode(IEnumerable<string> pathTokens, bool asFarAsPossible)
        {
            lock (_lock)
            {
                if (!pathTokens.Any())
                {
                    return this;
                }

                var token = pathTokens.First();

                var child = _children?.FirstOrDefault(e => e.Name == token);
                if (child != null)
                {
                    return child.GetNode(pathTokens.Skip(1), asFarAsPossible);
                }

                return asFarAsPossible ? this : null;
            }
        }

        public int GetIndex()
        {
            lock (_lock)
            {
                return _parent == null ? 0 : _parent._children.IndexOf(this);
            }
        }

        public int IndexOf(TreeListNode<T> item)
        {
            lock (_lock)
            {
                return _children.IndexOf(item);
            }
        }

        public bool Contains(TreeListNode<T> item)
        {
            lock (_lock)
            {
                return _children.Contains(item);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var node in _children)
                {
                    node._parent = null;
                }
                _children.Clear();
            }
        }

        public void Reset(IEnumerable<TreeListNode<T>> items)
        {
            lock (_lock)
            {
                foreach (var node in _children)
                {
                    node._parent = null;
                }
                foreach (var node in items)
                {
                    node._parent = this;
                }
                _children.Reset(items);
            }
        }

        public void Add(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            lock (_lock)
            {
                node._parent = this;
                _children.Add(node);
            }
        }

        public void Insert(int index, TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            lock (_lock)
            {
                if (index < 0 || _children.Count < index)
                {
                    index = _children.Count;
                }

                node._parent = this;
                _children.Insert(index, node);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _children.RemoveAt(index);
            }
        }

        public bool Remove(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != this) throw new InvalidOperationException();

            lock (_lock)
            {
                var isRemoved = _children.Remove(node);
                if (isRemoved)
                {
                    node._parent = null;
                }
                return isRemoved;
            }
        }

        public bool RemoveSelf()
        {
            if (_parent == null) return false;

            return _parent.Remove(this);
        }

        public void Move(int oldIndex, int newIndex)
        {
            Debug.Assert(oldIndex >= 0);
            Debug.Assert(newIndex >= 0);

            lock (_lock)
            {
                _children.Move(oldIndex, newIndex);
            }
        }

        // 階層をまたいだ移動にも対応
        public void MoveTo(TreeListNode<T> parent, int newIndex)
        {
            lock (_lock)
            {
                var oldIndex = this.Parent?._children.IndexOf(this) ?? -1;

                if (this.Parent == parent)
                {
                    parent.Move(oldIndex, newIndex);
                }
                else
                {
                    // 親を子に移動することはできない
                    if (parent.ParentContains(this))
                    {
                        return;
                    }
                    this.RemoveSelf();
                    parent.Insert(newIndex, this);
                }
            }
        }

        public void CopyTo(TreeListNode<T>[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _children.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<TreeListNode<T>> GetEnumerator()
        {
            lock (_lock)
            {
                foreach (var item in _children)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<TreeListNode<T>> WalkChildren()
        {
            lock (_lock)
            {
                foreach (var child in _children)
                {
                    yield return child;

                    foreach (var node in child.WalkChildren())
                    {
                        yield return node;
                    }
                }
            }
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

        public bool Equals(TreeListNode<T> other)
        {
            lock (_lock)
            {
                if (this.Value is not IEquatable<T> equtable) throw new InvalidOperationException("Value does not implement IEqutable.");

                if (!equtable.Equals(other.Value)) return false;
                if (_children.Count != other._children.Count) return false;
                for (int i = 0; i < _children.Count; ++i)
                {
                    if (!_children[i].Equals(other._children[i])) return false;
                }
                return true;
            }
        }

        public TreeListNode<T> Clone()
        {
            lock (_lock)
            {
                var node = new TreeListNode<T>((T)this.Value.Clone());

                foreach (var child in _children)
                {
                    node.Add(child.Clone());
                }

                return node;
            }
        }

        public override string ToString()
        {
            return Name;
        }
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
