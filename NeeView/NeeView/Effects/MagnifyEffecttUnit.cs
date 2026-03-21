using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;
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
            set => SetProperty(ref _center, value);
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.2)]
        public double InnerRadius
        {
            get => _innerRadius;
            set => SetProperty(ref _innerRadius, value);
        }

        [PropertyRange(0, 1)]
        [DefaultValue(0.4)]
        public double OuterRadius
        {
            get => _outerRadius;
            set => SetProperty(ref _outerRadius, value);
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

            _source.SubscribePropertyChanged(nameof(MagnifyEffectUnit.Center),
                (s, e) => _effect.Center = _source.Center);

            _source.SubscribePropertyChanged(nameof(MagnifyEffectUnit.Amount),
                (s, e) => _effect.Amount = _source.Amount);

            _source.SubscribePropertyChanged(nameof(MagnifyEffectUnit.InnerRadius),
                (s, e) => _effect.InnerRadius = _source.InnerRadius);

            _source.SubscribePropertyChanged(nameof(MagnifyEffectUnit.OuterRadius),
                (s, e) => _effect.OuterRadius = _source.OuterRadius);

            _source.RaisePropertyChangedAll();
        }
    }
}
