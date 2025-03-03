//#define LOCAL_DEBUG

using NeeLaboratory;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace NeeView.Collections
{
    /// <summary>
    /// 履歴。容量制限有り
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [LocalDebug]
    public partial class HistoryLimitedCollection<T>
    {
        private readonly T?[] _buffer;
        private readonly int _bufferCapacity;
        private int _bufferTop;
        private int _bufferSize;

        /// <summary>
        /// 現在履歴位置。0で先頭
        /// </summary>
        private int _current;


        public HistoryLimitedCollection(int capacity)
        {
            _bufferCapacity = capacity;
            _buffer = new T?[capacity];
        }


        public event EventHandler? Changed;


        private int GetRawIndex(int index)
        {
            return (_bufferTop + index) % _bufferCapacity;
        }

        private void Set(int index, T? value)
        {
            Debug.Assert(index >= 0 && index < _bufferSize);

            var rawIndex = GetRawIndex(index);
            _buffer[rawIndex] = value;
        }

        private T? Get(int index)
        {
            if (index < 0 || index >= _bufferSize)
            {
                return default;
            }

            var rawIndex = GetRawIndex(index);
            return _buffer[rawIndex];
        }


        public void Add(T? element)
        {
            if (_current != _bufferSize)
            {
                _bufferSize = _current;
            }

            _bufferSize++;
            if (_bufferSize > _bufferCapacity)
            {
                _bufferSize--;
                _bufferTop = (_bufferTop + 1) % _bufferCapacity;
            }

            Set(_bufferSize - 1, element);
            _current = _bufferSize;
            LocalDebug.WriteLine($"Add: {element}: {GetInfoString()}");
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void TrimEnd(T? element)
        {
            while (_bufferSize > 0 && EqualityComparer<T>.Default.Equals(Get(_bufferSize - 1), element))
            {
                _bufferSize--;
            }
            if (_current > _bufferSize)
            {
                _current = _bufferSize;
                LocalDebug.WriteLine($"TrimEnd: {GetInfoString()}");
            }
        }

        public void Move(int delta)
        {
            _current = MathUtility.Clamp(_current + delta, 0, _bufferSize);
            LocalDebug.WriteLine($"Move: Delta={delta}: {GetInfoString()}");
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public T? GetCurrent()
        {
            return Get(_current - 1);
        }

        public bool CanPrevious()
        {
            return _current - 2 >= 0;
        }

        public T? GetPrevious()
        {
            return Get(_current - 2);
        }

        public bool CanNext()
        {
            return _current < _bufferSize;
        }

        public T? GetNext()
        {
            return Get(_current);
        }

        public T? GetHistory(int index)
        {
            return Get(index);
        }

        public void SetCurrent(int index)
        {
            _current = MathUtility.Clamp(index, 0, _bufferSize);
            LocalDebug.WriteLine($"SetCurrent: Index={index}: {GetInfoString()}");
            Changed?.Invoke(this, EventArgs.Empty);
        }

        internal List<KeyValuePair<int, T>> GetHistory(int direction, int size)
        {
            return direction < 0 ? GetPreviousHistory(size) : GetNextHistory(size);
        }

        internal List<KeyValuePair<int, T>> GetPreviousHistory(int size)
        {
            var list = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < size; ++i)
            {
                var index = _current - 2 - i;
                if (index < 0) break;
                var item = Get(index);
                if (item is null) continue;
                list.Add(new KeyValuePair<int, T>(index, item));
            }
            return list;
        }

        internal List<KeyValuePair<int, T>> GetNextHistory(int size)
        {
            var list = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < size; ++i)
            {
                var index = _current + i;
                if (index >= _bufferSize) break;
                var item = Get(index);
                if (item is null) continue;
                list.Add(new KeyValuePair<int, T>(index, item));
            }
            return list;
        }

        private string GetInfoString()
        {
            return $"Top={_bufferTop}, Size={_bufferSize}, Current={_current}";
        }

    }
}
