using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.Collection
{
    /// <summary>
    /// 重複したIDを上書きするQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniqueQueue<T> : IEnumerable<UniqueQueue<T>.Unit>
    {
        public record Unit(int Id, T Value);

        private readonly List<Unit> _queue = new();

        public Unit Enqueue(int id, T value)
        {
            var unit = new Unit(id, value);
            return Enqueue(unit);
        }

        public Unit Enqueue(Unit unit)
        {
            // The same ID will be overwritten.
            Remove(unit.Id);
            _queue.Add(unit);
            return unit;
        }

        public Unit? Dequeue()
        {
            var unit = _queue.FirstOrDefault();
            if (unit is not null)
            {
                _queue.Remove(unit);
            }
            return unit;
        }

        public Unit? Peek()
        {
            return _queue.FirstOrDefault();
        }

        public Unit? Remove(int id)
        {
            var unit = _queue.FirstOrDefault(e => e.Id == id);
            if (unit is not null)
            {
                _queue.Remove(unit);
            }
            return unit;
        }

        public IEnumerator<Unit> GetEnumerator()
        {
            return ((IEnumerable<Unit>)_queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_queue).GetEnumerator();
        }
    }
}
