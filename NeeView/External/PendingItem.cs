//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace NeeView
{
    public class PendingItem : IDisposable
    {
        private readonly ClipboardWatcher _clipboardListener;
        private readonly Guid _guid;
        private readonly List<IPendingItem> _pages;
        private bool _disposedValue;
        private bool _isActive;

        public PendingItem(ClipboardWatcher clipboardListener, Guid guid, IEnumerable<IPendingItem> pages)
        {
            _clipboardListener = clipboardListener;
            _guid = guid;
            _pages = new List<IPendingItem>(pages);
        }


        public event EventHandler? Completed;


        public void Start()
        {
            if (_disposedValue) return;
            if (_isActive) return;

            _isActive = true;
            _pages.ForEach(e => e.IncrementPendingCount());
            _clipboardListener.ClipboardChanged += ClipboardListener_ClipboardChanged;
        }

        private void ClipboardListener_ClipboardChanged(object? sender, EventArgs e)
        {
            AppDispatcher.BeginInvoke(CheckClipboard);
        }

        private void CheckClipboard()
        {
            // どうにも例外(CLIPBRD_E_CANT_OPEN)が発生してしまうのでリトライさせることにした
            for (int retryCount = 0; retryCount < 10; retryCount++)
            {
                if (_disposedValue) return;

                try
                {
                    var guid = DataObjectGuid.Parse(Clipboard.GetData(DataObjectGuid.Format));
                    if (guid == _guid)
                    {
                        return;
                    }

                    Stop();
                    Completed?.Invoke(this, new EventArgs());
                    return;
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"Retry #{retryCount} ...");
                    Debug.WriteLine(ex.Message);
                    Thread.Sleep(100);
                }
            }
        }

        private void Stop()
        {
            if (!_isActive) return;

            _isActive = false;
            _clipboardListener.ClipboardChanged -= ClipboardListener_ClipboardChanged;
            _pages.ForEach(e => e.DecrementPendingCount());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Completed = null;
                    Stop();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            var s = string.Join(",", _pages);
            return $"CutPageUnit: Page={s}";
        }

        #region LOCAL_DEBUG

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }

        #endregion LOCAL_DEBUG
    }
}
