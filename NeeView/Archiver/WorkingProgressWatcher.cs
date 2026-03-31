using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;


namespace NeeView
{
    /// <summary>
    /// アプリ終了時に処理中のタスクを待機するためのもの
    /// </summary>
    public class WorkingProgressWatcher
    {
        static WorkingProgressWatcher() => Current = new WorkingProgressWatcher();
        public static WorkingProgressWatcher Current { get; }

        private readonly List<ICancelableObject> _items = new();
        private readonly Lock _lock = new();


        private WorkingProgressWatcher()
        {
        }


        public event EventHandler<WorkingProcessCountChangedEventArgs>? CountChanged;


        public int Count => _items.Count;


        public void Add(ICancelableObject item)
        {
            int count;
            ICancelableObject? current;

            lock (_lock)
            {
                if (!_items.Contains(item))
                {
                    _items.Add(item);
                }
                count = _items.Count;
                current = _items.FirstOrDefault();
            }

            CountChanged?.Invoke(this, new WorkingProcessCountChangedEventArgs(count, current));
        }

        public void Remove(ICancelableObject? item)
        {
            if (item is null) return;

            int count;
            ICancelableObject? current;

            lock (_lock)
            {
                _items.Remove(item);
                count = _items.Count;
                current = _items.FirstOrDefault();
            }

            CountChanged?.Invoke(this, new WorkingProcessCountChangedEventArgs(count, current));
        }

        public ICancelableObject? GetCurrent()
        {
            lock (_lock)
            {
                return _items.FirstOrDefault();
            }
        }

        public void WaitWithDialog(Window owner)
        {
            if (Count <= 0) return;

            var dialog = new ProgressMessageDialog();
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            try
            {
                this.CountChanged += FileProgressWatcher_CountChanged;
                var item = GetCurrent();
                if (item is not null)
                {
                    dialog.SetCancellableObject(item);
                    dialog.ShowDialog();
                }
            }
            finally
            {
                this.CountChanged -= FileProgressWatcher_CountChanged;
            }

            void FileProgressWatcher_CountChanged(object? sender, WorkingProcessCountChangedEventArgs e)
            {
                if (e.Count <= 0)
                {
                    AppDispatcher.BeginInvoke(() => dialog.Close());
                }
                else
                {
                    dialog.SetCancellableObject(e.Current);
                }
            }
        }

        public CancellableObjectToken Lock(string name, Action? cancelAction = null)
        {
            return new CancellableObjectToken(this, new CancelableObject(name, cancelAction));
        }
    }


    public class WorkingProcessCountChangedEventArgs : EventArgs
    {
        public WorkingProcessCountChangedEventArgs(int count, ICancelableObject? current)
        {
            Count = count;
            Current = current;
        }
        public int Count { get; }
        public ICancelableObject? Current { get; }
    }


    public interface ICancelableObject
    {
        string Name { get; }
        bool CanCancel { get; }
        bool IsCanceled { get; set; }
        void Cancel();
    }


    public class CancelableObject : ICancelableObject
    {
        public CancelableObject(string name, Action? cancelAction)
        {
            Name = name;
            CancelAction = cancelAction;
        }

        public string Name { get; }
        public bool CanCancel => CancelAction != null;
        public bool IsCanceled { get; set; }
        public Action? CancelAction { get; set; }

        public void Cancel()
        {
            CancelAction?.Invoke();
        }
    }


    public class CancellableObjectToken : IDisposable
    {
        private readonly ICancelableObject _item;
        private readonly WorkingProgressWatcher _watcher;

        public CancellableObjectToken(WorkingProgressWatcher watcher, ICancelableObject item)
        {
            _watcher = watcher;
            _item = item;
            _watcher.Add(_item);
        }

        public void Dispose()
        {
            _watcher.Remove(_item);
        }
    }
}
