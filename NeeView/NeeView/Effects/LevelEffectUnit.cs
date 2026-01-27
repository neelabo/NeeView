using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class LevelEffectUnit : EffectUnit
    {
        private static readonly LevelEffect _effect = new();

        public override Effect GetEffect() => _effect;


        [PropertyRange(0, 1, Title = "Input")]
        [DefaultValue(0.0)]
        public double Black
        {
            get { return _effect.Black; }
            set { if (_effect.Black != value) { _effect.Black = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(1.0)]
        public double White
        {
            get { return _effect.White; }
            set { if (_effect.White != value) { _effect.White = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0.1, 0.9)]
        [DefaultValue(0.5)]
        public double Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1, Title = "Output")]
        [DefaultValue(0.0)]
        public double Minimum
        {
            get { return _effect.Minimum; }
            set { if (_effect.Minimum != value) { _effect.Minimum = value; RaiseEffectPropertyChanged(); } }
        }

        [PropertyRange(0, 1)]
        [DefaultValue(1.0)]
        public double Maximum
        {
            get { return _effect.Maximum; }
            set { if (_effect.Maximum != value) { _effect.Maximum = value; RaiseEffectPropertyChanged(); } }
        }

    }
}
