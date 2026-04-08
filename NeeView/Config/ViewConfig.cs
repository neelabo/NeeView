using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ViewConfig : BindableBaseFull
    {
        [DefaultEquality] private PageStretchMode _stretchMode = PageStretchMode.Uniform;
        [DefaultEquality] private PageStretchMode _validStretchMode = PageStretchMode.Uniform;
        [DefaultEquality] private bool _allowStretchScaleUp = true;
        [DefaultEquality] private bool _allowStretchScaleDown = true;
        [DefaultEquality] private bool _allowFileContentAutoRotate;
        [DefaultEquality] private bool _isLimitMove = true;
        [DefaultEquality] private bool _isMoveLockStart = true;
        [DefaultEquality] private DragControlCenter _rotateCenter;
        [DefaultEquality] private DragControlCenter _scaleCenter;
        [DefaultEquality] private DragControlCenter _flipCenter;
        [DefaultEquality] private bool _isScaleStretchTracking;
        [DefaultEquality] private bool _isKeepScale;
        [DefaultEquality] private bool _isKeepAngle;
        [DefaultEquality] private bool _isKeepFlip;
        [DefaultEquality] private ViewHorizontalOrigin _viewHorizontalOrigin = ViewHorizontalOrigin.CenterOrDirectionDependent;
        [DefaultEquality] private ViewVerticalOrigin _viewVerticalOrigin = ViewVerticalOrigin.CenterOrDirectionDependent;
        [DefaultEquality] private double _angleFrequency = 0;
        [DefaultEquality] private bool _isBaseScaleEnabled = true;
        [DefaultEquality] private bool _isRotateStretchEnabled = true;
        [DefaultEquality] private double _mainViewMargin;
        [DefaultEquality] private bool _isKeepScaleBooks;
        [DefaultEquality] private bool _isKeepAngleBooks;
        [DefaultEquality] private bool _isKeepFlipBooks;
        [DefaultEquality] private bool _isKeepPageTransform;
        [DefaultEquality] private double _scrollDuration = 0.2;
        [DefaultEquality] private double _pageMoveDuration = 0.0;
        [DefaultEquality] private AutoRotatePolicy _autoRotatePolicy = AutoRotatePolicy.FitToViewArea;

        [IgnoreEquality]
        private BookSettingConfig? _bookSetting;


        // 回転の中心
        [PropertyMember]
        public DragControlCenter RotateCenter
        {
            get { return _rotateCenter; }
            set { SetProperty(ref _rotateCenter, value); }
        }

        // 拡大の中心
        [PropertyMember]
        public DragControlCenter ScaleCenter
        {
            get { return _scaleCenter; }
            set { SetProperty(ref _scaleCenter, value); }
        }

        // 反転の中心
        [PropertyMember]
        public DragControlCenter FlipCenter
        {
            get { return _flipCenter; }
            set { SetProperty(ref _flipCenter, value); }
        }

        // スケールのストレッチモード追従
        [PropertyMember]
        public bool IsScaleStretchTracking
        {
            get { return _isScaleStretchTracking; }
            set { SetProperty(ref _isScaleStretchTracking, value); }
        }

        // 拡大率キープ
        [PropertyMember]
        public bool IsKeepScale
        {
            get { return _isKeepScale; }
            set { SetProperty(ref _isKeepScale, value); }
        }

        // ブック間の拡大率キープ
        [PropertyMember]
        public bool IsKeepScaleBooks
        {
            get { return _isKeepScaleBooks; }
            set { SetProperty(ref _isKeepScaleBooks, value); }
        }

        // 回転キープ
        [PropertyMember]
        public bool IsKeepAngle
        {
            get { return _isKeepAngle; }
            set { SetProperty(ref _isKeepAngle, value); }
        }

        // ブック間の回転キープ
        [PropertyMember]
        public bool IsKeepAngleBooks
        {
            get { return _isKeepAngleBooks; }
            set { SetProperty(ref _isKeepAngleBooks, value); }
        }

        // 反転キープ
        [PropertyMember]
        public bool IsKeepFlip
        {
            get { return _isKeepFlip; }
            set { SetProperty(ref _isKeepFlip, value); }
        }

        // ブック間の反転キープ
        [PropertyMember]
        public bool IsKeepFlipBooks
        {
            get { return _isKeepFlipBooks; }
            set { SetProperty(ref _isKeepFlipBooks, value); }
        }

        // 開始時の水平配置
        [PropertyMember]
        public ViewHorizontalOrigin ViewHorizontalOrigin
        {
            get { return _viewHorizontalOrigin; }
            set { SetProperty(ref _viewHorizontalOrigin, value); }
        }

        // 開始時の垂直配置
        [PropertyMember]
        public ViewVerticalOrigin ViewVerticalOrigin
        {
            get { return _viewVerticalOrigin; }
            set { SetProperty(ref _viewVerticalOrigin, value); }
        }

        // 回転スナップ。0で無効
        [PropertyMember]
        public double AngleFrequency
        {
            get { return _angleFrequency; }
            set { SetProperty(ref _angleFrequency, AppMath.Round(value)); }
        }

        // ウィンドウ枠内の移動に制限する
        [PropertyMember]
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set { SetProperty(ref _isLimitMove, value); }
        }

        // 移動ロック状態で開始する
        [PropertyMember]
        public bool IsMoveLockStart
        {
            get { return _isMoveLockStart; }
            set { SetProperty(ref _isMoveLockStart, value); }
        }

        // スケールモード
        [PropertyMember]
        public PageStretchMode StretchMode
        {
            get { return _stretchMode; }
            set { SetStretchMode(value, false); }
        }

        // 有効なスケールモード
        // StretchMode.None のトグルに使用する
        [PropertyMapIgnore]
        public PageStretchMode ValidStretchMode => _validStretchMode;

        // スケールモード・拡大許可
        [PropertyMember]
        public bool AllowStretchScaleUp
        {
            get { return _allowStretchScaleUp; }
            set { SetProperty(ref _allowStretchScaleUp, value); }
        }

        // スケールモード・縮小許可
        [PropertyMember]
        public bool AllowStretchScaleDown
        {
            get { return _allowStretchScaleDown; }
            set { SetProperty(ref _allowStretchScaleDown, value); }
        }

        // 基底スケール有効
        [PropertyMember]
        public bool IsBaseScaleEnabled
        {
            get { return _isBaseScaleEnabled; }
            set { SetProperty(ref _isBaseScaleEnabled, value); }
        }

        // ファイルコンテンツの自動回転を許可する
        public bool AllowFileContentAutoRotate
        {
            get { return _allowFileContentAutoRotate; }
            set { SetProperty(ref _allowFileContentAutoRotate, value); }
        }

        // ナビゲーターボタンによる回転にストレッチを適用
        [PropertyMember]
        public bool IsRotateStretchEnabled
        {
            get { return _isRotateStretchEnabled; }
            set { SetProperty(ref _isRotateStretchEnabled, value); }
        }

        // ビューエリアの余白
        [PropertyRange(0.0, 100.0)]
        public double MainViewMargin
        {
            get { return _mainViewMargin; }
            set { SetProperty(ref _mainViewMargin, AppMath.Round(value)); }
        }

        // ページトランスフォームの維持
        [PropertyMember]
        public bool IsKeepPageTransform
        {
            get { return _isKeepPageTransform; }
            set { SetProperty(ref _isKeepPageTransform, value); }
        }


        // スクロール時間 (秒)
        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true, HasDecimalPoint = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, AppMath.Round(value)); }
        }

        // ページ変更時間(秒)
        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true, HasDecimalPoint = true)]
        public double PageMoveDuration
        {
            get { return _pageMoveDuration; }
            set { SetProperty(ref _pageMoveDuration, AppMath.Round(value)); }
        }

        // 自動回転方針
        [PropertyMember]
        public AutoRotatePolicy AutoRotatePolicy
        {
            get { return _autoRotatePolicy; }
            set { SetProperty(ref _autoRotatePolicy, value); }
        }


        #region Obsolete

        [Obsolete("no used"), Alternative($"{nameof(ViewHorizontalOrigin)}, {nameof(ViewVerticalOrigin)}", 42, ScriptErrorLevel.Warning)] // ver.42
        [JsonIgnore]
        public ViewOrigin ViewOrigin
        {
            get
            {
                if (ViewHorizontalOrigin == ViewHorizontalOrigin.Center && ViewVerticalOrigin == ViewVerticalOrigin.Center)
                {
                    return ViewOrigin.Center;
                }
                else if (ViewHorizontalOrigin == ViewHorizontalOrigin.CenterOrDirectionDependent && ViewVerticalOrigin == ViewVerticalOrigin.CenterOrTop)
                {
                    return ViewOrigin.DirectionDependentAndTop;
                }
                else
                {
                    return ViewOrigin.DirectionDependent;
                }
            }
            set
            {
                switch (value)
                {
                    case ViewOrigin.Center:
                        ViewHorizontalOrigin = ViewHorizontalOrigin.Center;
                        ViewVerticalOrigin = ViewVerticalOrigin.Center;
                        break;
                    case ViewOrigin.DirectionDependent:
                        ViewHorizontalOrigin = ViewHorizontalOrigin.CenterOrDirectionDependent;
                        ViewVerticalOrigin = ViewVerticalOrigin.CenterOrDirectionDependent;
                        break;
                    case ViewOrigin.DirectionDependentAndTop:
                        ViewHorizontalOrigin = ViewHorizontalOrigin.CenterOrDirectionDependent;
                        ViewVerticalOrigin = ViewVerticalOrigin.CenterOrTop;
                        break;
                }
            }
        }

        [Obsolete("ViewOrigin interface"), PropertyMapIgnore]
        [JsonPropertyName("ViewOrigin"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ViewOrigin ViewOrigin_Legacy
        {
            get { return default; }
            set { ViewOrigin = value; }
        }

        [PropertyMember]
        [Obsolete("no used"), Alternative($"{nameof(ViewHorizontalOrigin)},{nameof(ViewVerticalOrigin)}", 40, ScriptErrorLevel.Warning)] // ver.40.5
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsViewStartPositionCenter
        {
            get { return false; }
            set { ViewOrigin = value ? ViewOrigin.Center : ViewOrigin.DirectionDependent; }
        }

        [Obsolete("Typo"), Alternative(nameof(MainViewMargin), 40, ScriptErrorLevel.Info)] // ver.40
        [JsonIgnore]
        public double MainViewMergin
        {
            get { return MainViewMargin; }
            set { MainViewMargin = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("MainViewMergin"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double MainViewMergin_Typo
        {
            get { return 0.0; }
            set { MainViewMargin = value; }
        }

        // スクリプト互換性用：自動回転左/右
        // 設定値は BookSetting のものを使用する
        [PropertyMember]
        [Obsolete("no used"), Alternative($"nv.Config.BookSetting.AutoRotate", 40, ScriptErrorLevel.Info, IsFullName = true)] // ver.40
        [JsonIgnore]
        [IgnoreEquality]
        public AutoRotateType AutoRotate
        {
            get { return _bookSetting?.AutoRotate ?? default; }
            set { if (_bookSetting is not null) _bookSetting.AutoRotate = value; }
        }

        // 基底スケール
        [PropertyPercent(0.1, 2.0, TickFrequency = 0.01)]
        [Obsolete("no used"), Alternative($"nv.Config.BookSetting.BaseScale", 40, ScriptErrorLevel.Info, IsFullName = true)] // ver.40
        [JsonIgnore]
        [IgnoreEquality]
        public double BaseScale
        {
            get { return _bookSetting?.BaseScale ?? 1.0; }
            set { if (_bookSetting is not null) _bookSetting.BaseScale = value; }
        }

        /// <summary>
        /// AutoRotate プロパティを外部情報に依存させる
        /// </summary>
        /// <param name="bookSetting"></param>
        public void SetBookSettingSource(BookSettingConfig bookSetting)
        {
            _bookSetting = bookSetting;
        }

        #endregion


        /// <summary>
        /// ストレッチモード設定
        /// </summary>
        /// <param name="value">ストレッチモード</param>
        /// <param name="force">強制更新</param>
        public void SetStretchMode(PageStretchMode value, bool force)
        {
            if (force || _stretchMode != value)
            {
                _stretchMode = value;
                _validStretchMode = _stretchMode != PageStretchMode.None ? value : _validStretchMode;
                RaisePropertyChanged(nameof(StretchMode));
                RaisePropertyChanged(nameof(ValidStretchMode));
            }
        }
    }


    /// <summary>
    /// 表示開始時の基準
    /// </summary>
    [Obsolete]
    public enum ViewOrigin
    {
        /// <summary>
        /// 中央
        /// </summary>
        Center,

        /// <summary>
        /// 方向に依存
        /// </summary>
        DirectionDependent,

        /// <summary>
        /// 方向に依存、縦方向は上に固定
        /// </summary>
        DirectionDependentAndTop,
    }

    public enum ViewHorizontalOrigin
    {
        Center,
        Left,
        Right,
        DirectionDependent,
        CenterOrLeft,
        CenterOrRight,
        CenterOrDirectionDependent,
    }

    public enum ViewVerticalOrigin
    {
        Center,
        Top,
        Bottom,
        DirectionDependent,
        CenterOrTop,
        CenterOrBottom,
        CenterOrDirectionDependent,
    }

    public enum AutoRotatePolicy
    {
        /// <summary>
        /// 表示領域に合わせる
        /// </summary>
        FitToViewArea,

        /// <summary>
        /// 横長にする
        /// </summary>
        ToLandscape,

        /// <summary>
        /// 縦長にする
        /// </summary>
        ToPortrait,
    }
}
