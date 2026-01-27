using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// BackgroundSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BackgroundSettingControl : UserControl, IValueInitializable
    {
        private readonly BrushSource? _source;

        public BackgroundSettingControl()
        {
            InitializeComponent();
        }

        public BackgroundSettingControl(BrushSource source)
        {
            InitializeComponent();

            _source = source;
            this.DataContext = _source;
        }

        public void InitializeValue()
        {
            _source?.Reset();
        }
    }
}
