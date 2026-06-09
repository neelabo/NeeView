using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Windows.Data;
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
            set => SetProperty(ref _baseIntensity, AppMath.Round(value));
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseSaturation
        {
            get => _baseSaturation;
            set => SetProperty(ref _baseSaturation, AppMath.Round(value));
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.25)]
        public double BloomIntensity
        {
            get => _bloomIntensity;
            set => SetProperty(ref _bloomIntensity, AppMath.Round(value));
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BloomSaturation
        {
            get => _bloomSaturation;
            set => SetProperty(ref _bloomSaturation, AppMath.Round(value));
        }

        [PropertyRange(0, 1.0)]
        [DefaultValue(0.25)]
        public double Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, AppMath.Round(Math.Min(value, 1.0)));
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

            BindingOperations.SetBinding(_effect, BloomEffect.BaseIntensityProperty, new Binding(nameof(BloomEffectUnit.BaseIntensity)) { Source = _source });
            BindingOperations.SetBinding(_effect, BloomEffect.BaseSaturationProperty, new Binding(nameof(BloomEffectUnit.BaseSaturation)) { Source = _source });
            BindingOperations.SetBinding(_effect, BloomEffect.BloomIntensityProperty, new Binding(nameof(BloomEffectUnit.BloomIntensity)) { Source = _source });
            BindingOperations.SetBinding(_effect, BloomEffect.BloomSaturationProperty, new Binding(nameof(BloomEffectUnit.BloomSaturation)) { Source = _source });
            BindingOperations.SetBinding(_effect, BloomEffect.ThresholdProperty, new Binding(nameof(BloomEffectUnit.Threshold)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
