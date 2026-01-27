using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    // 渦巻き
    public class SwirlEffectUnit : EffectUnit
    {
        private static readonly SwirlEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(-50, 50)]
        [DefaultValue(10)]
        public double TwistAmount
        {
            get { return _effect.TwistAmount; }
            set { if (_effect.TwistAmount != value) { _effect.TwistAmount = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
