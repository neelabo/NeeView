using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class ColorSelectEffectUnit : EffectUnit
    {
        private static readonly ColorSelectEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0.0, 360.0)]
        [DefaultValue(15.0)]
        public double Hue
        {
            get { return _effect.Hue; }
            set { if (_effect.Hue != value) { _effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0.0, 1.0)]
        [DefaultValue(0.1)]
        public double Range
        {
            get { return _effect.Range; }
            set { if (_effect.Range != value) { _effect.Range = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0.0, 0.2)]
        [DefaultValue(0.1)]
        public double Curve
        {
            get { return _effect.Curve; }
            set { if (_effect.Curve != value) { _effect.Curve = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
