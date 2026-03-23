using Generator.Equals;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
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
        public double Black
        {
            get => _black;
            set => SetProperty(ref _black, AppMath.Round(value));
        }

        [PropertyRange(0, 1)]
        [DefaultValue(1.0)]
        public double White
        {
            get => _white;
            set => SetProperty(ref _white, AppMath.Round(value));
        }

        [PropertyRange(0.1, 0.9)]
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

#if false
        #region Equals

        public override bool Equals(object? obj)
        {
            return obj is LevelEffectUnit other && Equals(other);
        }

        public virtual bool Equals(LevelEffectUnit? other)
        {
            if (other is null) return false;

            return this.Black.Equals(other.Black)
                && this.White.Equals(other.White)
                && this.Center.Equals(other.Center)
                && this.Minimum.Equals(other.Minimum)
                && this.Maximum.Equals(other.Maximum);
        }

        public override int GetHashCode()
        {
            HashCode hashcode = new();
            hashcode.Add(this.Black.GetHashCode());
            hashcode.Add(this.White.GetHashCode());
            hashcode.Add(this.Center.GetHashCode());
            hashcode.Add(this.Minimum.GetHashCode());
            hashcode.Add(this.Maximum.GetHashCode());

            return hashcode.ToHashCode();
        }

        #endregion Equals
#endif

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
