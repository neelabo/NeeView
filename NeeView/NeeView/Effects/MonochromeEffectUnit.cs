using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class MonochromeEffectUnit : EffectUnit
    {
        private static readonly MonochromeEffect _effect = new();
        public override Effect GetEffect() => _effect;

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFFFFF")]
        public Color Color
        {
            get { return _effect.Color; }
            set { if (_effect.Color != value) { _effect.Color = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
