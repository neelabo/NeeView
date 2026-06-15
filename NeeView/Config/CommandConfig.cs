using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class CommandConfig : ObservableObject
    {
        [DefaultEquality] private InputScheme _presetInputScheme;
        [DefaultEquality] private PageReadOrder _presetPageReadOrder;
        [DefaultEquality] private bool _isAccessKeyEnabled = true;
        [DefaultEquality] private bool _isReversePageMove = true;
        [DefaultEquality] private bool _isReversePageMoveWheel;
        [DefaultEquality] private bool _isReversePageMoveHorizontalWheel = true;
        [DefaultEquality] private bool _isHorizontalWheelLimitedOnce = true;


        [PropertyMapIgnore]
        public InputScheme PresetInputScheme
        {
            get { return _presetInputScheme; }
            set { SetProperty(ref _presetInputScheme, value); }
        }

        [PropertyMapIgnore]
        public PageReadOrder PresetPageReadOrder
        {
            get { return _presetPageReadOrder; }
            set { SetProperty(ref _presetPageReadOrder, value); }
        }

        [PropertyMember]
        public bool IsAccessKeyEnabled
        {
            get { return _isAccessKeyEnabled; }
            set { SetProperty(ref _isAccessKeyEnabled, value); }
        }

        [PropertyMember]
        public bool IsReversePageMove
        {
            get { return _isReversePageMove; }
            set { SetProperty(ref _isReversePageMove, value); }
        }

        [PropertyMember]
        public bool IsReversePageMoveWheel
        {
            get { return _isReversePageMoveWheel; }
            set { SetProperty(ref _isReversePageMoveWheel, value); }
        }

        [PropertyMember]
        public bool IsReversePageMoveHorizontalWheel
        {
            get { return _isReversePageMoveHorizontalWheel; }
            set { SetProperty(ref _isReversePageMoveHorizontalWheel, value); }
        }

        [PropertyMember]
        public bool IsHorizontalWheelLimitedOnce
        {
            get { return _isHorizontalWheelLimitedOnce; }
            set { SetProperty(ref _isHorizontalWheelLimitedOnce, value); }
        }
    }
}
