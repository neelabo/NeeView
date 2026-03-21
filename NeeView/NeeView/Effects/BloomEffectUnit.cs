using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class BloomEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _baseIntensity = 1.0;
        [DefaultEquality] private double _baseSaturation = 1.0;
        [DefaultEquality] private double _bloomIntensity = 1.25;
        [DefaultEquality] private double _bloomSaturation = 1.0;
        [DefaultEquality] private double _threshold = 0.25;

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseIntensity
        {
            get => _baseIntensity;
            set => SetProperty(ref _baseIntensity, value);
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseSaturation
        {
            get => _baseSaturation;
            set => SetProperty(ref _baseSaturation, value);
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.25)]
        public double BloomIntensity
        {
            get => _bloomIntensity;
            set => SetProperty(ref _bloomIntensity, value);
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BloomSaturation
        {
            get => _bloomSaturation;
            set => SetProperty(ref _bloomSaturation, value);
        }

        [PropertyRange(0, 1.0)]
        [DefaultValue(0.25)]
        public double Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, value < 0.99 ? value : 0.99);
        }
    }


    public class BloomEffectAdapter : EffectAdapter
    {
        private readonly BloomEffect _effect = new();
        private readonly BloomEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public BloomEffectAdapter(BloomEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(BloomEffectUnit.BaseIntensity),
                (s, e) => _effect.BaseIntensity = _source.BaseIntensity);

            _source.SubscribePropertyChanged(nameof(BloomEffectUnit.BaseSaturation),
                (s, e) => _effect.BaseSaturation = _source.BaseSaturation);

            _source.SubscribePropertyChanged(nameof(BloomEffectUnit.BloomIntensity),
                (s, e) => _effect.BloomIntensity = _source.BloomIntensity);

            _source.SubscribePropertyChanged(nameof(BloomEffectUnit.BloomSaturation),
                (s, e) => _effect.BloomSaturation = _source.BloomSaturation);

            _source.SubscribePropertyChanged(nameof(BloomEffectUnit.Threshold),
                (s, e) => _effect.Threshold = _source.Threshold);

            _source.RaisePropertyChangedAll();
        }
    }
}
