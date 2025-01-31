using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private object _lock = new();


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

        public void WaitWithDialog(Window owner)
        {
            if (Count <= 0) return;

            var dialog = new ProgressMessageDialog(ResourceService.GetString("@Word.Processing"));
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Canceled += Dialog_Canceled;

            try
            {
                this.CountChanged += FileProgressWatcher_CountChanged;
                if (Count > 0)
                {
                    dialog.Message = _items.FirstOrDefault()?.Name ?? "";
                    dialog.ShowDialog();
                }
            }
            finally
            {
                this.CountChanged -= FileProgressWatcher_CountChanged;
            }

            void Dialog_Canceled(object? sender, EventArgs e)
            {
                List<ICancelableObject> clone;
                lock (_lock)
                {
                    clone = new List<ICancelableObject>(_items);
                }
                foreach (var item in clone)
                {
                    item.Cancel();
                }
            }

            void FileProgressWatcher_CountChanged(object? sender, WorkingProcessCountChangedEventArgs e)
            {
                if (e.Count <= 0)
                {
                    AppDispatcher.BeginInvoke(() => dialog.Close());
                }
                else
                {
                    dialog.Message = e.Current?.Name ?? "";
                }
            }
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

        public Action? CancelAction { get; set; }

        public void Cancel()
        {
            CancelAction?.Invoke();
        }
    }
}
