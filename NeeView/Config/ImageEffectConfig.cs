using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageEffectConfig : BindableBase
    {
        private bool _isEnabled;
        private EffectType _effectType = EffectType.Level;
        private bool _isHsvMode;

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
        /// 適用するエフェクトの種類
        /// </summary>
        [PropertyMember]
        public EffectType EffectType
        {
            get { return _effectType; }
            set { SetProperty(ref _effectType, value); }
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

        [PropertyMapLabel("EffectType.Level")]
        public LevelEffectUnit LevelEffect { get; set; } = new LevelEffectUnit();

        [PropertyMapLabel("EffectType.Hsv")]
        public HsvEffectUnit HsvEffect { get; set; } = new HsvEffectUnit();

        [PropertyMapLabel("EffectType.ColorSelect")]
        public ColorSelectEffectUnit ColorSelectEffect { get; set; } = new ColorSelectEffectUnit();

        [PropertyMapLabel("EffectType.Blur")]
        public BlurEffectUnit BlurEffect { get; set; } = new BlurEffectUnit();

        [PropertyMapLabel("EffectType.Bloom")]
        public BloomEffectUnit BloomEffect { get; set; } = new BloomEffectUnit();

        [PropertyMapLabel("EffectType.Monochrome")]
        public MonochromeEffectUnit MonochromeEffect { get; set; } = new MonochromeEffectUnit();

        [PropertyMapLabel("EffectType.ColorTone")]
        public ColorToneEffectUnit ColorToneEffect { get; set; } = new ColorToneEffectUnit();

        [PropertyMapLabel("EffectType.Sharpen")]
        public SharpenEffectUnit SharpenEffect { get; set; } = new SharpenEffectUnit();

        [PropertyMapLabel("EffectType.Embossed")]
        public EmbossedEffectUnit EmbossedEffect { get; set; } = new EmbossedEffectUnit();

        [PropertyMapLabel("EffectType.Pixelate")]
        public PixelateEffectUnit PixelateEffect { get; set; } = new PixelateEffectUnit();

        [PropertyMapLabel("EffectType.Magnify")]
        public MagnifyEffectUnit MagnifyEffect { get; set; } = new MagnifyEffectUnit();

        [PropertyMapLabel("EffectType.Ripple")]
        public RippleEffectUnit RippleEffect { get; set; } = new RippleEffectUnit();

        [PropertyMapLabel("EffectType.Swirl")]
        public SwirlEffectUnit SwirlEffect { get; set; } = new SwirlEffectUnit();
    }
}
