//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace NeeView
{
    public class PendingItemManager : IDisposable
    {
        private static PendingItemManager? _current;
        public static PendingItemManager Current => _current ?? throw new InvalidOperationException("Not initialized.");
        public static void Initialize(Window window) => _current = new PendingItemManager(window);


        private readonly ClipboardWatcher _clipboardWatcher;
        private readonly List<PendingItem> _items = new();
        private readonly Lock _lock = new();
        private bool _disposedValue;


        private PendingItemManager(Window window)
        {
            _clipboardWatcher = new ClipboardWatcher(window);

            ApplicationDisposer.Current.Add(this);
        }


        public void AddRange(Guid guid, IEnumerable<IPendingItem> pages)
        {
            if (_disposedValue) return;

            var item = new PendingItem(_clipboardWatcher, guid, pages);
            lock (_lock)
            {
                Trace($"Add: {item}");
                _items.Add(item);
                item.Completed += PendingItem_Completed;
                item.Start();
            }
        }

        private void PendingItem_Completed(object? sender, EventArgs e)
        {
            Remove(sender as PendingItem);
        }

        public void Remove(PendingItem? unit)
        {
            if (unit is null) return;

            lock (_lock)
            {
                Trace($"Remove: {unit}");
                unit.Dispose();
                _items.Remove(unit);
            }
        }

        public void Cancel()
        {
            if (_items.Count == 0) return;

            lock (_lock)
            {
                Trace($"Cancel: All");
                foreach (var unit in _items)
                {
                    unit.Dispose();
                }
                _items.Clear();
            }
            Clipboard.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
                    _clipboardWatcher.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
