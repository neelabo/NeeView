using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class SlideShowConfig : ObservableObject
    {
        [DefaultEquality] private double _slideShowInterval = 5.0;
        [DefaultEquality] private bool _isCancelSlideByMouseMove = true;
        [DefaultEquality] private bool _isSlideShowByLoop = true;
        [DefaultEquality] private bool _isTimerVisible;
        [DefaultEquality] private bool _isPrioritizeTime;
        [DefaultEquality] private bool _isWaitAnimation;
        [DefaultEquality] private bool _isAutoScroll = true;

        /// <summary>
        /// スライドショーの表示間隔(秒)
        /// </summary>
        [PropertyMember(HasDecimalPoint = true)]
        public double SlideShowInterval
        {
            get { return _slideShowInterval; }
            set { SetProperty(ref _slideShowInterval, AppMath.Round(Math.Max(value, 0.1))); }
        }

        /// <summary>
        /// カーソルでスライドを止める.
        /// </summary>
        [PropertyMember]
        public bool IsCancelSlideByMouseMove
        {
            get { return _isCancelSlideByMouseMove; }
            set { SetProperty(ref _isCancelSlideByMouseMove, value); }
        }

        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        [PropertyMember]
        public bool IsSlideShowByLoop
        {
            get { return _isSlideShowByLoop; }
            set { SetProperty(ref _isSlideShowByLoop, value); }
        }

        /// <summary>
        /// 時間表示
        /// </summary>
        [PropertyMember]
        public bool IsTimerVisible
        {
            get { return _isTimerVisible; }
            set { SetProperty(ref _isTimerVisible, value); }
        }

        /// <summary>
        /// 時間を優先
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizeTime
        {
            get { return _isPrioritizeTime; }
            set { SetProperty(ref _isPrioritizeTime, value); }
        }

        /// <summary>
        /// アニメーション完了を待機する
        /// </summary>
        [PropertyMember]
        public bool IsWaitAnimation
        {
            get { return _isWaitAnimation; }
            set { SetProperty(ref _isWaitAnimation, value); }
        }

        /// <summary>
        /// 自動スクロール
        /// </summary>
        [PropertyMember]
        public bool IsAutoScroll
        {
            get { return _isAutoScroll; }
            set { SetProperty(ref _isAutoScroll, value); }
        }
    }
}
