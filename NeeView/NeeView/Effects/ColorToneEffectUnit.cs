using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class ColorToneEffectUnit : EffectUnit
    {
        private static readonly ColorToneEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF338000")]
        public Color DarkColor
        {
            get { return _effect.DarkColor; }
            set { if (_effect.DarkColor != value) { _effect.DarkColor = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFE580")]
        public Color LightColor
        {
            get { return _effect.LightColor; }
            set { if (_effect.LightColor != value) { _effect.LightColor = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double ToneAmount
        {
            get { return _effect.ToneAmount; }
            set { if (_effect.ToneAmount != value) { _effect.ToneAmount = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Desaturation
        {
            get { return _effect.Desaturation; }
            set { if (_effect.Desaturation != value) { _effect.Desaturation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
