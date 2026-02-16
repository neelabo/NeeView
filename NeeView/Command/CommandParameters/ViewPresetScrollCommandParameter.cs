using NeeView.Windows;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ViewPresetScrollCommandParameter : CommandParameter
    {
        private LimitedHorizontalAlignment _horizontal = LimitedHorizontalAlignment.Center;
        private LimitedVerticalAlignment _vertical = LimitedVerticalAlignment.Center;
        private bool _isSnap;

        [PropertyMember]
        public LimitedHorizontalAlignment Horizontal
        {
            get { return _horizontal; }
            set { SetProperty(ref _horizontal, value); }
        }

        [PropertyMember]
        public LimitedVerticalAlignment Vertical
        {
            get { return _vertical; }
            set { SetProperty(ref _vertical, value); }
        }

        [PropertyMember]
        public bool IsSnap
        {
            get { return _isSnap; }
            set { SetProperty(ref _isSnap, value); }
        }
    }
}
