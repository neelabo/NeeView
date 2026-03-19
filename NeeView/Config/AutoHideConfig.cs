using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class AutoHideConfig : BindableBase
    {
        private double _autoHideDelayTime = 1.0;
        private double _autoHideDelayVisibleTime = 0.0;
        private AutoHideFocusLockMode _autoHideFocusLockMode = AutoHideFocusLockMode.LogicalTextBoxFocusLock;
        private bool _isAutoHideKeyDownDelay = true;
        private double _autoHideHitTestHorizontalMargin = 32.0;
        private double _autoHideHitTestVerticalMargin = 32.0;
        private AutoHideConflictMode _autoHideConflictTopMargin = AutoHideConflictMode.AllowPixel;
        private AutoHideConflictMode _autoHideConflictBottomMargin = AutoHideConflictMode.Allow;

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember(HasDecimalPoint = true)]
        public double AutoHideDelayTime
        {
            get { return _autoHideDelayTime; }
            set { SetProperty(ref _autoHideDelayTime, value); }
        }

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember(HasDecimalPoint = true)]
        public double AutoHideDelayVisibleTime
        {
            get { return _autoHideDelayVisibleTime; }
            set { SetProperty(ref _autoHideDelayVisibleTime, value); }
        }

        // パネル自動非表示のフォーカス挙動モード
        [PropertyMember]
        public AutoHideFocusLockMode AutoHideFocusLockMode
        {
            get { return _autoHideFocusLockMode; }
            set { SetProperty(ref _autoHideFocusLockMode, value); }
        }

        // パネル自動非表示のキー入力遅延
        [PropertyMember]
        public bool IsAutoHideKeyDownDelay
        {
            get { return _isAutoHideKeyDownDelay; }
            set { SetProperty(ref _isAutoHideKeyDownDelay, value); }
        }

        // パネル自動非表示の表示判定マージン(水平方向)
        [PropertyMember]
        public double AutoHideHitTestHorizontalMargin
        {
            get { return _autoHideHitTestHorizontalMargin; }
            set { SetProperty(ref _autoHideHitTestHorizontalMargin, value); }
        }

        // パネル自動非表示の表示判定マージン(垂直方向)
        [PropertyMember]
        public double AutoHideHitTestVerticalMargin
        {
            get { return _autoHideHitTestVerticalMargin; }
            set { SetProperty(ref _autoHideHitTestVerticalMargin, value); }
        }


        // サイドパネルとメニューの自動非表示判定が重なった場合
        [PropertyMember]
        public AutoHideConflictMode AutoHideConflictTopMargin
        {
            get { return _autoHideConflictTopMargin; }
            set { SetProperty(ref _autoHideConflictTopMargin, value); }
        }

        // サイドパネルとスライダーの自動非表示判定が重なった場合
        [PropertyMember]
        public AutoHideConflictMode AutoHideConflictBottomMargin
        {
            get { return _autoHideConflictBottomMargin; }
            set { SetProperty(ref _autoHideConflictBottomMargin, value); }
        }

        #region Obsolete

        // パネル自動非表示の表示判定マージン(水平方向)
        [Obsolete("no used"), Alternative("AutoHideHitTestHorizontalMargin and AutoHideHitTestVerticalMargin", 39)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double AutoHideHitTestMargin
        {
            get => default;
            set { AutoHideHitTestHorizontalMargin = value; AutoHideHitTestVerticalMargin = value; }
        }

        [Obsolete("Typo"), Alternative(nameof(AutoHideConflictTopMargin), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public AutoHideConflictMode AutoHideConfrictTopMargin
        {
            get { return AutoHideConflictTopMargin; }
            set { AutoHideConflictTopMargin = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("AutoHideConfrictTopMargin"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AutoHideConflictMode AutoHideConfrictTopMargin_Typo
        {
            get { return default; }
            set { AutoHideConflictTopMargin = value; }
        }


        [Obsolete("Typo"), Alternative(nameof(AutoHideConflictBottomMargin), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public AutoHideConflictMode AutoHideConfrictBottomMargin
        {
            get { return AutoHideConflictBottomMargin; }
            set { AutoHideConflictBottomMargin = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("AutoHideConfrictBottomMargin"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public AutoHideConflictMode AutoHideConfrictBottomMargin_Typo
        {
            get { return default; }
            set { AutoHideConflictBottomMargin = value; }
        }

        #endregion Obsolete
    }


    public enum AutoHideConflictMode
    {
        Allow,
        AllowPixel,
        Deny,
    }
}
