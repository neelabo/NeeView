using NeeLaboratory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Collections
{
    /// <summary>
    /// 履歴
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HistoryCollection<T>
    {
        private List<T> _history = new();

        /// <summary>
        /// 現在履歴位置。0で先頭
        /// </summary>
        private int _current;


        public event EventHandler? Changed;


        public void Add(T path)
        {
            if (_current != _history.Count)
            {
                _history = _history.Take(_current).ToList();
            }
            _history.Add(path);
            _current = _history.Count;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void TrimEnd(T element)
        {
            while (_history.Count > 0 && EqualityComparer<T>.Default.Equals(_history.Last(), element))
            {
                _history.RemoveAt(_history.Count - 1);
            }
            if (_current > _history.Count)
            {
                _current = _history.Count;
            }
        }

        public void Move(int delta)
        {
            _current = MathUtility.Clamp(_current + delta, 0, _history.Count);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public T? GetCurrent()
        {
            var index = _current - 1;
            return (index >= 0) ? _history[index] : default;
        }

        public bool CanPrevious()
        {
            return _current - 2 >= 0;
        }

        public T? GetPrevious()
        {
            var index = _current - 2;
            return (index >= 0) ? _history[index] : default;
        }

        public bool CanNext()
        {
            return _current < _history.Count;
        }

        public T? GetNext()
        {
            return (_current < _history.Count) ? _history[_current] : default;
        }

        //
        public T GetHistory(int index)
        {
            index = MathUtility.Clamp(index, 0, _history.Count - 1);
            return _history[index];
        }

        //
        public void SetCurrent(int index)
        {
            _current = MathUtility.Clamp(index, 0, _history.Count);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        //
        internal List<KeyValuePair<int, T>> GetHistory(int direction, int size)
        {
            return direction < 0 ? GetPrevousHistory(size) : GetNextHistory(size);
        }

        //
        internal List<KeyValuePair<int, T>> GetPrevousHistory(int size)
        {
            var list = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < size; ++i)
            {
                var index = _current - 2 - i;
                if (index < 0) break;
                list.Add(new KeyValuePair<int, T>(index, _history[index]));
            }
            return list;
        }

        //
        internal List<KeyValuePair<int, T>> GetNextHistory(int size)
        {
            var list = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < size; ++i)
            {
                var index = _current + i;
                if (index >= _history.Count) break;
                list.Add(new KeyValuePair<int, T>(index, _history[index]));
            }
            return list;
        }
    }
}
