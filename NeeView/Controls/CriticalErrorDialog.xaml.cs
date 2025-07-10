using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
