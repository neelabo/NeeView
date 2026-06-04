using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    [INotifyPropertyChanged]
    public partial class ProgressDialog : Window
    {
        private readonly Progress<ProgressInfo>? _progress;
        private readonly CancellationTokenSource _tokenSource = new();
        private string _caption = "";
        private string _message = TextResources.GetString("Word.Processing");
        private double _progressValue = 0.0;
        private bool _canCancel;
        private bool _canceled;
        private bool _closed;

        public ProgressDialog() : this(null, null)
        {
        }

        public ProgressDialog(Window? owner) : this(owner, null)
        {
        }

        public ProgressDialog(Window? owner, Progress<ProgressInfo>? progress)
        {
            if (owner is not null)
            {
                this.Owner = owner;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
            this.DataContext = this;

            _progress = progress;

            if (_progress is not null)
            {
                this.ProgressBar.Visibility = Visibility.Visible;
                this.ProgressRing.Visibility = Visibility.Collapsed;

                _progress.ProgressChanged += Progress_ProgressChanged;
            }
            else
            {
                this.ProgressBar.Visibility = Visibility.Collapsed;
                this.ProgressRing.Visibility = Visibility.Visible;
            }
        }


        public event EventHandler? Canceled;


        public IProgress<ProgressInfo>? Progress => _progress;

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

        public AggregateException? Exception { get; private set; }

        public ProgressDialogResult Result { get; private set; }


        private void Progress_ProgressChanged(object? sender, ProgressInfo e)
        {
            if (_canceled) return;
            ProgressValue = e.Value;
            Message = e.Text;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_progress is not null)
            {
                _progress.ProgressChanged -= Progress_ProgressChanged;
            }

            base.OnClosed(e);
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
            if (this.ProgressBar.Visibility == Visibility.Visible)
            {
                this.ProgressBar.Visibility = Visibility.Hidden;
            }

            _tokenSource.Cancel();
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        public new void Close()
        {
            if (_closed) return;
            _closed = true;

            base.Close();
        }

        public ProgressDialogResult ShowDialog(Task task)
        {
            if (_closed) return Result;

            task.ContinueWith(t => this.Dispatcher.Invoke(async () =>
            {
                Exception = t.Exception;

                bool isActuallyCanceled = t.IsCanceled || (t.IsFaulted && t.Exception.Flatten().InnerExceptions.Any(e => e is OperationCanceledException));

                if (_canceled || isActuallyCanceled)
                {
                    await Task.Delay(1000);
                    Result = ProgressDialogResult.Canceled;
                }
                else if (t.IsFaulted)
                {
                    DialogResult = false;
                    Result = ProgressDialogResult.Faulted;
                }
                else
                {
                    DialogResult = true;
                    Result = ProgressDialogResult.Completed;
                }
                Close();
            }));

            ShowDialog();
            return Result;
        }

        public ProgressDialogResult ShowDialog(Func<CancellationToken, Task> createTask)
        {
            if (_closed) return Result;

            return ShowDialog(createTask(_tokenSource.Token));
        }
    }


    public enum ProgressDialogResult
    {
        None,
        Completed,
        Canceled,
        Faulted,
    }


    public record ProgressInfo(double Value, string Text);
}