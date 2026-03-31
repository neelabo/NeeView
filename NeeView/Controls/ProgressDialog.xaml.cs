using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ProgressDialog : Window, INotifyPropertyChanged, IDisposable
    {
        private readonly Progress<ProgressInfo> _progress = new();
        private string _caption = "";
        private string _message = "";
        private double _progressValue;
        private bool _canCancel;
        private bool _disposedValue;

        public ProgressDialog()
        {
            this.Topmost = true;
            this.Owner = MainWindow.Current; // TODO: 呼び出したウィンドウをOwnerにする
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
            this.DataContext = this;

            _progress.ProgressChanged += (s, e) =>
            {
                ProgressValue = e.Value;
                Message = e.Text;
            };
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? Canceled;

        public IProgress<ProgressInfo> Progress => _progress;


        public string Caption
        {
            get { return _caption; }
            set { SetProperty(ref _caption, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public bool CanCancel
        {
            get { return _canCancel; }
            set { SetProperty(ref _canCancel, value); }
        }

        protected override void OnClosed(EventArgs e)
        {
            _disposedValue = true;
            base.OnClosed(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            this.CancelButton.IsEnabled = false;
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.Close();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public record ProgressInfo(double Value, string Text);
}