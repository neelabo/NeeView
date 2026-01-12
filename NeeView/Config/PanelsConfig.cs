using NeeLaboratory.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class PanelsConfig : BindableBase
    {
        private bool _isHideLeftPanel;
        private bool _isHideRightPanel;
        private bool _isSideBarEnabled = true;
        private double _opacity = 1.0;
        private bool _isHideLeftPanelInAutoHideMode = true;
        private bool _isHideRightPanelInAutoHideMode = true;
        private bool _isDecoratePlace = true;
        private bool _openWithDoubleClick;
        private bool _isLeftRightKeyEnabled;
        private bool _isManipulationBoundaryFeedbackEnabled;
        private double _mouseWheelSpeedRate = 1.0;
        private double _leftPanelWidth = 300.0;
        private double _rightPanelWidth = 300.0;
        private bool _isLimitPanelWidth;
        private bool? _isVisibleItemsCount;
        private bool _isTextSearchEnabled;
        private double _conflictTopMargin = 32.0;
        private double _conflictBottomMargin = 20.0;


        /// <summary>
        /// 左パネルを自動的に隠す
        /// </summary>
        [PropertyMember]
        public bool IsHideLeftPanel
        {
            get { return _isHideLeftPanel; }
            set { SetProperty(ref _isHideLeftPanel, value); }
        }

        /// <summary>
        /// 右パネルを自動的に隠す
        /// </summary>
        [PropertyMember]
        public bool IsHideRightPanel
        {
            get { return _isHideRightPanel; }
            set { SetProperty(ref _isHideRightPanel, value); }
        }

        /// <summary>
        /// 左パネルを自動的に隠す(自動非表示モード)
        /// </summary>
        [PropertyMember]
        public bool IsHideLeftPanelInAutoHideMode
        {
            get { return _isHideLeftPanelInAutoHideMode; }
            set { SetProperty(ref _isHideLeftPanelInAutoHideMode, value); }
        }

        /// <summary>
        /// 右パネルを自動的に隠す(自動非表示モード)
        /// </summary>
        [PropertyMember]
        public bool IsHideRightPanelInAutoHideMode
        {
            get { return _isHideRightPanelInAutoHideMode; }
            set { SetProperty(ref _isHideRightPanelInAutoHideMode, value); }
        }

        /// <summary>
        /// サイドバー表示フラグ 
        /// </summary>
        [PropertyMember]
        public bool IsSideBarEnabled
        {
            get { return _isSideBarEnabled; }
            set { SetProperty(ref _isSideBarEnabled, value); }
        }

        /// <summary>
        /// パネルの透明度
        /// </summary>
        [PropertyPercent]
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }

        /// <summary>
        /// ダブルクリックでブックを開く
        /// </summary>
        [PropertyMember]
        public bool OpenWithDoubleClick
        {
            get { return _openWithDoubleClick; }
            set { SetProperty(ref _openWithDoubleClick, value); }
        }

        /// <summary>
        /// パネルでの左右キー操作有効
        /// </summary>
        [PropertyMember]
        public bool IsLeftRightKeyEnabled
        {
            get { return _isLeftRightKeyEnabled; }
            set { SetProperty(ref _isLeftRightKeyEnabled, value); }
        }

        /// <summary>
        /// タッチパ操作でのリストバウンド効果
        /// </summary>
        [PropertyMember]
        public bool IsManipulationBoundaryFeedbackEnabled
        {
            get { return _isManipulationBoundaryFeedbackEnabled; }
            set { SetProperty(ref _isManipulationBoundaryFeedbackEnabled, value); }
        }

        /// <summary>
        /// パス表示形式を "CCC (C:\AAA\BBB) にする
        /// </summary>
        [PropertyMember]
        public bool IsDecoratePlace
        {
            get { return _isDecoratePlace; }
            set { SetProperty(ref _isDecoratePlace, value); }
        }

        /// <summary>
        /// サムネイルリストのマウスホイール速度倍率
        /// </summary>
        [PropertyRange(0.1, 2.0, TickFrequency = 0.1, Format = "× {0:0.0}")]
        public double MouseWheelSpeedRate
        {
            get { return _mouseWheelSpeedRate; }
            set
            {
                if (SetProperty(ref _mouseWheelSpeedRate, Math.Max(value, 0.1)))
                {
                    RaisePropertyChanged(nameof(MouseWheelDelta));
                }
            }
        }

        /// <summary>
        /// ウィンドウに収まるようにパネル幅を制限する
        /// </summary>
        [PropertyMember]
        public bool IsLimitPanelWidth
        {
            get { return _isLimitPanelWidth; }
            set { SetProperty(ref _isLimitPanelWidth, value); }
        }

        [PropertyMember]
        public bool IsTextSearchEnabled
        {
            get { return _isTextSearchEnabled; }
            set { SetProperty(ref _isTextSearchEnabled, value); }
        }

        [PropertyMember]
        public double ConflictTopMargin
        {
            get { return _conflictTopMargin; }
            set { SetProperty(ref _conflictTopMargin, Math.Max(value, 0.0)); }
        }

        [PropertyMember]
        public double ConflictBottomMargin
        {
            get { return _conflictBottomMargin; }
            set { SetProperty(ref _conflictBottomMargin, Math.Max(value, 0.0)); }
        }


        [PropertyMapLabel("Word.StyleList")]
        public PanelListItemProfile NormalItemProfile { get; set; } = PanelListItemProfile.DefaultNormalItemProfile.Clone();

        [PropertyMapLabel("Word.StyleContent")]
        public PanelListItemProfile ContentItemProfile { get; set; } = PanelListItemProfile.DefaultContentItemProfile.Clone();

        [PropertyMapLabel("Word.StyleBanner")]
        public PanelListItemProfile BannerItemProfile { get; set; } = PanelListItemProfile.DefaultBannerItemProfile.Clone();

        [PropertyMapLabel("Word.StyleThumbnail")]
        public PanelListItemProfile ThumbnailItemProfile { get; set; } = PanelListItemProfile.DefaultThumbnailItemProfile.Clone();


        #region HiddenParameters

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double MouseWheelDelta
        {
            get => MouseWheelSpeedRate * SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines;
        }

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double LeftPanelWidth
        {
            get { return _leftPanelWidth; }
            set { SetProperty(ref _leftPanelWidth, value); }
        }

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double RightPanelWidth
        {
            get { return _rightPanelWidth; }
            set { SetProperty(ref _rightPanelWidth, value); }
        }

        // ver 38
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public LayoutPanelManager.Memento? Layout { get; set; }

        #endregion HiddenParameters


        #region Obsolete

        [Obsolete("no used"), PropertyMapIgnore]
        [JsonIgnore]
        public string? FontName_Legacy { get; private set; }

        [Obsolete("no used"), Alternative("nv.Config.Fonts.FontName", 39, IsFullName = true)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? FontName
        {
            get { return null; }
            set { FontName_Legacy = value; }
        }

        [Obsolete("no used"), PropertyMapIgnore]
        [JsonIgnore]
        public double FontSize_Legacy { get; private set; }

        [Obsolete("no used"), Alternative("nv.Config.Fonts.PanelFontScale", 39, IsFullName = true)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double FontSize
        {
            get { return 0.0; }
            set { FontSize_Legacy = value; }
        }

        [Obsolete("no used"), PropertyMapIgnore]
        [JsonIgnore]
        public double FolderTreeFontSize_Legacy { get; private set; }

        [Obsolete("no used"), Alternative("nv.Config.Fonts.FolderTreeFontScale", 39, IsFullName = true)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double FolderTreeFontSize
        {
            get { return 0.0; }
            set { FolderTreeFontSize_Legacy = value; }
        }

        [Obsolete("no used"), Alternative("IsHideLeftPanelInAutoHideMode, IsHideRightPanelInAutoHideMode", 38)] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePanelInFullscreen
        {
            get { return default; }
            set
            {
                IsHideLeftPanelInAutoHideMode = value;
                IsHideRightPanelInAutoHideMode = value;
            }
        }

        [Obsolete("no used"), Alternative(null, 38)] // ver.38
        [JsonIgnore]
        public string? PanelDocks { get; set; }

        [Obsolete("no used"), Alternative(null, 38)] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? LeftPanelSeleted { get; set; }

        [Obsolete("no used"), Alternative(null, 38)] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? RightPanelSeleted { get; set; }

        [Obsolete("no used"), Alternative("IsHideLeftPanel, IsHideRightPanel", 44, ScriptErrorLevel.Info)] // ver.44
        [JsonIgnore]
        public bool IsHidePanel
        {
            get { return _isHideLeftPanel || _isHideRightPanel; }
            set
            {
                IsHideLeftPanel = value;
                IsHideRightPanel = value;
            }
        }

        [Obsolete("no used"), PropertyMapIgnore] // ver.44
        [JsonPropertyName("IsHidePanel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePanel_Legacy
        {
            get { return default; }
            set
            {
                IsHideLeftPanel = value;
                IsHideRightPanel = value;
            }
        }

        [Obsolete("no used"), Alternative("IsHideLeftPanelInAutoHideMode, IsHideRightPanelInAutoHideMode", 44, ScriptErrorLevel.Info)] // ver.44
        [JsonIgnore]
        public bool IsHidePanelInAutoHideMode
        {
            get { return IsHideLeftPanelInAutoHideMode || IsHideRightPanelInAutoHideMode; }
            set
            {
                IsHideLeftPanelInAutoHideMode = value;
                IsHideRightPanelInAutoHideMode = value;
            }
        }

        [Obsolete("no used"), PropertyMapIgnore] // ver.44
        [JsonPropertyName("IsHidePanelInAutoHideMode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePanelInAutoHideMode_Legacy
        {
            get { return default; }
            set
            {
                IsHideLeftPanelInAutoHideMode = value;
                IsHideRightPanelInAutoHideMode = value;
            }
        }

        [Obsolete("no used"), Alternative("IsVisibleItemsCount in nv.Config.Bookshelf, Bookmark, PageList and History", 45, ScriptErrorLevel.Warning)] // ver.45
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? IsVisibleItemsCount
        {
            get { return default; }
            set { _isVisibleItemsCount = value; }
        }

        #endregion Obsolete

        public bool TryGetIsVisibleItemsCount(out bool result)
        {
            if (_isVisibleItemsCount.HasValue)
            {
                result = _isVisibleItemsCount.Value;
                return true;
            }
            else
            {
                result = false;
                return false;
            }
        }

        public PanelListItemProfile GetPanelListItemProfile(PanelListItemStyle style)
        {
            return style switch
            {
                PanelListItemStyle.Normal => NormalItemProfile,
                PanelListItemStyle.Content => ContentItemProfile,
                PanelListItemStyle.Banner => BannerItemProfile,
                PanelListItemStyle.Thumbnail => ThumbnailItemProfile,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }


    public enum PanelDock
    {
        Left,
        Right
    }

}