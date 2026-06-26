using NeeView.Effects;
using System;


namespace NeeView
{
    public static class EffectUnitExtensions
    {
        public static EffectType ToEffectType(this EffectUnit? unit)
        {
            return unit switch
            {
                null => EffectType.None,
                LevelEffectUnit => EffectType.Level,
                HsvEffectUnit => EffectType.Hsv,
                ColorSelectEffectUnit => EffectType.ColorSelect,
                BlurEffectUnit => EffectType.Blur,
                BloomEffectUnit => EffectType.Bloom,
                MonochromeEffectUnit => EffectType.Monochrome,
                ColorToneEffectUnit => EffectType.ColorTone,
                ColorizeEffectUnit => EffectType.Colorize,
                SharpenEffectUnit => EffectType.Sharpen,
                EmbossedEffectUnit => EffectType.Embossed,
                PixelateEffectUnit => EffectType.Pixelate,
                MagnifyEffectUnit => EffectType.Magnify,
                RippleEffectUnit => EffectType.Ripple,
                SwirlEffectUnit => EffectType.Swirl,
                _ => throw new NotSupportedException($"Not support effect: {unit.GetType().FullName}")
            };
        }


        public static EffectAdapter? CreateEffectAdapter(this EffectUnit? unit)
        {
            return unit switch
            {
                null => null,
                LevelEffectUnit e => new LevelEffectAdapter(e),
                HsvEffectUnit e => new HsvEffectAdapter(e),
                ColorSelectEffectUnit e => new ColorSelectEffectAdapter(e),
                BlurEffectUnit e => new BlurEffectAdapter(e),
                BloomEffectUnit e => new BloomEffectAdapter(e),
                MonochromeEffectUnit e => new MonochromeEffectAdapter(e),
                ColorToneEffectUnit e => new ColorToneEffectAdapter(e),
                ColorizeEffectUnit e => new ColorizeEffectAdapter(e),
                SharpenEffectUnit e => new SharpenEffectAdapter(e),
                EmbossedEffectUnit e => new EmbossedEffectAdapter(e),
                PixelateEffectUnit e => new PixelateEffectAdapter(e),
                MagnifyEffectUnit e => new MagnifyEffectAdapter(e),
                RippleEffectUnit e => new RippleEffectAdapter(e),
                SwirlEffectUnit e => new SwirlEffectAdapter(e),
                _ => throw new NotSupportedException($"Not support effect: {unit.GetType().FullName}")
            };
        }

    }
}