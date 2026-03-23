using Generator.Equals;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit =true)]
    public partial class ColorSelectEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _hue = 15.0;
        [DefaultEquality] private double _range = 0.1;
        [DefaultEquality] private double _curve = 0.1;

        [PropertyRange(0.0, 360.0)]
        [DefaultValue(15.0)]
        public double Hue
        {
            get => _hue;
            set => SetProperty(ref _hue, AppMath.Round(value));
        }

        [PropertyRange(0.0, 1.0)]
        [DefaultValue(0.1)]
        public double Range
        {
            get => _range;
            set => SetProperty(ref _range, AppMath.Round(value));
        }

        [PropertyRange(0.0, 0.2)]
        [DefaultValue(0.1)]
        public double Curve
        {
            get => _curve;
            set => SetProperty(ref _curve, AppMath.Round(value));
        }
    }


    public class ColorSelectEffectAdapter : EffectAdapter
    {
        private readonly ColorSelectEffect _effect = new();
        private readonly ColorSelectEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public ColorSelectEffectAdapter(ColorSelectEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(ColorSelectEffectUnit.Hue),
                (s, e) => _effect.Hue = _source.Hue);

            _source.SubscribePropertyChanged(nameof(ColorSelectEffectUnit.Range),
                (s, e) => _effect.Range = _source.Range);

            _source.SubscribePropertyChanged(nameof(ColorSelectEffectUnit.Curve),
                (s, e) => _effect.Curve = _source.Curve);

            _source.RaisePropertyChangedAll();
        }
    }
}
