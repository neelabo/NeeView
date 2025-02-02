using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
    public class MouseConfig : BindableBase
    {
        private bool _isGestureEnabled = true;
        private bool _isDragEnabled = true;
        private double _gestureMinimumDistance = 30.0;
        private LongButtonDownMode _longButtonDownMode = LongButtonDownMode.Loupe;
        private bool _isCursorHideEnabled = true;
        private double _cursorHideTime = 2.0;
        private double _minimumDragDistance = 5.0;
        private LongButtonMask _longButtonMask;
        private double _longButtonDownTime = 1.0;
        private double _longButtonRepeatTime = 0.1;
        private bool _isCursorHideReleaseAction = true;
        private double _cursorHideReleaseDistance = 5.0;
        private bool _isHoverScroll;
        private double _hoverScrollDuration = 0.5;
        private double _inertiaSensitivity = 0.5;
        private double _autoScrollSensitivity = 1.0;


        // マウスジェスチャー有効
        [PropertyMember]
        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        // マウスドラッグ有効
        [PropertyMember]
        public bool IsDragEnabled
        {
            get { return _isDragEnabled; }
            set { SetProperty(ref _isDragEnabled, value); }
        }

        // ドラッグ開始距離
        [PropertyRange(1.0, 200.0, TickFrequency = 1.0, IsEditable = true)]
        public double MinimumDragDistance
        {
            get { return _minimumDragDistance; }
            set { SetProperty(ref _minimumDragDistance, value); }
        }

        [PropertyRange(5.0, 200.0, TickFrequency = 1.0, IsEditable = true)]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { SetProperty(ref _gestureMinimumDistance, Math.Max(value, SystemParameters.MinimumHorizontalDragDistance)); }
        }

        [PropertyMember]
        public LongButtonDownMode LongButtonDownMode
        {
            get { return _longButtonDownMode; }
            set { SetProperty(ref _longButtonDownMode, value); }
        }

        [PropertyMember]
        public LongButtonMask LongButtonMask
        {
            get { return _longButtonMask; }
            set { SetProperty(ref _longButtonMask, value); }
        }

        [PropertyRange(0.1, 2.0, TickFrequency = 0.1, HasDecimalPoint = true)]
        public double LongButtonDownTime
        {
            get { return _longButtonDownTime; }
            set { SetProperty(ref _longButtonDownTime, value); }
        }

        [PropertyRange(0.01, 1.0, TickFrequency = 0.01, HasDecimalPoint = true)]
        public double LongButtonRepeatTime
        {
            get { return _longButtonRepeatTime; }
            set { SetProperty(ref _longButtonRepeatTime, value); }
        }

        /// <summary>
        /// カーソルの自動非表示
        /// </summary>
        [PropertyMember]
        public bool IsCursorHideEnabled
        {
            get { return _isCursorHideEnabled; }
            set { SetProperty(ref _isCursorHideEnabled, value); }
        }

        [PropertyRange(1.0, 10.0, TickFrequency = 0.2, IsEditable = true, HasDecimalPoint = true)]
        public double CursorHideTime
        {
            get { return _cursorHideTime; }
            set { SetProperty(ref _cursorHideTime, Math.Max(1.0, value)); }
        }

        [PropertyMember]
        public bool IsCursorHideReleaseAction
        {
            get { return _isCursorHideReleaseAction; }
            set { SetProperty(ref _isCursorHideReleaseAction, value); }
        }

        [PropertyRange(0.0, 1000.0, TickFrequency = 1.0, IsEditable = true)]
        public double CursorHideReleaseDistance
        {
            get { return _cursorHideReleaseDistance; }
            set { SetProperty(ref _cursorHideReleaseDistance, value); }
        }

        [PropertyMember]
        public bool IsHoverScroll
        {
            get { return _isHoverScroll; }
            set { SetProperty(ref _isHoverScroll, value); }
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true, HasDecimalPoint = true)]
        public double HoverScrollDuration
        {
            get { return _hoverScrollDuration; }
            set { SetProperty(ref _hoverScrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyPercent]
        public double InertiaSensitivity
        {
            get { return _inertiaSensitivity; }
            set { SetProperty(ref _inertiaSensitivity, value); }
        }

        [PropertyRange(0.0, 2.0, TickFrequency = 0.1, IsEditable = true, HasDecimalPoint = true)]
        public double AutoScrollSensitivity
        {
            get { return _autoScrollSensitivity; }
            set { SetProperty(ref _autoScrollSensitivity, value); }
        }
    }


    /// <summary>
    /// 慣性パラメータ用
    /// </summary>
    public static class InertiaTools
    { 
        /// <summary>
        /// 慣性感度から加速度を求める。設定用。
        /// </summary>
        /// <param name="sensitivity">慣性感度(0-1)</param>
        /// <returns></returns>
        public static double GetAcceleration(double sensitivity)
        {
            // y = a * x^b + c で
            // x = [0.0 - 1.0], y = [0.001 - 1.0], x=0.5 のとき y=0.01 になるような係数
            var a = 0.999;
            var b = 6.79586;
            var c = 0.001;

            var x = MathUtility.Clamp(1.0 - sensitivity, 0.0, 1.0);
            var y = MathUtility.Clamp(a * Math.Pow(x, b) + c, c, 1.0);
            return -y;
        }

    }

}
