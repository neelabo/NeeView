using NeeView.Windows.Property;

namespace NeeView
{
    public class FocusMainViewCommandParameter : CommandParameter
    {
        private bool _needClosePanels;
        private bool _isToggle = true;

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
