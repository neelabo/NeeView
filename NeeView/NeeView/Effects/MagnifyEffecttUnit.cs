using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class MagnifyEffectUnit : EffectUnit
    {
        private static readonly MagnifyEffect _effect = new();
        public override Effect GetEffect() => _effect;

        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Amount
        {
            get { return _effect.Amount; }
            set { if (_effect.Amount != value) { _effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.2)]
        public double InnerRadius
        {
            get { return _effect.InnerRadius; }
            set { if (_effect.InnerRadius != value) { _effect.InnerRadius = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.4)]
        public double OuterRadius
        {
            get { return _effect.OuterRadius; }
            set { if (_effect.OuterRadius != value) { _effect.OuterRadius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
