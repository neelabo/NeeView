using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// ProgressMessageDialog.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class ProgressMessageDialog : Window, INotifyPropertyChanged
    {
        private bool _closeable = true;
        private bool _canceled = false;
        private string _caption = "";
        private string _message = "";

        public ProgressMessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ProgressMessageDialog(string caption) : this()
        {
            this.CaptionTextBlock.Text = caption;
        }

        public ProgressMessageDialog(string caption, string message) : this()
        {
            this.CaptionTextBlock.Text = caption;
            this.MessageTextBlock.Text = message;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? Canceled;


        public bool Closeable
        {
            get { return _closeable; }
            set { SetProperty(ref _closeable, value); }
        }

        public string Caption
        {
            get { return _caption; }
            set { SetProperty(ref _caption, value); }
        }

        public string Message
        {
            get { return _message; }
            set { if (!_canceled) { SetProperty(ref _message, value); } }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_closeable)
            {
                Cancel();
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            if (!_canceled)
            {
                _canceled = true;
                this.CancelButton.IsEnabled = false;
                this.MessageTextBlock.Text = TextResources.GetString("Word.Canceling");
                Canceled?.Invoke(this, EventArgs.Empty);
            }
        }

        public new void Close()
        {
            _closeable = true;
            base.Close();
        }
    }
}
