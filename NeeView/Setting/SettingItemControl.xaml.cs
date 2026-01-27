using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemControl : UserControl
    {
        public SettingItemControl()
        {
            InitializeComponent();
        }

        public SettingItemControl(object? icon, string header, string? tips, object? content, object? subContent, bool isContentStretch)
        {
            InitializeComponent();

            this.Header.Text = header;
            this.ContentValue.Content = content;

            if (icon != null)
            {
                this.Icon.Content = icon;
                this.Icon.Visibility = Visibility.Visible;
            }

            if (content is null)
            {
                this.ContentValue.Visibility = Visibility.Collapsed;
            }

            if (subContent != null)
            {
                this.SubContent.Content = subContent;
                this.SubContent.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(tips))
            {
                this.Note.Text = tips;
                this.Note.Visibility = Visibility.Visible;
            }

            if (!isContentStretch)
            {
                this.ContentValue.HorizontalAlignment = HorizontalAlignment.Left;
                this.ContentValue.MinWidth = 300;
            }
        }
    }
}
