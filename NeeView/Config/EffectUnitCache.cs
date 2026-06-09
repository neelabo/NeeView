using Generator.Equals;
using NeeView.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class EffectUnitCache : ICollection<EffectUnit>
    {
        [OrderedEquality] public Dictionary<Type, EffectUnit> _caches = new();

        public int Count => _caches.Count;

        public bool IsReadOnly => false;

        public void Add(EffectUnit? unit)
        {
            if (unit is null) return;

            var type = unit.GetType();

            var def = EffectUnit.CreateInstance(type);
            if (unit == def)
            {
                _caches.Remove(type);
            }
            else
            {
                _caches[type] = unit;
            }
        }

        public void Clear()
        {
            _caches.Clear();
        }

        public bool Contains(EffectUnit item)
        {
            return _caches.Values.Contains(item);
        }

        public void CopyTo(EffectUnit[] array, int arrayIndex)
        {
            foreach(var unit in array)
            {
                Add(unit);
            }
        }

        public EffectUnit? Get(Type? type)
        {
            if (type is null) return null;

            if (!typeof(EffectUnit).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{nameof(type)} is not {nameof(EffectUnit)}");
            }

            if (_caches.TryGetValue(type, out var unit))
            {
                return unit;
            }
            else
            {
                var def = EffectUnit.CreateInstance(type);
                return def;
            }
        }

        public IEnumerator<EffectUnit> GetEnumerator()
        {
            return _caches.Values.GetEnumerator();
        }

        public bool Remove(EffectUnit item)
        {
            var type = item.GetType();
            if (_caches.TryGetValue(type, out var unit))
            {
                if (unit == item)
                {
                    _caches.Remove(type);
                    return true;
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
