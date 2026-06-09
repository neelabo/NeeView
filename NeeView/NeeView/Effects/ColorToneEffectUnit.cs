using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class ColorToneEffectUnit : EffectUnit
    {
        [DefaultEquality] private Color _darkColor = Color.FromArgb(0xFF, 0x33, 0x80, 0x00);
        [DefaultEquality] private Color _lightColor = Color.FromArgb(0xFF, 0xFF, 0xE5, 0x80);
        [DefaultEquality] private double _toneAmount = 0.5;
        [DefaultEquality] private double _desaturation = 0.5;

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF338000")]
        public Color DarkColor
        {
            get => _darkColor;
            set => SetProperty(ref _darkColor, value);
        }

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFE580")]
        public Color LightColor
        {
            get => _lightColor;
            set => SetProperty(ref _lightColor, value);
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double ToneAmount
        {
            get => _toneAmount;
            set => SetProperty(ref _toneAmount, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Desaturation
        {
            get => _desaturation;
            set => SetProperty(ref _desaturation, AppMath.Round(value));
        }
    }


    public class ColorToneEffectAdapter : EffectAdapter
    {
        private readonly ColorToneEffect _effect = new();
        private readonly ColorToneEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public ColorToneEffectAdapter(ColorToneEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, ColorToneEffect.DarkColorProperty, new Binding(nameof(ColorToneEffectUnit.DarkColor)) { Source = _source });
            BindingOperations.SetBinding(_effect, ColorToneEffect.LightColorProperty, new Binding(nameof(ColorToneEffectUnit.LightColor)) { Source = _source });
            BindingOperations.SetBinding(_effect, ColorToneEffect.ToneAmountProperty, new Binding(nameof(ColorToneEffectUnit.ToneAmount)) { Source = _source });
            BindingOperations.SetBinding(_effect, ColorToneEffect.DesaturationProperty, new Binding(nameof(ColorToneEffectUnit.Desaturation)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
