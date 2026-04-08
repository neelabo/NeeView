using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class TouchConfig : BindableBase
    {
        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private TouchAction _dragAction = TouchAction.Gesture;
        [DefaultEquality] private TouchAction _holdAction = TouchAction.Drag;
        [DefaultEquality] private bool _isAngleEnabled = true;
        [DefaultEquality] private bool _isScaleEnabled = true;
        [DefaultEquality] private double _gestureMinimumDistance = 16.0;
        [DefaultEquality] private double _minimumManipulationRadius = 80.0;
        [DefaultEquality] private double _minimumManipulationDistance = 30.0;
        [DefaultEquality] private double _inertiaSensitivity = 0.6;


        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// ドラッグアクション
        [PropertyMember]
        public TouchAction DragAction
        {
            get { return _dragAction; }
            set { SetProperty(ref _dragAction, value); }
        }

        /// 長押しドラッグアクション
        [PropertyMember]
        public TouchAction HoldAction
        {
            get { return _holdAction; }
            set { SetProperty(ref _holdAction, value); }
        }

        [PropertyMember]
        public bool IsAngleEnabled
        {
            get { return _isAngleEnabled; }
            set { SetProperty(ref _isAngleEnabled, value); }
        }

        [PropertyMember]
        public bool IsScaleEnabled
        {
            get { return _isScaleEnabled; }
            set { SetProperty(ref _isScaleEnabled, value); }
        }

        [PropertyMember]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { SetProperty(ref _gestureMinimumDistance, AppMath.Round(value)); }
        }

        [PropertyMember]
        public double MinimumManipulationRadius
        {
            get { return _minimumManipulationRadius; }
            set { SetProperty(ref _minimumManipulationRadius, AppMath.Round(value)); }
        }

        [PropertyMember]
        public double MinimumManipulationDistance
        {
            get { return _minimumManipulationDistance; }
            set { SetProperty(ref _minimumManipulationDistance, AppMath.Round(value)); }
        }

        [PropertyPercent]
        public double InertiaSensitivity
        {
            get { return _inertiaSensitivity; }
            set { SetProperty(ref _inertiaSensitivity, AppMath.Round(value)); }
        }
    }

}
