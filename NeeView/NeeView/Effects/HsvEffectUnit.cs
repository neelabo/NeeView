using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class HsvEffectUnit : EffectUnit
    {
        private static readonly HsvEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0.0, 360.0)]
        [DefaultValue(0.0)]
        public double Hue
        {
            get { return _effect.Hue; }
            set { if (_effect.Hue != value) { _effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get { return _effect.Saturation; }
            set { if (_effect.Saturation != value) { _effect.Saturation = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Value
        {
            get { return _effect.Value; }
            set { if (_effect.Value != value) { _effect.Value = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
