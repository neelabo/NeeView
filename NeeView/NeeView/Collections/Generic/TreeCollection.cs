using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace NeeView.Collections.Generic
{
    // TODO: TreeListNode<T>のイベントを購読しCollectionChangedを発火するようにする？
    // TODO: thread safe (lock)

    public partial class TreeCollection<T> : BindableBase, IEnumerable<TreeListNode<T>>
        where T : ITreeListNode
    {
        private readonly Lock _lock = new();


        public TreeCollection(T rootValue)
        {
            Root = new TreeListNode<T>(rootValue);
        }


        [Subscribable]
        public event EventHandler<TreeCollectionChangeEventArgs<T>>? CollectionChanged;

        [Subscribable]
        public event PropertyChangedEventHandler? RoutedPropertyChanged
        {
            add => Root.RoutedPropertyChanged += value;
            remove => Root.RoutedPropertyChanged -= value;
        }

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? RoutedCollectionChanged
        {
            add => Root.RoutedCollectionChanged += value;
            remove => Root.RoutedCollectionChanged -= value;
        }


        public TreeListNode<T> Root { get; }


        public void Reset(IEnumerable<TreeListNode<T>> items)
        {
            lock (_lock)
            {
                Root.Clear();
                foreach (var item in items)
                {
                    Root.Add(item);
                }
                CollectionChanged?.Invoke(this, TreeCollectionChangeEventArgs<T>.Refresh());
            }
        }

        public void Add(TreeListNode<T> parent, TreeListNode<T> item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            lock (_lock)
            {
                parent.Add(item);
                CollectionChanged?.Invoke(this, TreeCollectionChangeEventArgs<T>.Add(item, parent.Children.IndexOf(item)));
            }
        }

        public void Insert(TreeListNode<T> parent, int index, TreeListNode<T> item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            lock (_lock)
            {
                if (index < 0 || parent.Children.Count < index)
                {
                    index = parent.Children.Count;
                }
                parent.Insert(index, item);
                CollectionChanged?.Invoke(this, TreeCollectionChangeEventArgs<T>.Add(item, index));
            }
        }

        public bool Remove(TreeListNode<T> parent, TreeListNode<T> item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (item.Parent != parent) return false;

            return RemoveSelf(item);
        }

        public bool RemoveSelf(TreeListNode<T> item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            lock (_lock)
            {
                var parent = item.Parent;
                if (item.RemoveSelf())
                {
                    CollectionChanged?.Invoke(this, TreeCollectionChangeEventArgs<T>.Remove(item, parent));
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Move(TreeListNode<T> parent, int oldIndex, int newIndex)
        {
            Debug.Assert(parent.Children is not null);
            Debug.Assert(oldIndex >= 0);
            Debug.Assert(newIndex >= 0);

            if (oldIndex == newIndex) return;

            lock (_lock)
            {
                var item = parent.Children[oldIndex];
                var target = parent.Children[newIndex];
                parent.Children.Move(oldIndex, newIndex);
                CollectionChanged?.Invoke(this, TreeCollectionChangeEventArgs<T>.Move(item, oldIndex, newIndex));
            }
        }

        // 階層をまたいだ移動にも対応
        public void Move(TreeListNode<T> parent, TreeListNode<T> item, int newIndex)
        {
            var oldIndex = item.Parent?.Children.IndexOf(item) ?? -1;

            if (item.Parent == parent)
            {
                Move(parent, oldIndex, newIndex);
            }
            else
            {
                lock (_lock)
                {
                    // 親を子に移動することはできない
                    if (parent.ParentContains(item))
                    {
                        return;
                    }
                    RemoveSelf(item);
                    Insert(parent, newIndex, item);
                }
            }
        }


        public IEnumerator<TreeListNode<T>> GetEnumerator()
        {
            return Root.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public enum TreeCollectionChangeAction
    {
        Add,
        Remove,
        Move,
        Refresh,
        Replace,
    }


    public class TreeCollectionChangeEventArgs<T> : EventArgs
        where T : ITreeListNode
    {
        private TreeCollectionChangeEventArgs(TreeCollectionChangeAction action, TreeListNode<T>? changeItem)
        {
            Action = action;
            if (action == TreeCollectionChangeAction.Remove)
            {
                OldItem = changeItem;
            }
            else
            {
                NewItem = changeItem;
            }
        }

        private TreeCollectionChangeEventArgs(TreeCollectionChangeAction action, TreeListNode<T>? newItem, TreeListNode<T>? oldItem, int newIndex = -1, int oldIndex = -1)
        {
            Action = action;
            NewItem = newItem;
            OldItem = oldItem;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }


        public TreeCollectionChangeAction Action { get; }
        public TreeListNode<T>? NewItem { get; }
        public TreeListNode<T>? OldItem { get; }
        public TreeListNode<T>? OldParent { get; init; }
        public int NewIndex { get; init; } = -1;
        public int OldIndex { get; init; } = -1;


        private void CheckFormat()
        {
            switch (Action)
            {
                case TreeCollectionChangeAction.Add:
                    Debug.Assert(NewItem is not null);
                    Debug.Assert(NewIndex >= 0);
                    break;

                case TreeCollectionChangeAction.Remove:
                    Debug.Assert(OldItem is not null);
                    Debug.Assert(OldParent is not null);
                    break;

                case TreeCollectionChangeAction.Move:
                    Debug.Assert(NewItem is not null);
                    Debug.Assert(NewIndex >= 0);
                    Debug.Assert(OldIndex >= 0);
                    break;

                case TreeCollectionChangeAction.Replace:
                    Debug.Assert(NewItem is not null);
                    Debug.Assert(OldItem is not null);
                    break;
            }
        }

        public static TreeCollectionChangeEventArgs<T> Refresh()
        {
            return new TreeCollectionChangeEventArgs<T>(TreeCollectionChangeAction.Refresh, null);
        }

        public static TreeCollectionChangeEventArgs<T> Replace(TreeListNode<T> oldItem, TreeListNode<T> newItem)
        {
            return new TreeCollectionChangeEventArgs<T>(TreeCollectionChangeAction.Replace, newItem, oldItem);
        }

        public static TreeCollectionChangeEventArgs<T> Move(TreeListNode<T> item, int oldIndex, int newIndex)
        {
            return new TreeCollectionChangeEventArgs<T>(TreeCollectionChangeAction.Move, item, null, newIndex, oldIndex);
        }

        public static TreeCollectionChangeEventArgs<T> Add(TreeListNode<T> item, int index)
        {
            return new TreeCollectionChangeEventArgs<T>(TreeCollectionChangeAction.Add, item) { NewIndex = index };
        }

        public static TreeCollectionChangeEventArgs<T> Remove(TreeListNode<T> item, TreeListNode<T>? parent)
        {
            return new TreeCollectionChangeEventArgs<T>(TreeCollectionChangeAction.Remove, item) { OldParent = parent };
        }
    }
}
