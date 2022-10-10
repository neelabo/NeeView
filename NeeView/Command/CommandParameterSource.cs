﻿using NeeView.Data;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class CommandParameterSource
    {
        private CommandParameter? _parameter;
        private readonly Type _type;


        // TODO: 型を直接指定するように
        public CommandParameterSource(CommandParameter defaultParameter)
        {
            if (defaultParameter == null) throw new ArgumentNullException(nameof(defaultParameter));

            _type = defaultParameter.GetType(); // ##
        }


        public CommandParameter? GetRaw()
        {
            return _parameter;
        }

        public CommandParameter GetDefault()
        {
            var parameter = Activator.CreateInstance(_type) as CommandParameter ?? throw new InvalidOperationException();
            return parameter;
        }

        public CommandParameter Get()
        {
            if (_parameter is null)
            {
                _parameter = GetDefault();
            }

            return _parameter;
        }

        public void Set(CommandParameter? value)
        {
            if (value != null && value.GetType() != _type)
            {
                throw new ArgumentException($"{_type} is required: not {value.GetType()}");
            }

            _parameter = value;
        }

        public bool EqualsDefault()
        {
            return GetDefault().MemberwiseEquals(_parameter);
        }

    }
}
