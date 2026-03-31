using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ProgressDialog : Window, INotifyPropertyChanged
    {
        private readonly Progress<ProgressInfo> _progress;
        private readonly CancellationTokenSource _tokenSource = new();
        private string _caption = "";
        private string _message = "";
        private double _progressValue = 0.0;
        private bool _canCancel;
        private bool _canceled;
        private bool _closed;

        public ProgressDialog() : this(null, new Progress<ProgressInfo>())
        {
        }

        public ProgressDialog(Window? owner) : this(owner, new Progress<ProgressInfo>())
        {
        }

        public ProgressDialog(Window? owner, Progress<ProgressInfo> progress)
        {
            if (owner is not null)
            {
                this.Owner = owner;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
            this.DataContext = this;

            _progress = progress;
            _progress.ProgressChanged += (s, e) =>
            {
                if (_canceled) return;
                ProgressValue = e.Value;
                Message = e.Text;
            };
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? Canceled;


        public IProgress<ProgressInfo> Progress => _progress;

        public CancellationToken CancellationToken => _tokenSource.Token;


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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_canCancel && e.Key == Key.Escape)
            {
                Cancel();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            if (_canceled) return;
            _canceled = true;

            this.CancelButton.IsEnabled = false;
            this.Message = TextResources.GetString("Word.Canceling");
            this.ProgressBar.Visibility = Visibility.Hidden;

            _tokenSource.Cancel();
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        public new void Close()
        {
            if (_closed) return;
            _closed = true;

            base.Close();
        }

        public bool? ShowDialog(Task task)
        {
            if (_closed) return false;

            task.ContinueWith(t => this.Dispatcher.Invoke(async () =>
            {
                if (_canceled)
                {
                    await Task.Delay(1000);
                }
                Close();
            }));

            return ShowDialog();
        }

        public bool? ShowDialog(Func<CancellationToken, Task> createTask)
        {
            if (_closed) return false;

            return ShowDialog(createTask(_tokenSource.Token));
        }
    }


    public record ProgressInfo(double Value, string Text);
}