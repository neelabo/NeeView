using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class SlideShowConfig : ObservableObject
    {
        private static readonly string _defaultNextPageCommandName = CommandElementTools.CreateCommandName(nameof(NextPageCommand));

        [DefaultEquality] private string _nextPageCommandName = _defaultNextPageCommandName;
        [DefaultEquality] private double _slideShowInterval = 5.0;
        [DefaultEquality] private bool _isCancelSlideByMouseMove;
        [DefaultEquality] private PageEndAction _pageEndAction = PageEndAction.Loop;
        [DefaultEquality] private bool _isTimerVisible;
        [DefaultEquality] private bool _isPrioritizeTime;
        [DefaultEquality] private bool _isWaitAnimation;
        [DefaultEquality] private bool _isAutoScroll = true;

        /// <summary>
        /// ページ送りコマンド名
        /// </summary>
        [PropertyStrings]
        public string NextPageCommandName
        {
            get { return _nextPageCommandName; }
            set { SetProperty(ref _nextPageCommandName, value); }
        }

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
        /// ページ終端でのアクション
        /// </summary>
        [PropertyMember]
        public PageEndAction PageEndAction
        {
            get { return _pageEndAction; }
            set { SetProperty(ref _pageEndAction, value); }
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

        #region Obsolete

        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        [Obsolete, Alternative(nameof(PageEndAction), 46, ScriptErrorLevel.Warning)] // v46.0
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public bool IsSlideShowByLoop
        {
            get { return PageEndAction == PageEndAction.Loop; }
            set { PageEndAction = value ? PageEndAction.Loop : PageEndAction.None; }
        }

        #endregion Obsolete

        public void ResetNextPageCommandName()
        {
            NextPageCommandName = _defaultNextPageCommandName;
        }
    }
}
