using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Globalization;

namespace NeeView.Setting
{
    public class CommandParameterViewModel : BindableBase
    {
        private readonly IReadOnlyDictionary<string, CommandElement> _commandMap;
        private readonly string _key;
        private readonly CommandElement _commandElement;
        private PropertyDocument? _propertyDocument;


        public CommandParameterViewModel(IReadOnlyDictionary<string, CommandElement> commandMap, string key)
        {
            _commandMap = commandMap;
            _key = key;

            _commandElement = CommandTable.Current.GetElement(_key);
            if (_commandElement.Share != null)
            {
                _key = _commandElement.Share.Name;
                this.Note = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("CommandParameter.Share"), CommandTable.Current.GetElement(_key).Text);
            }

            var defaultParameter = _commandElement.ParameterSource?.GetDefault();
            if (defaultParameter == null)
            {
                return;
            }

            var parameter = (CommandParameter?)(_commandMap[_key].Parameter ?? defaultParameter)?.Clone();
            if (parameter is not null)
            {
                _propertyDocument = new PropertyDocument(parameter);
            }
        }


        public PropertyDocument? PropertyDocument
        {
            get { return _propertyDocument; }
            set { if (_propertyDocument != value) { _propertyDocument = value; RaisePropertyChanged(); } }
        }

        public string? Note { get; private set; }


        public void Flush()
        {
            if (_propertyDocument != null)
            {
                _commandMap[_key].Parameter = (CommandParameter?)_propertyDocument.Source;
                _commandElement.UpdateDefaultParameter();
            }
        }

        public void Reset()
        {
            var defaultParameter = _commandElement.ParameterSource?.GetDefault();
            if (defaultParameter == null)
            {
                return;
            }

            if (_propertyDocument != null && defaultParameter != null)
            {
                _propertyDocument.Set(defaultParameter);
                RaisePropertyChanged(nameof(PropertyDocument));
            }
        }
    }
}
