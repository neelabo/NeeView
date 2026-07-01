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
        [OrderedEquality] private EffectLayerCollection _layers = new EffectLayerCollection() { new EffectLayer() };

        private EffectUnitCache? _cache;


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
        /// エフェクトレイヤー
        /// </summary>
        [PropertyMapIgnore]
        public EffectLayerCollection Layers
        {
            get { return _layers; }
            set { SetProperty(ref _layers, value); }
        }

        #region Obsolete

        [Obsolete, Alternative(null, 46, ErrorLevel = ScriptErrorLevel.Warning)]
        [JsonIgnore]
        public bool IsHsvMode { get; set; }

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
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public HsvEffectUnit HsvEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public ColorSelectEffectUnit ColorSelectEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public BlurEffectUnit BlurEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public BloomEffectUnit BloomEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public MonochromeEffectUnit MonochromeEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public ColorToneEffectUnit ColorToneEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public SharpenEffectUnit SharpenEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public EmbossedEffectUnit EmbossedEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public PixelateEffectUnit PixelateEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public MagnifyEffectUnit MagnifyEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public RippleEffectUnit RippleEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        [Obsolete, Alternative("nv.ImageEffect", 46, ErrorLevel = ScriptErrorLevel.Warning, IsFullName = true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public SwirlEffectUnit SwirlEffect
        {
            get => new();
            set => _cache?.Add(value);
        }

        internal void SetCacheSource(EffectUnitCache cache)
        {
            _cache = cache;
        }

        #endregion
    }

}
