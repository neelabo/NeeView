using Generator.Equals;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class LevelEffectUnit : EffectUnit, IEquatable<LevelEffectUnit>
    {
        [DefaultEquality] private double _black = 0.0;
        [DefaultEquality] private double _white = 1.0;
        [DefaultEquality] private double _center = 0.5;
        [DefaultEquality] private double _minimum = 0.0;
        [DefaultEquality] private double _maximum = 1.0;


        [PropertyRange(0, 1, Title = "Input")]
        [DefaultValue(0.0)]
        [JsonIgnore]
        public double Black
        {
            get => _black;
            set
            {
                var centerRate = _white - _black != 0 ? (_center - _black) / (_white - _black) : 0.5;
                if (SetProperty(ref _black, AppMath.Round(value)))
                {
                    Center = _black + centerRate * (_white - _black);
                }
            }
        }

        [JsonPropertyName(nameof(Black))]
        [PropertyMapIgnore]
        public double BlackRaw
        {
            get => _black;
            set => _black = value;
        }

        [PropertyRange(0, 1)]
        [DefaultValue(1.0)]
        [JsonIgnore]
        public double White
        {
            get => _white;
            set
            {
                var centerRate = _white - _black != 0 ? (_center - _black) / (_white - _black) : 0.5;
                if (SetProperty(ref _white, AppMath.Round(value)))
                {
                    Center = _black + centerRate * (_white - _black);
                }
            }
        }

        [JsonPropertyName(nameof(White))]
        [PropertyMapIgnore]
        public double WhiteRaw
        {
            get => _white;
            set => _white = value;
        }

        [PropertyRange(0.0, 1.0)]
        [DefaultValue(0.5)]
        public double Center
        {
            get => _center;
            set => SetProperty(ref _center, AppMath.Round(value));
        }

        [PropertyRange(0, 1, Title = "Output")]
        [DefaultValue(0.0)]
        public double Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(1.0)]
        public double Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, AppMath.Round(value));
        }
    }


    public class LevelEffectAdapter : EffectAdapter
    {
        private readonly LevelEffect _effect = new();
        private readonly LevelEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public LevelEffectAdapter(LevelEffectUnit source)
        {
            _source = source;

            _source.SubscribePropertyChanged(nameof(LevelEffectUnit.Black),
                (s, e) => _effect.Black = _source.Black);

            _source.SubscribePropertyChanged(nameof(LevelEffectUnit.White),
                (s, e) => _effect.White = _source.White);

            _source.SubscribePropertyChanged(nameof(LevelEffectUnit.Center),
                (s, e) => _effect.Center = _source.Center);

            _source.SubscribePropertyChanged(nameof(LevelEffectUnit.Minimum),
                (s, e) => _effect.Minimum = _source.Minimum);

            _source.SubscribePropertyChanged(nameof(LevelEffectUnit.Maximum),
                (s, e) => _effect.Maximum = _source.Maximum);

            _source.RaisePropertyChangedAll();
        }
    }
}
