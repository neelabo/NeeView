using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class EmbossedEffectUnit : EffectUnit
    {
        [DefaultEquality] private Color _color = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
        [DefaultEquality] private double _amount = 3.0;
        [DefaultEquality] private double _height = 1.0;

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF808080")]
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        [PropertyRange(-5, 5)]
        [DefaultValue(3)]
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        [PropertyRange(0, 5)]
        [DefaultValue(1)]
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
    }


    public class EmbossedEffectAdapter : EffectAdapter
    {
        private readonly EmbossedEffect _effect = new();
        private readonly EmbossedEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public EmbossedEffectAdapter(EmbossedEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(EmbossedEffectUnit.Color),
                (s, e) => _effect.Color = _source.Color);

            _source.SubscribePropertyChanged(nameof(EmbossedEffectUnit.Amount),
                (s, e) => _effect.Amount = _source.Amount);

            _source.SubscribePropertyChanged(nameof(EmbossedEffectUnit.Height),
                (s, e) => _effect.Height = _source.Height * 0.001);

            _source.RaisePropertyChangedAll();
        }
    }
}
