using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class PixelateEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _pixelation = 0.75;

        [PropertyRange(0, 1)]
        [DefaultValue(0.75)]
        public double Pixelation
        {
            get => _pixelation;
            set => SetProperty(ref _pixelation, value);
        }
    }


    public class PixelateEffectAdapter : EffectAdapter
    {
        private readonly PixelateEffect _effect = new();
        private readonly PixelateEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public PixelateEffectAdapter(PixelateEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(PixelateEffectUnit.Pixelation),
                (s, e) => _effect.Pixelation = _source.Pixelation);

            _source.RaisePropertyChangedAll();
        }
    }



}
