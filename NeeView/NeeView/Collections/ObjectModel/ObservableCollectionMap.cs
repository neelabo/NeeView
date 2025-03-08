//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace NeeView.Collections.ObjectModel
{
    /// <summary>
    /// ObservableCollection の索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [LocalDebug]
    public partial class ObservableCollectionMap<T>
        where T : class, IHasKey<string>
    {
        private readonly ObservableCollection<T> _source;
        private Dictionary<string, T> _map;


        public ObservableCollectionMap(ObservableCollection<T> source)
        {
            _source = source;
            _map = source.ToDictionary(e => e.Key, e => e);
            _source.CollectionChanged += Source_CollectionChanged;
        }


        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            return _map.TryGetValue(key, out value);
        }

        public T? Find(string key)
        {
            return _map.TryGetValue(key, out var value) ? value : null;
        }

        public void Remap(string key, T item)
        {
            if (key == item.Key) return;

            LocalDebug.WriteLine($"Remap: {key} -> {item.Key}");
            _map.Remove(key);
            _map.Add(item.Key, item);
        }

        private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LocalDebug.WriteLine($"Action={e.Action}");

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems?.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems?.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    Remove(e.OldItems?.Cast<T>());
                    Add(e.NewItems?.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void Reset()
        {
            LocalDebug.WriteLine($"Reset {_source.Count}");
            _map = _source.ToDictionary(e => e.Key, e => e);
        }

        private void Add(IEnumerable<T>? items)
        {
            if (items is null) return;

            foreach (var item in items)
            {
                LocalDebug.WriteLine($"Add={item.Key}");
                _map.Add(item.Key, item);
            }
        }

        private void Remove(IEnumerable<T>? items)
        {
            if (items is null) return;

            foreach (var item in items)
            {
                Debug.Assert(_map.TryGetValue(item.Key, out var value) && value == item);
                LocalDebug.WriteLine($"Remove={item.Key}");
                _map.Remove(item.Key);
            }
        }
    }
}
