using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ImageEffectConfig : ObservableObject
    {
        [DefaultEquality] private bool _isEnabled;
        [DefaultEquality] private bool _isHsvMode;
        [DefaultEquality] private EffectLayerCollection _layers = new EffectLayerCollection() { new EffectLayer() };

        /// <summary>
        /// エフェクト有効
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 色をHSV表示
        /// </summary>
        [PropertyMember]
        public bool IsHsvMode
        {
            get { return _isHsvMode; }
            set { SetProperty(ref _isHsvMode, value); }
        }

        /// <summary>
        /// エフェクトレイヤー
        /// </summary>
        [PropertyMapIgnore]
        public EffectLayerCollection Layers
        {
            get { return _layers; }
            set { SetProperty(ref _layers, value); }
        }

        /// <summary>
        /// エフェクトキャッシュ
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public EffectUnitCache Caches { get; set; } = new();


        #region Obsolete

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore]
        public EffectType EffectType
        {
            get { return Layers[0].EffectType; }
            set { Layers[0].EffectType = value; }
        }

        [Obsolete]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        [JsonPropertyName("EffectType")]
        public EffectType EffectTypeLegacy { get; set; }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public LevelEffectUnit LevelEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public HsvEffectUnit HsvEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public ColorSelectEffectUnit ColorSelectEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public BlurEffectUnit BlurEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public BloomEffectUnit BloomEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public MonochromeEffectUnit MonochromeEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public ColorToneEffectUnit ColorToneEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public SharpenEffectUnit SharpenEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public EmbossedEffectUnit EmbossedEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public PixelateEffectUnit PixelateEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public MagnifyEffectUnit MagnifyEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public RippleEffectUnit RippleEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public SwirlEffectUnit SwirlEffect
        {
            get => new();
            set => Caches.Add(value);
        }

        #endregion
    }
}
