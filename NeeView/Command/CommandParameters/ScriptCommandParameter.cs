using NeeView.Windows.Property;

namespace NeeView
{
    public class ScriptCommandParameter : CommandParameter
    {
        private string? _argument;
        private bool _isChecked;

        [PropertyMember]
        public string? Argument
        {
            get { return _argument; }
            set { SetProperty(ref _argument, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        [PropertyMember]
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }

}
