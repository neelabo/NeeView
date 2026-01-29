using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class LoupeConfig : BindableBase
    {
        private double _defaultScale = 2.0;
        private bool _isLoupeCenter;
        private double _minimumScale = 1.0;
        private double _maximumScale = 10.0;
        private double _scaleStep = 1.0;
        private bool _isResetByRestart = false;
        private bool _isResetByPageChanged = true;
        private bool _isVisibleLoupeInfo = true;
        private bool _isWheelScalingEnabled = true;
        private double _speed = 1.0;
        private bool _isEscapeKeyEnabled = true;
        private bool _isBaseOnOriginal;


        [PropertyMember]
        public bool IsLoupeCenter
        {
            get { return _isLoupeCenter; }
            set { SetProperty(ref _isLoupeCenter, value); }
        }

        [PropertyRange(1, 20, TickFrequency = 1.0, IsEditable = true, Format = "× {0:0.0}")]
        public double MinimumScale
        {
            get { return _minimumScale; }
            set { SetProperty(ref _minimumScale, value); }
        }

        [PropertyRange(1, 20, TickFrequency = 1.0, IsEditable = true, Format = "× {0:0.0}")]
        public double MaximumScale
        {
            get { return _maximumScale; }
            set { SetProperty(ref _maximumScale, value); }
        }

        [PropertyRange(1, 20, TickFrequency = 1.0, IsEditable = true, Format = "× {0:0.0}")]
        public double DefaultScale
        {
            get { return _defaultScale; }
            set { SetProperty(ref _defaultScale, value); }
        }

        [PropertyRange(0.1, 5.0, TickFrequency = 0.1, IsEditable = true, Format = "{0:0.0}")]
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { SetProperty(ref _scaleStep, Math.Max(value, 0.0)); }
        }

        [PropertyMember]
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { SetProperty(ref _isResetByRestart, value); }
        }

        [PropertyMember]
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { SetProperty(ref _isResetByPageChanged, value); }
        }

        [PropertyMember]
        public bool IsWheelScalingEnabled
        {
            get { return _isWheelScalingEnabled; }
            set { SetProperty(ref _isWheelScalingEnabled, value); }
        }

        [PropertyRange(0.0, 10.0, TickFrequency = 0.1, Format = "× {0:0.0}")]
        public double Speed
        {
            get { return _speed; }
            set { SetProperty(ref _speed, value); }
        }

        [PropertyMember]
        public bool IsEscapeKeyEnabled
        {
            get { return _isEscapeKeyEnabled; }
            set { SetProperty(ref _isEscapeKeyEnabled, value); }
        }

        [PropertyMember]
        public bool IsVisibleLoupeInfo
        {
            get { return _isVisibleLoupeInfo; }
            set { SetProperty(ref _isVisibleLoupeInfo, value); }
        }

        [PropertyMember]
        public bool IsBaseOnOriginal
        {
            get { return _isBaseOnOriginal; }
            set { SetProperty(ref _isBaseOnOriginal, value); }
        }
    }

}
