using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeeView.Collections.ObjectModel
{
    /// <summary>
    /// Reverse ObservableCollection
    /// </summary>
    /// <remarks>
    /// CollectionViewSource のソースに逆順で渡したいときに使用する。
    /// SortDescription で並び替えるより軽い。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ReverseObservableCollection<T> :  IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ObservableCollectionEx<T> _collection;

        public ReverseObservableCollection(ObservableCollectionEx<T> collection)
        {
            _collection = collection;
            ((INotifyPropertyChanged)_collection).PropertyChanged += Collection_PropertyChanged;
            _collection.CollectionChanged += Collection_CollectionChanged;
        }


        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        private event PropertyChangedEventHandler? PropertyChanged;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        public int Count => _collection.Count;

        public bool IsReadOnly => ((IList<T>)_collection).IsReadOnly;

        public bool IsFixedSize => ((IList)_collection).IsFixedSize;

        public bool IsSynchronized => ((ICollection)_collection).IsSynchronized;

        public object SyncRoot => ((ICollection)_collection).SyncRoot;

        public T this[int index]
        {
            get => _collection[ReverseIndex(index)];
            set => _collection[ReverseIndex(index)] = value;
        }

        object? IList.this[int index]
        {
            get => ((IList)_collection)[ReverseIndex(index)];
            set => ((IList)_collection)[ReverseIndex(index)] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in _collection.Reverse())
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void Collection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(e.PropertyName == nameof(Count) || e.PropertyName == "Item[]");
            PropertyChanged?.Invoke(this, e);
        }

        private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CollectionChanged?.Invoke(sender, CreateAddEventArgs(e));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(sender, CreateRemoveEventArgs(e));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(sender, CreateReplaceEventArgs(e));
                    break;
                case NotifyCollectionChangedAction.Move:
                    CollectionChanged?.Invoke(sender, CreateMoveEventArgs(e));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged?.Invoke(sender, CreateResetEventArgs(e));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ReverseIndex(int index)
        {
            return _collection.Count - 1 - index;
        }

        private NotifyCollectionChangedEventArgs CreateAddEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Add);
            Debug.Assert(e.OldItems == null);
            var items = e.NewItems?.Cast<T>().Reverse().ToList() ?? throw new ArgumentNullException(nameof(e.NewItems));
            var index = ReverseIndex(e.NewStartingIndex + items.Count - 1);
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index);
        }

        private NotifyCollectionChangedEventArgs CreateRemoveEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Remove);
            Debug.Assert(e.NewItems == null);
            var items = e.OldItems?.Cast<T>().Reverse().ToList() ?? throw new ArgumentNullException(nameof(e.OldItems));
            var index = ReverseIndex(e.OldStartingIndex + items.Count - 1) + 1; // +1 するのは削除前のサイズでのインデックスにするため
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index);
        }

        private NotifyCollectionChangedEventArgs CreateReplaceEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Replace);
            Debug.Assert(e.NewItems is not null && e.OldItems is not null);
            var newItems = e.NewItems?.Cast<T>().Reverse().ToList() ?? throw new ArgumentNullException(nameof(e.NewItems));
            var oldItems = e.OldItems?.Cast<T>().Reverse().ToList() ?? throw new ArgumentNullException(nameof(e.OldItems));
            var index = ReverseIndex(e.NewStartingIndex + newItems.Count - 1);
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
        }

        private NotifyCollectionChangedEventArgs CreateMoveEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Move);
            Debug.Assert(e.NewItems is not null && e.OldItems is not null);
            Debug.Assert(Enumerable.SequenceEqual(e.NewItems.Cast<object>(), e.OldItems.Cast<object>()));
            var items = e.NewItems?.Cast<T>().Reverse().ToList() ?? throw new ArgumentNullException(nameof(e.NewItems));
            var newIndex = ReverseIndex(e.NewStartingIndex + items.Count - 1);
            var oldIndex = ReverseIndex(e.OldStartingIndex + items.Count - 1);
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex);
        }

        private NotifyCollectionChangedEventArgs CreateResetEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Reset);
            return e;
        }

        public void Add(T item)
        {
            _collection.Insert(0, item);
        }

        public int Add(object? value)
        {
            ((IList)_collection).Insert(0, value);
            return ReverseIndex(0);
        }
        
        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(T value)
        {
            return _collection.Contains(value);
        }

        public bool Contains(object? value)
        {
            return ((IList)_collection).Contains(value);
        }

        public int IndexOf(T item)
        {
            return ReverseIndex(_collection.IndexOf(item));
        }

        public int IndexOf(object? value)
        {
            return ReverseIndex(((IList)_collection).IndexOf(value));
        }

        public void Insert(int index, T item)
        {
            _collection.Insert(ReverseIndex(index), item);
        }

        public void Insert(int index, object? value)
        {
            ((IList)_collection).Insert(ReverseIndex(index), value);
        }

        public bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        public void Remove(object? value)
        {
            ((IList)_collection).Remove(value);
        }

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(ReverseIndex(index));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array.Length <= 0) return;
            var max = Math.Min(_collection.Count, arrayIndex + array.Length);
            for(int i = arrayIndex; i < max; i++)
            {
                array[i] = _collection[ReverseIndex(i)];
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array.Length <= 0) return;
            var max = Math.Min(_collection.Count, index + array.Length);
            var list = (IList)array;
            for (int i = index; i < max; i++)
            {
                list[i] = _collection[ReverseIndex(i)];
            }
        }
    }


    public static class NotifyCollectionChangedEventArgsExtensions
    {
        [Conditional("DEBUG")]
        public static void Dump(this NotifyCollectionChangedEventArgs e)
        {
            const string indent = "    ";
            Debug.WriteLine(nameof(NotifyCollectionChangedEventArgs) + ":");
            Debug.WriteLine(indent + $"Action: {e.Action}");
            var newItemsString = e.NewItems is null ? "null" : ("[" + string.Join(",", e.NewItems.Cast<string>()) + "]");
            var oldItemsString = e.OldItems is null ? "null" : ("[" + string.Join(",", e.OldItems.Cast<string>()) + "]");
            Debug.WriteLine(indent + $"NewItems: {e.NewStartingIndex}, {newItemsString}");
            Debug.WriteLine(indent + $"OldItems: {e.OldStartingIndex}, {oldItemsString}");
        }
    }
}
