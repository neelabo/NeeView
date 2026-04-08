using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class MainViewConfig : BindableBase
    {
        [DefaultEquality] private bool _isFloating;
        [DefaultEquality] private bool _isTopmost;
        [DefaultEquality] private bool _isFrontAsPossible;
        [DefaultEquality] private bool _isHideTitleBar;
        [DefaultEquality] private bool _isAutoStretch;
        [DefaultEquality] private bool _isAutoHide = true;
        [DefaultEquality] private bool _isAutoShow = true;
        [DefaultEquality] private Size _referenceSize;
        [DefaultEquality] private AlternativeContent _alternativeContent = AlternativeContent.PageList;
        [DefaultEquality] private bool _isFloatingEndWhenClosed;


        /// <summary>
        /// メインビューウィンドウ モード
        /// </summary>
        [PropertyMember]
        public bool IsFloating
        {
            get { return _isFloating; }
            set { SetProperty(ref _isFloating, value); }
        }

        /// <summary>
        /// メインビューウィンドウを閉じたときにウィンドウモードを解除するか
        /// </summary>
        [PropertyMember]
        public bool IsFloatingEndWhenClosed
        {
            get { return _isFloatingEndWhenClosed; }
            set { SetProperty(ref _isFloatingEndWhenClosed, value); }
        }

        /// <summary>
        /// メインビューの代替コンテンツ
        /// </summary>
        [PropertyMember]
        public AlternativeContent AlternativeContent
        {
            get { return _alternativeContent; }
            set { SetProperty(ref _alternativeContent, value); }
        }

        /// <summary>
        /// メインビューウィンドウ 常に手前に表示
        /// </summary>
        [PropertyMember]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        /// <summary>
        /// メインビューウィンドウ なるべく手前に表示
        /// </summary>
        [PropertyMember]
        public bool IsFrontAsPossible
        {
            get { return _isFrontAsPossible; }
            set { SetProperty(ref _isFrontAsPossible, value); }
        }

        /// <summary>
        /// メインビューウィンドウ タイトルバー非表示
        /// </summary>
        [PropertyMember]
        public bool IsHideTitleBar
        {
            get { return _isHideTitleBar; }
            set { SetProperty(ref _isHideTitleBar, value); }
        }

        /// <summary>
        /// メインビューウィンドウサイズを自動調整
        /// </summary>
        [PropertyMember]
        public bool IsAutoStretch
        {
            get { return _isAutoStretch; }
            set { SetProperty(ref _isAutoStretch, value); }
        }

        /// <summary>
        /// ブックを開いていない時にメインビューウィンドウを自動非表示
        /// </summary>
        [PropertyMember]
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set { SetProperty(ref _isAutoHide, value); }
        }

        /// <summary>
        /// ページ切替時に最小化されているメインビューウィンドウを自動表示
        /// </summary>
        [PropertyMember]
        public bool IsAutoShow
        {
            get { return _isAutoShow; }
            set { SetProperty(ref _isAutoShow, value); }
        }

        /// <summary>
        /// フルスクリーンから復帰するウィンドウ状態
        /// </summary>
        [PropertyMapIgnore]
        [DefaultEquality]
        public WindowStateEx LastState { get; set; } = WindowStateEx.Normal;

        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;

        /// <summary>
        /// リファレンスサイズ
        /// </summary>
        [PropertyMapIgnore]
        public Size ReferenceSize
        {
            get { return _referenceSize; }
            set { SetProperty(ref _referenceSize, value); }
        }
    }


    /// <summary>
    /// メインビューの代替コンテンツ
    /// </summary>
    public enum AlternativeContent
    {
        [AliasName]
        Blank,

        [AliasName]
        PageList,
    }
}


