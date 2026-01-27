using NeeView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.Collection
{
    /// <summary>
    /// キーの重複を許容するDictionary。高速検索用
    /// </summary>
    public class MultiMap<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, List<TValue>> _map = new();


        public MultiMap()
        {
        }


        public Dictionary<TKey, List<TValue>>.KeyCollection Keys
        {
            get { return _map.Keys; }
        }

        public int Count
        {
            get { return _map.Select(e => e.Value.Count).Sum(); }
        }


        public List<TValue> this[TKey key]
        {
            get { return _map[key]; }
        }


        public void Add(TKey key, TValue value)
        {
            if (_map.ContainsKey(key))
            {
                _map[key].Add(value);
            }
            else
            {
                var list = new List<TValue>() { value };
                _map.Add(key, list);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var list))
            {
                if (list.Remove(value))
                {
                    if (list.Count == 0)
                    {
                        _map.Remove(key);
                    }
                    return true;
                }
            }

            return false;
        }

        public bool RemoveKey(TKey key)
        {
            return _map.Remove(key);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var list))
            {
                return list.Contains(value);
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            if (_map.TryGetValue(key, out var list))
            {
                // NOTE: 複数ある場合は先頭の要素のみ返す
                value = list.FirstOrDefault();
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValueCollection(TKey key, out List<TValue>? list)
        {
            return _map.TryGetValue(key, out list);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var pair in _map)
            {
                foreach (var value in pair.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(pair.Key, value);
                }
            }
        }
    }



    public static class MultiMapTools
    {
        public static MultiMap<TKey, TElement> ToMultiMap<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            where TKey : notnull
        {
            var multiMap = new MultiMap<TKey, TElement>();
            foreach (TSource element in source)
            {
                multiMap.Add(keySelector(element), elementSelector(element));
            }
            return multiMap;
        }
    }

}
