using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class SharpenEffectUnit : EffectUnit
    {
        [DefaultEquality] private double _amount = 2.0;
        [DefaultEquality] private double _height = 0.5;


        [PropertyRange(0, 4)]
        [DefaultValue(2.0)]
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, AppMath.Round(value));
        }

        [PropertyRange(0, 2.0)]
        [DefaultValue(0.5)]
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, AppMath.Round(value));
        }
    }


    public class SharpenEffectAdapter : EffectAdapter
    {
        private readonly SharpenEffect _effect = new();
        private readonly SharpenEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public SharpenEffectAdapter(SharpenEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, SharpenEffect.AmountProperty, new Binding(nameof(SharpenEffectUnit.Amount)) { Source = _source });
            BindingOperations.SetBinding(_effect, SharpenEffect.HeightProperty, new Binding(nameof(SharpenEffectUnit.Height)) { Source = _source, Converter = new DoubleConstMulConverter() { Coefficient = 0.001 } });

            _source.RaisePropertyChangedAll();
        }
    }
}
