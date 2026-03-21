using Generator.Equals;
using NeeView.Windows.Property;
using System.ComponentModel;
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
            set => SetProperty(ref _hue, value);
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, value);
        }

        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
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

            _source.SubscribePropertyChanged(nameof(HsvEffectUnit.Hue),
                (s, e) => _effect.Hue = _source.Hue);

            _source.SubscribePropertyChanged(nameof(HsvEffectUnit.Saturation),
                (s, e) => _effect.Saturation = _source.Saturation);

            _source.SubscribePropertyChanged(nameof(HsvEffectUnit.Value),
                (s, e) => _effect.Value = _source.Value);

            _source.RaisePropertyChangedAll();
        }
    }
}
