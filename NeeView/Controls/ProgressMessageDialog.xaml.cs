using NeeLaboratory.Generators;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ProgressMessageDialog : Window, INotifyPropertyChanged
    {
        private bool _closeable = true;
        private ICancelableObject? _cancellableObject;

        public ProgressMessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public string Message => _cancellableObject?.Name ?? "";

        public bool CanCancel => _cancellableObject?.CanCancel ?? false;

        public bool IsCanceled => _cancellableObject?.IsCanceled ?? false;


        public void SetCancellableObject(ICancelableObject? item)
        {
            _cancellableObject = item;
            RaisePropertyChanged(nameof(Message));
            RaisePropertyChanged(nameof(CanCancel));
            RaisePropertyChanged(nameof(IsCanceled));
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
            if (_cancellableObject is null) return;
            if (_cancellableObject.IsCanceled) return;

            _cancellableObject.IsCanceled = true;
            RaisePropertyChanged(nameof(IsCanceled));
            _cancellableObject.Cancel();
        }

        public new void Close()
        {
            _closeable = true;
            base.Close();
        }
    }
}
