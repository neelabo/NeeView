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
    public partial class SwirlEffectUnit : EffectUnit
    {
        [DefaultEquality] private Point _center = new(0.5, 0.5);
        [DefaultEquality] private double _twistAmount = 10.0;

        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get => _center;
            set => SetProperty(ref _center, AppMath.Round(value));
        }

        [PropertyRange(-50, 50)]
        [DefaultValue(10)]
        public double TwistAmount
        {
            get => _twistAmount;
            set => SetProperty(ref _twistAmount, AppMath.Round(value));
        }
    }


    public class SwirlEffectAdapter : EffectAdapter
    {
        private readonly SwirlEffect _effect = new();
        private readonly SwirlEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public SwirlEffectAdapter(SwirlEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, SwirlEffect.CenterProperty, new Binding(nameof(SwirlEffectUnit.Center)) { Source = _source });
            BindingOperations.SetBinding(_effect, SwirlEffect.TwistAmountProperty, new Binding(nameof(SwirlEffectUnit.TwistAmount)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
