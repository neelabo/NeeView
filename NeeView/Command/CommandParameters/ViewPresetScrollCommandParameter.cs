using Generator.Equals;
using NeeView.Windows;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class ViewPresetScrollCommandParameter : CommandParameter
    {
        [DefaultEquality] private LimitedHorizontalAlignment _horizontal = LimitedHorizontalAlignment.Center;
        [DefaultEquality] private LimitedVerticalAlignment _vertical = LimitedVerticalAlignment.Center;
        [DefaultEquality] private bool _isSnap;

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
