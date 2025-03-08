﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NeeView.Collections.ObjectModel
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
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

        public void Reset(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (Items is not List<T> items) throw new InvalidCastException("ObservableCollection.Items is not List<T>");

            items.Clear();
            items.AddRange(collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

    }
}
