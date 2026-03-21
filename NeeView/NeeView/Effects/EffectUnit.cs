using Generator.Equals;
using NeeLaboratory.ComponentModel;

namespace NeeView.Effects
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class EffectUnit : BindableBase
    {
        public void RaisePropertyChangedAll()
        {
            RaisePropertyChanged(null);
        }
    }
}
