using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public class ExternalAppEditDialogViewModel : BindableBase
    {
        private ExternalApp _model;

        public ExternalAppEditDialogViewModel(ExternalApp model)
        {
            _model = model;
        }

        public ExternalApp Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }


        public string? Name
        {
            get => _model.Name ?? _model.DisplayName;
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? null : value;
                if (_model.Name != value)
                {
                    _model.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string? Command
        {
            get { return _model.Command; }
            set { if (_model.Command != value)
                {
                    _model.Command = value;
                    RaisePropertyChanged();

                    if (_model.Name == null)
                    {
                        _model.Name = _model.DisplayName;
                        RaisePropertyChanged(nameof(Name));
                    }
                }
            }
        }


        public Dictionary<ArchivePolicy, string> ArchivePolicyList => AliasNameExtensions.GetAliasNameDictionary<ArchivePolicy>();
    }
}
