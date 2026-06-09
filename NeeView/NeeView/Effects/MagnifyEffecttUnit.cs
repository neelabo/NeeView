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
    public partial class MagnifyEffectUnit : EffectUnit
    {
        [DefaultEquality] private Point _center = new(0.5, 0.5);
        [DefaultEquality] private double _amount = 0.5;
        [DefaultEquality] private double _innerRadius = 0.2;
        [DefaultEquality] private double _outerRadius = 0.4;


        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get => _center;
            set => SetProperty(ref _center, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.2)]
        public double InnerRadius
        {
            get => _innerRadius;
            set => SetProperty(ref _innerRadius, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.4)]
        public double OuterRadius
        {
            get => _outerRadius;
            set => SetProperty(ref _outerRadius, AppMath.Round(value));
        }
    }



    public class MagnifyEffectAdapter : EffectAdapter
    {
        private readonly MagnifyEffect _effect = new();
        private readonly MagnifyEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public MagnifyEffectAdapter(MagnifyEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, MagnifyEffect.CenterProperty, new Binding(nameof(MagnifyEffectUnit.Center)) { Source = _source });
            BindingOperations.SetBinding(_effect, MagnifyEffect.AmountProperty, new Binding(nameof(MagnifyEffectUnit.Amount)) { Source = _source });
            BindingOperations.SetBinding(_effect, MagnifyEffect.InnerRadiusProperty, new Binding(nameof(MagnifyEffectUnit.InnerRadius)) { Source = _source });
            BindingOperations.SetBinding(_effect, MagnifyEffect.OuterRadiusProperty, new Binding(nameof(MagnifyEffectUnit.OuterRadius)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}
