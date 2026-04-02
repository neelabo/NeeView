using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class WindowConfig : BindableBase
    {
        [DefaultEquality] private bool _isTopmost = false;
        [IgnoreEquality] private WindowStateEx _state;
        [DefaultEquality] private bool _isCaptionEmulateInFullScreen;
        [DefaultEquality] private bool _mouseActivateAndEat;
        [DefaultEquality] private bool _isAeroSnapPlacementEnabled = true;
        [DefaultEquality] private bool _isAutoHideInNormal = false;
        [DefaultEquality] private bool _isAutoHideInMaximized = false;
        [DefaultEquality] private bool _IsAutoHideInFullScreen = true;
        [DefaultEquality] private bool _isAutoHideScrollBar = true;


        [PropertyMember]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember]
        public bool IsCaptionEmulateInFullScreen
        {
            get { return _isCaptionEmulateInFullScreen; }
            set { SetProperty(ref _isCaptionEmulateInFullScreen, value); }
        }


        [PropertyMember]
        public bool MouseActivateAndEat
        {
            get { return _mouseActivateAndEat; }
            set { SetProperty(ref _mouseActivateAndEat, value); }
        }

        /// <summary>
        /// ウィンドウ状態
        /// </summary>
        [PropertyMember]
        [JsonIgnore]
        public WindowStateEx State
        {
            get { return _state; }
            set
            {
                if (FreezeWindowState) return;
                SetProperty(ref _state, value);
            }
        }

        /// <summary>
        /// エアロスナップのウィンドウ座標を保存
        /// </summary>
        [PropertyMember]
        public bool IsRestoreAeroSnapPlacement
        {
            get { return _isAeroSnapPlacementEnabled; }
            set { SetProperty(ref _isAeroSnapPlacementEnabled, value); }
        }

        [PropertyMember]
        public bool IsAutoHideInNormal
        {
            get { return _isAutoHideInNormal; }
            set { SetProperty(ref _isAutoHideInNormal, value); }
        }

        [PropertyMember]
        public bool IsAutoHideInMaximized
        {
            get { return _isAutoHideInMaximized; }
            set { SetProperty(ref _isAutoHideInMaximized, value); }
        }

        [PropertyMember]
        public bool IsAutoHideInFullScreen
        {
            get { return _IsAutoHideInFullScreen; }
            set { SetProperty(ref _IsAutoHideInFullScreen, value); }
        }

        [PropertyMember]
        public bool IsAutoHideScrollBar
        {
            get { return _isAutoHideScrollBar; }
            set { SetProperty(ref _isAutoHideScrollBar, value); }
        }

        #region HiddenParameters

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
        [DefaultEquality]
        public WindowPlacement? WindowPlacement { get; set; }

        /// <summary>
        /// ウィンドウ状態を凍結
        /// </summary>
        [JsonIgnore]
        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public bool FreezeWindowState { get; set; }

        #endregion HiddenParameters


        #region Obsolete

        [Obsolete("no used"), Alternative("Window.Border in the custom theme file", 39, IsFullName = true)] // ver.39
        [JsonIgnore]
        public WindowChromeFrame WindowChromeFrame
        {
            get { return WindowChromeFrame.None; }
            set { }
        }

        [Obsolete("no used"), Alternative(null, 39)] // ver.39
        [JsonIgnore]
        public bool IsCaptionVisible
        {
            get { return false; }
            set { }
        }

        [Obsolete("no used"), Alternative(null, 40, ScriptErrorLevel.Warning)] // ver.40
        [JsonIgnore]
        public double MaximizeWindowGapWidth
        {
            get { return 0.0; }
            set { }
        }

        [Obsolete("Typo"), Alternative(nameof(IsAutoHideInMaximized), 42, ScriptErrorLevel.Info)] // ver.42
        [JsonIgnore]
        public bool IsAutoHidInMaximized
        {
            get { return IsAutoHideInMaximized; }
            set { IsAutoHideInMaximized = value; }
        }

        #endregion Obsolete
    }

    [Obsolete("no used")] // ver.39
    public enum WindowChromeFrame
    {
        [AliasName]
        None,

        [AliasName]
        WindowFrame,
    }

}
