using Generator.Equals;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class BlurEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _radius = 5.0;

        [PropertyRange(0, 100)]
        [DefaultValue(5.0)]
        public double Radius
        {
            get => _radius;
            set => SetProperty(ref _radius, AppMath.Round(value));
        }
    }


    public class BlurEffectAdapter : EffectAdapter
    {
        private readonly BlurEffect _effect = new();
        private readonly BlurEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public BlurEffectAdapter(BlurEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(BlurEffectUnit.Radius),
                (s, e) => _effect.Radius = _source.Radius);

            _source.RaisePropertyChangedAll();
        }
    }
}
