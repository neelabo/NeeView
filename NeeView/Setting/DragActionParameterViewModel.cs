using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView.Setting
{
    public class DragActionParameterViewModel : BindableBase
    {
        private readonly DragActionParameter? _defaultParameter;
        private PropertyDocument? _propertyDocument;


        public DragActionParameterViewModel(DragActionCollection commandMap, string key)
        {
            var parameter = commandMap[key].Parameter;
            if (parameter is null)
            {
                return;
            }

            _defaultParameter = (DragActionParameter?)Activator.CreateInstance(parameter.GetType());

            _propertyDocument = new PropertyDocument(parameter);
        }


        public PropertyDocument? PropertyDocument
        {
            get { return _propertyDocument; }
            set { if (_propertyDocument != value) { _propertyDocument = value; RaisePropertyChanged(); } }
        }

        public string? Note { get; private set; }


        public void Reset()
        {
            if (_propertyDocument is null) return;
            if (_defaultParameter is null) return;

            _propertyDocument.Set(_defaultParameter);
            RaisePropertyChanged(nameof(PropertyDocument));
        }
    }
}
