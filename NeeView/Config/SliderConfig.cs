using Generator.Equals;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class SliderConfig : BindableBase
    {
        [IgnoreEquality] private bool _isVisible;
        [DefaultEquality] private bool _isIsHidePageSlider;
        [DefaultEquality] private double _sliderOpacity = 1.0;
        [DefaultEquality] private SliderIndexLayout _sliderIndexLayout = SliderIndexLayout.Right;
        [DefaultEquality] private SliderDirection _sliderDirection = SliderDirection.SyncBookReadDirection;
        [DefaultEquality] private bool _isSliderLinkedFilmStrip = true;
        [DefaultEquality] private bool _isHidePageSliderInAutoHideMode = true;
        [DefaultEquality] private bool _isSyncPageMode;
        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private bool _isVisiblePlaylistMark = true;
        [DefaultEquality] private double _thickness = 25.0;


        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        /// <summary>
        /// スライダー表示
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        // スライダーを自動的に隠す
        [PropertyMember]
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set { SetProperty(ref _isIsHidePageSlider, value); }
        }

        /// <summary>
        /// スライダーを自動的に隠す(自動非表示モード)
        /// </summary>
        [PropertyMember]
        public bool IsHidePageSliderInAutoHideMode
        {
            get { return _isHidePageSliderInAutoHideMode; }
            set { SetProperty(ref _isHidePageSliderInAutoHideMode, value); }
        }

        // プレイリストマーク表示
        [PropertyMember]
        public bool IsVisiblePlaylistMark
        {
            get { return _isVisiblePlaylistMark; }
            set { SetProperty(ref _isVisiblePlaylistMark, value); }
        }

        // スライダーの厚さ
        [PropertyRange(15.0, 50.0, TickFrequency = 1.0)]
        public double Thickness
        {
            get { return _thickness; }
            set { SetProperty(ref _thickness, AppMath.Round(MathUtility.Clamp(value, 15.0, 50.0))); }
        }

        // スライダー透明度
        [PropertyPercent]
        public double Opacity
        {
            get { return _sliderOpacity; }
            set { SetProperty(ref _sliderOpacity, AppMath.Round(value)); }
        }

        /// <summary>
        /// ページ数表示位置
        /// </summary>
        [PropertyMember]
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _sliderIndexLayout; }
            set { SetProperty(ref _sliderIndexLayout, value); }
        }

        /// <summary>
        /// スライダーの方向定義
        /// </summary>
        [PropertyMember]
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set { SetProperty(ref _sliderDirection, value); }
        }

        /// <summary>
        /// フィルムストリップとスライダーの連動
        /// フィルムストリップ表示時に限りフィルムストリップのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        [PropertyMember]
        public bool IsSliderLinkedFilmStrip
        {
            get { return _isSliderLinkedFilmStrip; }
            set { SetProperty(ref _isSliderLinkedFilmStrip, value); }
        }

        /// <summary>
        /// スライダーの移動量をページモードに従う
        /// </summary>
        [PropertyMember]
        public bool IsSyncPageMode
        {
            get { return _isSyncPageMode; }
            set { SetProperty(ref _isSyncPageMode, value); }
        }


        #region Obsolete

        [Obsolete("no used"), Alternative(nameof(IsHidePageSliderInAutoHideMode), 38)] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePageSliderInFullscreen
        {
            get { return default; }
            set { IsHidePageSliderInAutoHideMode = value; }
        }

        #endregion Obsoletet
    }
}
