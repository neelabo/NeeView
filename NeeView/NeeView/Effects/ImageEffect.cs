using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    /// <summary>
    /// 画像エフェクト
    /// </summary>
    public class ImageEffect : BindableBase
    {
        static ImageEffect() => Current = new ImageEffect();
        public static ImageEffect Current { get; }


        private PropertyDocument? _effectParameters;


        private ImageEffect()
        {
            Effects = new Dictionary<EffectType, EffectAdapter?>
            {
                [EffectType.None] = null,
                [EffectType.Level] = new LevelEffectAdapter(Config.Current.ImageEffect.LevelEffect),
                [EffectType.Hsv] = new HsvEffectAdapter(Config.Current.ImageEffect.HsvEffect),
                [EffectType.ColorSelect] = new ColorSelectEffectAdapter(Config.Current.ImageEffect.ColorSelectEffect),
                [EffectType.Blur] = new BlurEffectAdapter(Config.Current.ImageEffect.BlurEffect),
                [EffectType.Bloom] = new BloomEffectAdapter(Config.Current.ImageEffect.BloomEffect),
                [EffectType.Monochrome] = new MonochromeEffectAdapter(Config.Current.ImageEffect.MonochromeEffect),
                [EffectType.ColorTone] = new ColorToneEffectAdapter(Config.Current.ImageEffect.ColorToneEffect),
                [EffectType.Sharpen] = new SharpenEffectAdapter(Config.Current.ImageEffect.SharpenEffect),
                [EffectType.Embossed] = new EmbossedEffectAdapter(Config.Current.ImageEffect.EmbossedEffect),
                [EffectType.Pixelate] = new PixelateEffectAdapter(Config.Current.ImageEffect.PixelateEffect),
                [EffectType.Magnify] = new MagnifyEffectAdapter(Config.Current.ImageEffect.MagnifyEffect),
                [EffectType.Ripple] = new RippleEffectAdapter(Config.Current.ImageEffect.RippleEffect),
                [EffectType.Swirl] = new SwirlEffectAdapter(Config.Current.ImageEffect.SwirlEffect)
            };

            Config.Current.ImageEffect.SubscribePropertyChanged(nameof(ImageEffectConfig.IsEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
            });

            Config.Current.ImageEffect.SubscribePropertyChanged(nameof(ImageEffectConfig.EffectType), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
                UpdateEffectParameters();
            });

            UpdateEffectParameters();
        }


        public Dictionary<EffectType, EffectAdapter?> Effects { get; private set; }

        public Effect? Effect => Config.Current.ImageEffect.IsEnabled ? Effects[Config.Current.ImageEffect.EffectType]?.Effect : null;


        public PropertyDocument? EffectParameters
        {
            get { return _effectParameters; }
            set { if (_effectParameters != value) { _effectParameters = value; RaisePropertyChanged(); } }
        }


        private void UpdateEffectParameters()
        {
            var effect = Effects[Config.Current.ImageEffect.EffectType];
            if (effect is null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(effect.Source);
            }
        }
    }
}
