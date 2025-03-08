using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView.Collections.Generic
{
    /// <summary>
    /// LinkedList + Dictionary
    /// 検索を高速化したLinkedList
    /// </summary>
    /// <typeparam name="TKey">検索用キー</typeparam>
    /// <typeparam name="TValue">収納型</typeparam>
    public class LinkedDictionary<TKey, TValue> : IEnumerable<TValue>
        where TKey : notnull
        where TValue : IHasKey<TKey>
    {
        private readonly Dictionary<TKey, LinkedListNode<TValue>> _map = new();
        private readonly LinkedList<TValue> _list = new();

        public int Count => _map.Count;
        public LinkedListNode<TValue>? First => _list.First;
        public LinkedListNode<TValue>? Last => _list.Last;


        public int IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }

        public void AddFirst(TValue item)
        {
            AddFirst(item.Key, item);
        }

        public void AddFirst(TKey key, TValue item)
        {
            Debug.Assert(EqualityComparer<TKey>.Default.Equals(key, item.Key));

            var node = Find(key);
            if (node == null)
            {
                node = new LinkedListNode<TValue>(item);
                _map.Add(key, node);
                _list.AddFirst(node);
            }
            else
            {
                node.Value = item;
                _list.Remove(node);
                _list.AddFirst(node);
            }
        }

        public void AddLast(TValue item)
        {
            AddLast(item.Key, item);
        }

        public void AddLast(TKey key, TValue item)
        {
            Debug.Assert(EqualityComparer<TKey>.Default.Equals(key, item.Key));

            var node = Find(key);
            if (node == null)
            {
                node = new LinkedListNode<TValue>(item);
                _map.Add(key, node);
                _list.AddLast(node);
            }
            else
            {
                node.Value = item;
                _list.Remove(node);
                _list.AddLast(node);
            }
        }

        // no check version
        public void AddLastRaw(TKey key, TValue item)
        {
            Debug.Assert(EqualityComparer<TKey>.Default.Equals(key, item.Key));

            var node = new LinkedListNode<TValue>(item);
            _map.Add(key, node);
            _list.AddLast(node);
        }

        public LinkedListNode<TValue>? Find(TKey key)
        {
            _map.TryGetValue(key, out var node);
            return node;
        }

        public void MoveToFirst(LinkedListNode<TValue> node)
        {
            Debug.Assert(node.List == _list);
            _list.Remove(node);
            _list.AddFirst(node);
        }

        public void MoveToLast(LinkedListNode<TValue> node)
        {
            Debug.Assert(node.List == _list);
            _list.Remove(node);
            _list.AddLast(node);
        }

        public TValue? Remove(TKey key)
        {
            if (key == null) return default;

            var node = Find(key);
            if (node != null)
            {
                _list.Remove(node);
                _map.Remove(key);
                return node.Value;
            }
            else
            {
                return default;
            }
        }

        public TValue? Remove(LinkedListNode<TValue> node)
        {
            _list.Remove(node);
            _map.Remove(node.Value.Key);
            return node.Value;
        }

        public bool Remap(TKey src, TKey dst)
        {
            var node = Find(src);
            if (node != null)
            {
                _map.Remove(src);
                _map.Add(dst, node);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            _list.Clear();
            _map.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        #region IEnumerable support

        public IEnumerator<TValue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
