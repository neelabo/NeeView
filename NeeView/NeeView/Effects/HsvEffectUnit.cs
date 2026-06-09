using Generator.Equals;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class HsvEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _hue;
        [DefaultEquality] private double _saturation;
        [DefaultEquality] private double _value;

        [PropertyRange(0.0, 360.0)]
        [DefaultValue(0.0)]
        public double Hue
        {
            get => _hue;
            set => SetProperty(ref _hue, AppMath.Round(value));
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, AppMath.Round(value));
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, AppMath.Round(value));
        }
    }


    public class HsvEffectAdapter : EffectAdapter
    {
        private readonly HsvEffect _effect = new();
        private readonly HsvEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public HsvEffectAdapter(HsvEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, HsvEffect.HueProperty, new Binding(nameof(HsvEffectUnit.Hue)) { Source = _source });
            BindingOperations.SetBinding(_effect, HsvEffect.SaturationProperty, new Binding(nameof(HsvEffectUnit.Saturation)) { Source = _source });
            BindingOperations.SetBinding(_effect, HsvEffect.ValueProperty, new Binding(nameof(HsvEffectUnit.Value)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
