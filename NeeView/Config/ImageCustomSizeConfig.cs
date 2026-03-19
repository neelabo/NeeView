using Generator.Equals;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class ImageCustomSizeConfig : BindableBase
    {
        private bool _isEnabled;
        private Size _size = new(256, 256);
        private CustomSizeAspectRatio _aspectRatio;
        private double _applicabilityRate = 1.0;
        private bool _isAlignLongSide = false;


        /// <summary>
        /// 指定サイズ有効
        /// </summary>
        [PropertyMember(IsVisible = false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// カスタムサイズ
        /// </summary>
        [PropertyMember(IsVisible = false)]
        public Size Size
        {
            get { return _size; }
            set
            {
                if (SetProperty(ref _size, value))
                {
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(Height));
                }
            }
        }

        /// <summary>
        /// カスタムサイズ：横幅
        /// </summary>
        [PropertyRange(16, 4096)]
        [JsonIgnore, PropertyMapIgnore]
        public int Width
        {
            get { return (int)_size.Width; }
            set { if (value != _size.Width) { Size = new Size(value, _size.Height); } }
        }

        /// <summary>
        /// カスタムサイズ：縦幅
        /// </summary>
        [PropertyRange(16, 4096)]
        [JsonIgnore, PropertyMapIgnore]
        public int Height
        {
            get { return (int)_size.Height; }
            set { if (value != _size.Height) { Size = new Size(_size.Width, value); } }
        }


        /// <summary>
        /// アスペクト比
        /// </summary>
        [PropertyMember]
        public CustomSizeAspectRatio AspectRatio
        {
            get { return _aspectRatio; }
            set { SetProperty(ref _aspectRatio, value); }
        }

        /// <summary>
        /// 適用率
        /// </summary>
        [PropertyPercent]
        public double ApplicabilityRate
        {
            get { return _applicabilityRate; }
            set { SetProperty(ref _applicabilityRate, MathUtility.Clamp(value, 0.0, 1.0)); }
        }

        /// <summary>
        /// 長辺をそろえる
        /// </summary>
        [PropertyMember]
        public bool IsAlignLongSide
        {
            get { return _isAlignLongSide; }
            set { SetProperty(ref _isAlignLongSide, value); }
        }

        #region Obsolete

        /// <summary>
        /// 縦横比を固定する
        /// </summary>
        [Obsolete("no used"), Alternative(nameof(AspectRatio), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsUniformed
        {
            get { return false; }
            set { AspectRatio = value ? CustomSizeAspectRatio.Origin : CustomSizeAspectRatio.None; }
        }

        #endregion

        /// <summary>
        /// ハッシュ値の計算
        /// </summary>
        /// <returns></returns>
        public int GetEnvironmentHashCode()
        {
            return HashCode.Combine(_isEnabled, _size, _aspectRatio, _applicabilityRate, _isAlignLongSide);
        }
    }

    public enum CustomSizeAspectRatio
    {
        None,
        Origin,
        Ratio_1_1,
        Ratio_2_3,
        Ratio_4_3,
        Ratio_8_9,
        Ratio_16_9,
        HalfView,
        View,
    }
}
