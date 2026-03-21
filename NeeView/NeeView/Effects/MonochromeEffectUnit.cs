using Generator.Equals;
using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class MonochromeEffectUnit : EffectUnit
    {
        [DefaultEquality] private Color _color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFFFFF")]
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }


    public class MonochromeEffectAdapter : EffectAdapter
    {
        private readonly MonochromeEffect _effect = new();
        private readonly MonochromeEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public MonochromeEffectAdapter(MonochromeEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(MonochromeEffectUnit.Color),
                (s, e) => _effect.Color = _source.Color);

            _source.RaisePropertyChangedAll();
        }
    }
}
