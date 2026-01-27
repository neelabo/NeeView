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

        #region Constructors

        private ImageEffect()
        {
            Effects = new Dictionary<EffectType, EffectUnit?>
            {
                [EffectType.None] = null,
                [EffectType.Level] = Config.Current.ImageEffect.LevelEffect,
                [EffectType.Hsv] = Config.Current.ImageEffect.HsvEffect,
                [EffectType.ColorSelect] = Config.Current.ImageEffect.ColorSelectEffect,
                [EffectType.Blur] = Config.Current.ImageEffect.BlurEffect,
                [EffectType.Bloom] = Config.Current.ImageEffect.BloomEffect,
                [EffectType.Monochrome] = Config.Current.ImageEffect.MonochromeEffect,
                [EffectType.ColorTone] = Config.Current.ImageEffect.ColorToneEffect,
                [EffectType.Sharpen] = Config.Current.ImageEffect.SharpenEffect,
                [EffectType.Embossed] = Config.Current.ImageEffect.EmbossedEffect,
                [EffectType.Pixelate] = Config.Current.ImageEffect.PixelateEffect,
                [EffectType.Magnify] = Config.Current.ImageEffect.MagnifyEffect,
                [EffectType.Ripple] = Config.Current.ImageEffect.RippleEffect,
                [EffectType.Swirl] = Config.Current.ImageEffect.SwirlEffect
            };

            Config.Current.ImageEffect.AddPropertyChanged(nameof(ImageEffectConfig.IsEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
            });

            Config.Current.ImageEffect.AddPropertyChanged(nameof(ImageEffectConfig.EffectType), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
                UpdateEffectParameters();
            });

            UpdateEffectParameters();
        }

        #endregion

        #region Properties

        //
        public Dictionary<EffectType, EffectUnit?> Effects { get; private set; }

        /// <summary>
        /// Property: Effect
        /// </summary>
        public Effect? Effect => Config.Current.ImageEffect.IsEnabled ? Effects[Config.Current.ImageEffect.EffectType]?.GetEffect() : null;

        /// <summary>
        /// Property: EffectParameters
        /// </summary>
        private PropertyDocument? _effectParameters;
        public PropertyDocument? EffectParameters
        {
            get { return _effectParameters; }
            set { if (_effectParameters != value) { _effectParameters = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        //
        private void UpdateEffectParameters()
        {
            var effect = Effects[Config.Current.ImageEffect.EffectType];
            if (effect is null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(effect);
            }
        }

        #endregion

    }
}
