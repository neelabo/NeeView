using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class BloomEffectUnit : EffectUnit
    {
        private static readonly BloomEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseIntensity
        {
            get { return _effect.BaseIntensity; }
            set { if (_effect.BaseIntensity != value) { _effect.BaseIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseSaturation
        {
            get { return _effect.BaseSaturation; }
            set { if (_effect.BaseSaturation != value) { _effect.BaseSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.25)]
        public double BloomIntensity
        {
            get { return _effect.BloomIntensity; }
            set { if (_effect.BloomIntensity != value) { _effect.BloomIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BloomSaturation
        {
            get { return _effect.BloomSaturation; }
            set { if (_effect.BloomSaturation != value) { _effect.BloomSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1.0)]
        [DefaultValue(0.25)]
        public double Threshold
        {
            get { return _effect.Threshold; }
            set
            {
                var a = value < 0.99 ? value : 0.99;
                if (_effect.Threshold != a) { _effect.Threshold = a; RaiseEffectPropertyChanged(); }
            }
        }
    }
}
