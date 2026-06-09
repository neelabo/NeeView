using NeeView.Effects;
using System;


namespace NeeView
{
    public static class EffectTypeExtensions
    {
        public static Type? ToType(this EffectType effectType)
        {
            return effectType switch
            {
                EffectType.None => null,
                EffectType.Level => typeof(LevelEffectUnit),
                EffectType.Hsv => typeof(HsvEffectUnit),
                EffectType.ColorSelect => typeof(ColorSelectEffectUnit),
                EffectType.Blur => typeof(BlurEffectUnit),
                EffectType.Bloom => typeof(BloomEffectUnit),
                EffectType.Monochrome => typeof(MonochromeEffectUnit),
                EffectType.ColorTone => typeof(ColorToneEffectUnit),
                EffectType.Sharpen => typeof(SharpenEffectUnit),
                EffectType.Embossed => typeof(EmbossedEffectUnit),
                EffectType.Pixelate => typeof(PixelateEffectUnit),
                EffectType.Magnify => typeof(MagnifyEffectUnit),
                EffectType.Ripple => typeof(RippleEffectUnit),
                EffectType.Swirl => typeof(SwirlEffectUnit),
                _ => throw new NotSupportedException($"Not support effect type: {effectType.ToString()}")
            };
        }
    }
}