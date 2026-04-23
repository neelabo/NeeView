using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.ComponentModel
{
    public class Messenger
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> action)
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out List<Delegate>? value))
            {
                value = new();
                _subscribers[type] = value;
            }

            value.Add(action);
        }

        public void Publish<T>(T message)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers.ToList())
                {
                    ((Action<T>)handler)(message);
                }
            }
        }
    }
}
