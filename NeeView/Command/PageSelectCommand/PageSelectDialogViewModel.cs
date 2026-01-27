using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Globalization;

namespace NeeView
{
    public class PageSelectDialogDecidedEventArgs : EventArgs
    {
        public PageSelectDialogDecidedEventArgs(bool result)
        {
            Result = result;
        }

        public bool Result { get; set; }
    }

    public class PageSelectDialogViewModel : BindableBase
    {
        private readonly PageSelectDialogModel _model;
        private RelayCommand? _decideCommand;
        private RelayCommand? _cancelCommand;

        public PageSelectDialogViewModel(PageSelectDialogModel model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.Value),
                (s, e) => RaisePropertyChanged(nameof(Value)));
        }


        public event EventHandler<PageSelectDialogDecidedEventArgs>? Decided;


        public string Caption => TextResources.GetString("JumpPageCommand");

        public string Label => string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.JumpPageLabel"), _model.Min, _model.Max);

        public int Value
        {
            get { return _model.Value; }
            set { _model.Value = value; }
        }


        public RelayCommand DecideCommand
        {
            get
            {
                return _decideCommand = _decideCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    Decided?.Invoke(this, new PageSelectDialogDecidedEventArgs(true));
                }
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand = _cancelCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    Decided?.Invoke(this, new PageSelectDialogDecidedEventArgs(false));
                }
            }
        }


        public void AddValue(int delta)
        {
            _model.AddValue(delta);
        }
    }
}
