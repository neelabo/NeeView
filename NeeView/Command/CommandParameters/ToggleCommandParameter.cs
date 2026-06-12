using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// トグルコマンドパラメータ基底
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ToggleCommandParameter : CommandParameter
    {
        [DefaultEquality] private ToggleMode _toggleMode;

        /// <summary>
        /// トグルモードのトグル/ON/OFFを選択
        /// </summary>
        /// <remarks>
        /// ショートカットで実行するときに適用。メニューから実行するときは無視されます。
        /// </remarks>
        [PropertyMember]
        public ToggleMode ToggleMode
        {
            get => _toggleMode;
            set => SetProperty(ref _toggleMode, value);
        }
    }

    public enum ToggleMode
    {
        Toggle,
        On,
        Off,
    }
}
