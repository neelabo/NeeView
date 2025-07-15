using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ScriptUnitPool
    {
        private readonly List<ScriptUnit> _units = new();
        private readonly System.Threading.Lock _lock = new();

        public ScriptUnit Run(object? sender, string script, string? name, List<object>? args)
        {
            var unit = new ScriptUnit(this);
            Add(unit);
            unit.Execute(sender, script, name, args);
            return unit;
        }

        public void Add(ScriptUnit unit)
        {
            if (unit is null) throw new ArgumentNullException(nameof(unit));

            lock (_lock)
            {
                _units.Add(unit);
            }
        }

        public void Remove(ScriptUnit unit)
        {
            lock (_lock)
            {
                _units.Remove(unit);
            }
        }

        public void CancelAll()
        {
            lock (_lock)
            {
                foreach (var item in _units)
                {
                    item.Cancel();
                }
            }
        }

    }
}
