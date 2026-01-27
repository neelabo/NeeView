using System;
using System.Diagnostics;

namespace NeeView
{
    public class DragActionParameterSource
    {
        private DragActionParameter? _parameter;
        private readonly Type _type;


        public DragActionParameterSource(Type defaultType)
        {
            _type = defaultType ?? throw new ArgumentNullException(nameof(defaultType));
        }


        public DragActionParameter? GetRaw()
        {
            return _parameter;
        }

        public DragActionParameter GetDefault()
        {
            if (Activator.CreateInstance(_type) is not DragActionParameter instance) throw new InvalidOperationException();
            return instance;
        }

        public DragActionParameter Get()
        {
            if (_parameter is null)
            {
                _parameter = GetDefault();
            }

            return _parameter;
        }

        public void Set(DragActionParameter? value)
        {
            if (value != null && value.GetType() != _type)
            {
                Debug.Assert(false, $"{_type} is required: not {value.GetType()}");
                return;
            }

            _parameter = value;
        }

        public bool EqualsDefault()
        {
            return GetDefault().MemberwiseEquals(_parameter);
        }
    }
}
