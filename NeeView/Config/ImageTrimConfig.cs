using Generator.Equals;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class ImageTrimConfig : BindableBase
    {
        private const double _maxRate = 0.9;

        private bool _isEnabled;
        private double _top;
        private double _bottom;
        private double _left;
        private double _right;


        [PropertyMember(IsVisible = false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }


        [PropertyPercent(0.0, _maxRate)]
        public double Left
        {
            get { return _left; }
            set
            {
                if (SetProperty(ref _left, AppMath.Round(MathUtility.Clamp(value, 0.0, _maxRate))))
                {
                    if (_left + _right > _maxRate)
                    {
                        _right = _maxRate - _left;
                        RaisePropertyChanged(nameof(Right));
                    }
                }
            }
        }

        [PropertyPercent(0.0, _maxRate)]
        public double Right
        {
            get { return _right; }
            set
            {
                if (SetProperty(ref _right, AppMath.Round(MathUtility.Clamp(value, 0.0, _maxRate))))
                {
                    if (_left + _right > _maxRate)
                    {
                        _left = _maxRate - _right;
                        RaisePropertyChanged(nameof(Left));
                    }
                }
            }
        }


        [PropertyPercent(0.0, _maxRate)]
        public double Top
        {
            get { return _top; }
            set
            {
                if (SetProperty(ref _top, AppMath.Round(MathUtility.Clamp(value, 0.0, _maxRate))))
                {
                    if (_top + _bottom > _maxRate)
                    {
                        _bottom = _maxRate - _top;
                        RaisePropertyChanged(nameof(Bottom));
                    }
                }
            }
        }

        [PropertyPercent(0.0, _maxRate)]
        public double Bottom
        {
            get { return _bottom; }
            set
            {
                if (SetProperty(ref _bottom, AppMath.Round(MathUtility.Clamp(value, 0.0, _maxRate))))
                {
                    if (_top + _bottom > _maxRate)
                    {
                        _top = _maxRate - _bottom;
                        RaisePropertyChanged(nameof(Top));
                    }
                }
            }
        }


    }
}
