using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class RippleEffectUnit : EffectUnit
    {
        [DefaultEquality] private Point _center = new(0.5, 0.5);
        [DefaultEquality] private double _frequency = 40.0;
        [DefaultEquality] private double _magnitude = 0.1;
        [DefaultEquality] private double _phase = 10.0;

        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get => _center;
            set => SetProperty(ref _center, value);
        }

        [PropertyRange(0, 100)]
        [DefaultValue(40)]
        public double Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.1)]
        public double Magnitude
        {
            get => _magnitude;
            set => SetProperty(ref _magnitude, value);
        }

        [PropertyRange(0, 100)]
        [DefaultValue(10)]
        public double Phase
        {
            get => _phase;
            set => SetProperty(ref _phase, value);
        }
    }


    public class RippleEffectAdapter : EffectAdapter
    {
        private readonly RippleEffect _effect = new();
        private readonly RippleEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public RippleEffectAdapter(RippleEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(RippleEffectUnit.Center),
                (s, e) => _effect.Center = _source.Center);

            _source.SubscribePropertyChanged(nameof(RippleEffectUnit.Frequency),
                (s, e) => _effect.Frequency = _source.Frequency);

            _source.SubscribePropertyChanged(nameof(RippleEffectUnit.Magnitude),
                (s, e) => _effect.Magnitude = _source.Magnitude);

            _source.SubscribePropertyChanged(nameof(RippleEffectUnit.Phase),
                (s, e) => _effect.Phase = _source.Phase);

            _source.RaisePropertyChangedAll();
        }
    }
}
