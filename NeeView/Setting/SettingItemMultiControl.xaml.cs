using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// object content.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemMultiControl : UserControl
    {
        public SettingItemMultiControl()
        {
            InitializeComponent();
        }

        public SettingItemMultiControl(string header, string? tips, object content1, object content2)
        {
            InitializeComponent();

            this.Header.Text = header;
            this.ContentValue1.Content = content1;
            this.ContentValue2.Content = content2;

            if (!string.IsNullOrWhiteSpace(tips))
            {
                this.Note.Text = tips;
                this.Note.Visibility = Visibility.Visible;
            }
        }
    }
}
