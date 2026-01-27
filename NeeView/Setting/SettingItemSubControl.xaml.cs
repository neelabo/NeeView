using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemSubControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemSubControl : UserControl
    {
        // TODO: ひとまずチェックボックにのみ対応している。

        public SettingItemSubControl()
        {
            InitializeComponent();
        }

        public SettingItemSubControl(string header, string? tips, object content, bool isContentStretch)
        {
            InitializeComponent();

            this.ContentValue.Content = content;

            if (!string.IsNullOrWhiteSpace(tips))
            {
                this.ToolTip = tips;
            }

            if (!isContentStretch)
            {
                this.ContentValue.HorizontalAlignment = HorizontalAlignment.Left;
                this.ContentValue.MinWidth = 300;
            }
        }
    }
}
