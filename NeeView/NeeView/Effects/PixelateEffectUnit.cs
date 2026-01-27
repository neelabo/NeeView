using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class PixelateEffectUnit : EffectUnit
    {
        private static readonly PixelateEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0, 1)]
        [DefaultValue(0.75)]
        public double Pixelation
        {
            get { return _effect.Pixelation; }
            set { if (_effect.Pixelation != value) { _effect.Pixelation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
