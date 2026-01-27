using System.Collections.Generic;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseGestureSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseGestureSettingControl : UserControl
    {
        private MouseGestureSettingViewModel? _vm;

        public MouseGestureSettingControl()
        {
            InitializeComponent();
        }

        public void Initialize(IReadOnlyDictionary<string, CommandElement> commandMap, string key)
        {
            _vm = new MouseGestureSettingViewModel(commandMap, key, this.GestureBox);
            this.DataContext = _vm;
        }

        public void Flush()
        {
            _vm?.Flush();
        }
    }
}
