using NeeLaboratory.Generators;
using System;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class LoupeContext : INotifyPropertyChanged
    {
        private readonly MainViewComponent _mainViewComponent;
        private LoupeConfig _loupeConfig;
        private bool _isEnabled;
        private double _scale = 1.0;

        public LoupeContext(MainViewComponent mainViewComponent, LoupeConfig loupeConfig)
        {
            _mainViewComponent = mainViewComponent;
            _loupeConfig = loupeConfig;
            _scale = _loupeConfig.DefaultScale;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public double Scale
        {
            get { return _scale; }
            set { SetProperty(ref _scale, value); }
        }

        public double FixedScale
        {
            get { return _loupeConfig.IsBaseOnOriginal ? _scale / _mainViewComponent.GetScaleBaseOnOriginal() : _scale; }
        }


        public void ZoomIn()
        {
            Scale = Math.Min(Scale + _loupeConfig.ScaleStep, _loupeConfig.MaximumScale);
        }

        public void ZoomOut()
        {
            Scale = Math.Max(Scale - _loupeConfig.ScaleStep, _loupeConfig.MinimumScale);
        }

        public void Reset()
        {
            Scale = _loupeConfig.DefaultScale;
        }
    }

}
