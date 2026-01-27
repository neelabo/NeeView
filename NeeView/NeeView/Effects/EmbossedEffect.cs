using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class EmbossedEffectUnit : EffectUnit
    {
        private static readonly EmbossedEffect _effect = new();

        public override Effect GetEffect() => _effect;

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF808080")]
        public Color Color
        {
            get { return _effect.Color; }
            set { if (_effect.Color != value) { _effect.Color = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(-5, 5)]
        [DefaultValue(3)]
        public double Amount
        {
            get { return _effect.Amount; }
            set { if (_effect.Amount != value) { _effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 5)]
        [DefaultValue(1)]
        public double Height
        {
            get { return _effect.Height * 1000.0; }
            set { var a = value * 0.001; if (_effect.Height != a) { _effect.Height = a; RaiseEffectPropertyChanged(); } }
        }
    }
}
