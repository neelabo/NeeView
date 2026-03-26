using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// プレイリスト項目移動用パラメータ
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class MovePlaylistItemInBookCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _isLoop;
        [DefaultEquality] private bool _isIncludeTerminal;

        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        [PropertyMember]
        public bool IsIncludeTerminal
        {
            get => _isIncludeTerminal;
            set => SetProperty(ref _isIncludeTerminal, value);
        }
    }
}
