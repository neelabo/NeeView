using System;

namespace NeeView.ComponentModel
{
    public interface IValueProxy<T>
    {
        T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        T GetValue();
        void SetValue(T value);
    }


    public class ValueProxy<T> : IValueProxy<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;
        public ValueProxy(Func<T> getter, Action<T> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public T GetValue() => _getter();
        public void SetValue(T value) => _setter(value);
    }


    public class DefaultValueProxy<T> : IValueProxy<T>
    {
        private T _value;

        public DefaultValueProxy(T init)
        {
            _value = init;
        }

        public T GetValue() => _value;
        public void SetValue(T value) => _value = value;
    }

}
