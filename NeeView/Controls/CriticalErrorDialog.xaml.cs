using NeeView.Properties;
using System.Globalization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// CriticalErrorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CriticalErrorDialog : Window
    {
        public CriticalErrorDialog()
        {
            InitializeComponent();
        }

        public CriticalErrorDialog(string errorLog, string errorLogPath) : this()
        {
            this.ErrorLog.Text = errorLog;
            this.ErrorLogLocate.IsXHtml = true;
            this.ErrorLogLocate.Source = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("CriticalExceptionDialog.LogPath"), System.Security.SecurityElement.Escape(errorLogPath));

            this.Loaded += (s, e) => System.Media.SystemSounds.Hand.Play();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.ErrorLog.Text);
        }
    }
}
