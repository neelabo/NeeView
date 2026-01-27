using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class BlurEffectUnit : EffectUnit
    {
        private static readonly BlurEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0, 100)]
        [DefaultValue(5.0)]
        public double Radius
        {
            get { return _effect.Radius; }
            set { if (_effect.Radius != value) { _effect.Radius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
