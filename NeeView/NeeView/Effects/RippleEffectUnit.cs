using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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
            set => SetProperty(ref _center, AppMath.Round(value));
        }

        [PropertyRange(0, 100)]
        [DefaultValue(40)]
        public double Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.1)]
        public double Magnitude
        {
            get => _magnitude;
            set => SetProperty(ref _magnitude, AppMath.Round(value));
        }

        [PropertyRange(0, 100)]
        [DefaultValue(10)]
        public double Phase
        {
            get => _phase;
            set => SetProperty(ref _phase, AppMath.Round(value));
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

            BindingOperations.SetBinding(_effect, RippleEffect.CenterProperty, new Binding(nameof(RippleEffectUnit.Center)) { Source = _source });
            BindingOperations.SetBinding(_effect, RippleEffect.FrequencyProperty, new Binding(nameof(RippleEffectUnit.Frequency)) { Source = _source });
            BindingOperations.SetBinding(_effect, RippleEffect.MagnitudeProperty, new Binding(nameof(RippleEffectUnit.Magnitude)) { Source = _source });
            BindingOperations.SetBinding(_effect, RippleEffect.PhaseProperty, new Binding(nameof(RippleEffectUnit.Phase)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
