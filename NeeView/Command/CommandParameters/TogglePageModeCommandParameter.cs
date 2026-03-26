using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class TogglePageModeCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _isLoop = true;

        // ループ
        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }
    }

}
