using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public abstract class EffectAdapter
    {
        public abstract Effect Effect { get; }
        public abstract EffectUnit Source { get; }
    }
}
