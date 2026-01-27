using NeeLaboratory.Generators;
using System;

namespace NeeView
{
    public partial class CommandParameterSource
    {
        private CommandParameter? _parameter;
        private readonly Type _type;
        private readonly ICommandParameterDecorator? _defaultParameterDecorator;

        // TODO: 型を直接指定するように
        public CommandParameterSource(CommandParameter defaultParameter) : this(defaultParameter, null)
        {
        }

        public CommandParameterSource(CommandParameter defaultParameter, ICommandParameterDecorator? defaultParameterDecorator)
        {
            if (defaultParameter == null) throw new ArgumentNullException(nameof(defaultParameter));

            _type = defaultParameter.GetType();
            _defaultParameterDecorator = defaultParameterDecorator;
        }


        [Subscribable]
        public event EventHandler<ParameterChangedEventArgs>? ParameterChanged;


        public CommandParameter? GetRaw()
        {
            return _parameter;
        }

        public CommandParameter GetDefault()
        {
            var parameter = Activator.CreateInstance(_type) as CommandParameter ?? throw new InvalidOperationException();
            _defaultParameterDecorator?.DecorateCommandParameter(parameter);
            return parameter;
        }

        public CommandParameter Get()
        {
            if (_parameter is null)
            {
                _parameter = GetDefault();
                _parameter.PropertyChanged += Parameter_PropertyChanged;
            }

            return _parameter;
        }

        public void Set(CommandParameter? value)
        {
            if (value != null && value.GetType() != _type)
            {
                throw new ArgumentException($"{_type} is required: not {value.GetType()}");
            }

            if (_parameter != value)
            {
                if (_parameter != null)
                {
                    _parameter.PropertyChanged -= Parameter_PropertyChanged;
                }

                _parameter = value;

                if (_parameter != null)
                {
                    _parameter.PropertyChanged += Parameter_PropertyChanged;
                }

                ParameterChanged?.Invoke(this, new ParameterChangedEventArgs(null));
            }
        }

        private void Parameter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ParameterChanged?.Invoke(this, new ParameterChangedEventArgs(e.PropertyName));
        }

        public bool EqualsDefault()
        {
            return GetDefault().MemberwiseEquals(_parameter);
        }

    }


    public class ParameterChangedEventArgs : EventArgs
    {
        public ParameterChangedEventArgs(string? propertyName)
        {
            PropertyName = propertyName;
        }

        public string? PropertyName { get; }
    }

}
