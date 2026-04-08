using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;

namespace NeeView
{
    /// <summary>
    /// Resize filter (PhotoSauce.MagicScaler)
    /// </summary>
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ImageResizeFilterConfig : BindableBase
    {
        [DefaultEquality] private bool _isResizeFilterEnabled = false;
        [DefaultEquality] private ResizeInterpolation _resizeInterpolation = ResizeInterpolation.Lanczos;
        [DefaultEquality] private bool _isUnsharpMaskEnabled;
        [DefaultEquality] private UnsharpMaskConfig _unsharpMask;

        public ImageResizeFilterConfig()
        {
            var setting = new ProcessImageSettings(); // default values.
            _isUnsharpMaskEnabled = setting.Sharpen;

            _unsharpMask = new();
            _unsharpMask.PropertyChanged += UnsharpMask_PropertyChanged;

            this.UnsharpMask.Amount = setting.UnsharpMask.Amount;
            this.UnsharpMask.Radius = setting.UnsharpMask.Radius;
            this.UnsharpMask.Threshold = setting.UnsharpMask.Threshold;
        }

        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { SetProperty(ref _isResizeFilterEnabled, value); }
        }

        [PropertyMember]
        public ResizeInterpolation ResizeInterpolation
        {
            get { return _resizeInterpolation; }
            set { SetProperty(ref _resizeInterpolation, value); }
        }

        [PropertyMember]
        public bool IsUnsharpMaskEnabled
        {
            get { return _isUnsharpMaskEnabled; }
            set { SetProperty(ref _isUnsharpMaskEnabled, value); }
        }

        [PropertyMapLabel("Word.UnsharpMask")]
        public UnsharpMaskConfig UnsharpMask
        {
            get { return _unsharpMask; }
            set
            {
                if (_unsharpMask != value)
                {
                    _unsharpMask.PropertyChanged -= UnsharpMask_PropertyChanged;
                    _unsharpMask = value;
                    _unsharpMask.PropertyChanged += UnsharpMask_PropertyChanged;
                    RaisePropertyChanged(nameof(UnsharpMask));
                }
            }
        }
        
        private void UnsharpMask_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(UnsharpMask));
        }

        public int GetEnvironmentHashCode()
        {
            return HashCode.Combine(_isResizeFilterEnabled, _resizeInterpolation, _isUnsharpMaskEnabled, UnsharpMask.GetEnvironmentHashCode());
        }

        public ProcessImageSettings CreateProcessImageSetting()
        {
            var setting = new ProcessImageSettings();

            setting.Sharpen = this.IsUnsharpMaskEnabled;
            setting.UnsharpMask = this.UnsharpMask.CreateUnsharpMaskSetting();

            switch (_resizeInterpolation)
            {
                case ResizeInterpolation.NearestNeighbor:
                    setting.Interpolation = InterpolationSettings.NearestNeighbor;
                    break;
                case ResizeInterpolation.Average:
                    setting.Interpolation = InterpolationSettings.Average;
                    break;
                case ResizeInterpolation.Linear:
                    setting.Interpolation = InterpolationSettings.Linear;
                    break;
                case ResizeInterpolation.Quadratic:
                    setting.Interpolation = InterpolationSettings.Quadratic;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.QuadraticInterpolator(1.0));
                    break;
                case ResizeInterpolation.Hermite:
                    setting.Interpolation = InterpolationSettings.Hermite;
                    break;
                case ResizeInterpolation.Mitchell:
                    setting.Interpolation = InterpolationSettings.Mitchell;
                    break;
                case ResizeInterpolation.CatmullRom:
                    setting.Interpolation = InterpolationSettings.CatmullRom;
                    break;
                case ResizeInterpolation.Cubic:
                    setting.Interpolation = InterpolationSettings.Cubic;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.CubicInterpolator(0, 0.5));
                    break;
                case ResizeInterpolation.CubicSmoother:
                    setting.Interpolation = InterpolationSettings.CubicSmoother;
                    break;
                case ResizeInterpolation.Lanczos:
                    setting.Interpolation = InterpolationSettings.Lanczos;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.LanczosInterpolator(3));
                    break;
                case ResizeInterpolation.Spline36:
                    setting.Interpolation = InterpolationSettings.Spline36;
                    break;
            }

            return setting;
        }
    }
}
