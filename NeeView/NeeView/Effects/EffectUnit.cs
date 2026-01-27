using NeeLaboratory.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class EffectUnit : BindableBase
    {
        public virtual Effect? GetEffect() => null;

        protected void RaiseEffectPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            RaisePropertyChanged(name);
            RaisePropertyChanged(nameof(Effect));
        }
    }
}
