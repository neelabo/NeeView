using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NeeView.Collections.ObjectModel
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>, IResetCollrction<T>, IResetCollection
    {
        public ObservableCollectionEx() : base()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableCollectionEx(List<T> collection) : base(collection)
        {
        }


        void IResetCollection.Reset(IEnumerable<object> newItems)
        {
            Reset(newItems.Cast<T>());
        }

        public void Reset(IEnumerable<T> newItems)
        {
            if (newItems == null) throw new ArgumentNullException(nameof(newItems));
            if (Items is not List<T> items) throw new InvalidCastException("ObservableCollection.Items is not List<T>");

            items.Clear();
            items.AddRange(newItems);

            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
            OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (Items is not List<T> list) throw new NotSupportedException();

            return list.LastIndexOf(item, index, count);
        }

        public int LastIndexOf(T item)
        {
            var size = Items.Count;
            if (size == 0)
            {
                return -1;
            }
            else
            {
                return LastIndexOf(item, size - 1, size);
            }
        }


        internal static class EventArgsCache
        {
            internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
            internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
            internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        }
    }
}
