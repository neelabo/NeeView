using NeeLaboratory.Collection;
using NeeView.Properties;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class ConsoleWindowManager : IDisposable
    {
        static ConsoleWindowManager() => Current = new ConsoleWindowManager();
        public static ConsoleWindowManager Current { get; }

        private ConsoleWindowManager()
        {
        }


        const int _messagesCapacity = 256;
        private ConsoleWindow? _window;
        private FixedQueue<string> _messages = new(_messagesCapacity);
        private bool _disposedValue;

        public bool IsOpened => _window != null;


        public void OpenWindow()
        {
            if (_disposedValue) return;

            if (_window != null)
            {
                AppDispatcher.Invoke(() => _window.Activate());
            }
            else
            {
                _window = AppDispatcher.Invoke(() =>
                {
                    var window = new ConsoleWindow();
                    window.Owner = App.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Closed += (s, e) => _window = null;
                    window.Show();
                    return window;
                });

                Flush();
            }
        }

        public void InforMessage(string message, bool withToast)
        {
            if (_disposedValue) return;

            WriteLine(ScriptMessageLevel.Info, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, TextResources.GetString("ScriptErrorDialog.Title.Info"), ToastIcon.Information, TextResources.GetString("ScriptErrorDialog.OpenConsole"), () => OpenWindow()));
            }
        }

        public void WarningMessage(string message, bool withToast)
        {
            if (_disposedValue) return;

            WriteLine(ScriptMessageLevel.Warning, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, TextResources.GetString("ScriptErrorDialog.Title.Warning"), ToastIcon.Warning, TextResources.GetString("ScriptErrorDialog.OpenConsole"), () => OpenWindow()));
            }
        }

        public void ErrorMessage(string message, bool withToast)
        {
            if (_disposedValue) return;

            WriteLine(ScriptMessageLevel.Error, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, TextResources.GetString("ScriptErrorDialog.Title.Error"), ToastIcon.Error, TextResources.GetString("ScriptErrorDialog.OpenConsole"), () => OpenWindow()));
            }
        }

        public void WriteLine(ScriptMessageLevel level, string message)
        {
            if (_disposedValue) return;

            var fixedMessage = (level != ScriptMessageLevel.None) ? level.ToString() + ": " + message : message;
            Debug.WriteLine(fixedMessage);

            if (_window != null)
            {
                _window.Console.WriteLine(fixedMessage);
            }
            else
            {
                _messages.Enqueue(fixedMessage);
            }
        }

        public void Flush()
        {
            if (_disposedValue) return;

            if (_messages.Count == 0) return;

            var messages = _messages;
            _messages = new FixedQueue<string>(_messagesCapacity);

            var console = _window?.Console;
            if (console != null)
            {
                foreach (var message in messages)
                {
                    console.WriteLine(message);
                }
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _window?.Close();
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
}
