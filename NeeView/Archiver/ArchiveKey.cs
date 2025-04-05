using NeeView.Properties;
using System;

namespace NeeView
{
    public class ArchiveKey
    {
        private readonly string _fileName;
        private int _retryCount;

        public ArchiveKey(string fileName)
        {
            _fileName = fileName;
            _retryCount = 0;
        }


        public event EventHandler? KeyChanged;


        public string Key { get; private set; } = "";

        public ArchiveKeyState State { get; private set; }


        public void SetKey(string key)
        {
            Key = key;
            State = ArchiveKeyState.Completed;

            KeyChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetState(ArchiveKeyState state)
        {
            State = state;
        }

        public string? ShowArchiveKeyDialog(string message, string title)
        {
            var dialog = new PasswordDialog();
            dialog.Title = title;
            dialog.Message = message;
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
            return (dialog.DialogResult == true) ? dialog.InputValue : null;
        }

        public bool UpdateArchiveKeyByUser()
        {
            if (State == ArchiveKeyState.Canceled)
            {
                return false;
            }

            if (State == ArchiveKeyState.None)
            {
                if (ArchiveKeyCache.Current.TryGetValue(_fileName, out var text))
                {
                    SetKey(text);
                    return true;
                }
            }

            if (_retryCount > 0)
            {
                ToastService.Current.Show("ArchiveKey", new Toast(ResourceService.GetString("@ArchiveKeyDialog.IncorrectPassword"), null, ToastIcon.Warning));
            }

            var title = ResourceService.GetString("@ArchiveKeyDialog.Title");
            var message = TextResources.GetFormatString("ArchiveKeyDialog.Message", LoosePath.GetFileName(_fileName));
            var inputText = App.Current.Dispatcher.Invoke(() => ShowArchiveKeyDialog(message, title));

            if (inputText is not null)
            {
                SetKey(inputText);
                ArchiveKeyCache.Current.Add(_fileName, Key);
                _retryCount++;
                return true;
            }
            else
            {
                SetState(ArchiveKeyState.Canceled);
                return false;
            }
        }


        public enum ArchiveKeyState
        {
            None,
            Completed,
            Canceled,
        }
    }
}
