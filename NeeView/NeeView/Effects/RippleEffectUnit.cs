using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class RippleEffectUnit : EffectUnit
    {
        private static readonly RippleEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 100)]
        [DefaultValue(40)]
        public double Frequency
        {
            get { return _effect.Frequency; }
            set { if (_effect.Frequency != value) { _effect.Frequency = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.1)]
        public double Magnitude
        {
            get { return _effect.Magnitude; }
            set { if (_effect.Magnitude != value) { _effect.Magnitude = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 100)]
        [DefaultValue(10)]
        public double Phase
        {
            get { return _effect.Phase; }
            set { if (_effect.Phase != value) { _effect.Phase = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
