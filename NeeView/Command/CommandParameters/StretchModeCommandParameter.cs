using Generator.Equals;
using NeeView.Windows.Property;


namespace NeeView
{
    /// <summary>
    /// スケールモード用設定
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class StretchModeCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _isToggle;

        // 属性に説明文
        [PropertyMember]
        public bool IsToggle
        {
            get => _isToggle;
            set => SetProperty(ref _isToggle, value);
        }
    }
}
