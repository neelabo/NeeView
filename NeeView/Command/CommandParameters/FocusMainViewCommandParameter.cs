using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class FocusMainViewCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _needClosePanels;
        [DefaultEquality] private bool _isToggle = true;

        [PropertyMember]
        public bool NeedClosePanels
        {
            get => _needClosePanels;
            set => SetProperty(ref _needClosePanels, value);
        }

        [PropertyMember]
        public bool IsToggle
        {
            get { return _isToggle; }
            set { SetProperty(ref _isToggle, value); }
        }
    }

}
