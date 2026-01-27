using NeeView.Windows.Property;


namespace NeeView
{
    /// <summary>
    /// スケールモード用設定
    /// </summary>
    public class StretchModeCommandParameter : CommandParameter
    {
        private bool _isToggle;

        // 属性に説明文
        [PropertyMember]
        public bool IsToggle
        {
            get => _isToggle;
            set => SetProperty(ref _isToggle, value);
        }
    }
}
