using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace NeeView
{
    public class ToastService : BindableBase
    {
        static ToastService() => Current = new ToastService();
        public static ToastService Current { get; }


        private readonly Queue<Toast> _queue;
        private ToastCard? _toastCard;
        private readonly DispatcherTimer _timer;
        private DateTime _timeLimit;
        private readonly Dictionary<string, Toast> _slotMap = new();
        private readonly System.Threading.Lock _lock = new();


        public ToastService()
        {
            _queue = new Queue<Toast>();

            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }


        public ToastCard? ToastCard
        {
            get { return _toastCard; }
            set { SetProperty(ref _toastCard, value); }
        }


        public void Show(string slot, Toast toast)
        {
            if (toast is null) return;

            lock (_lock)
            {
                if (_slotMap.TryGetValue(slot, out Toast? oldToast))
                {
                    oldToast.Cancel();
                }

                _slotMap[slot] = toast;
            }

            Show(toast);
        }

        public void Show(Toast toast)
        {
            if (toast is null) return;

            lock (_lock)
            {
                _queue.Enqueue(toast);

                // ひとまず１枚だけに限定する
                if (ToastCard != null)
                {
                    ToastCard.IsCanceled = true;
                }
            }

            Update();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Update();
        }

        public void Update()
        {
            AppDispatcher.BeginInvoke(() => UpdateCore());
        }

        private void UpdateCore()
        {
            if (ToastCard != null)
            {
                if (ToastCard.IsCanceled || ToastCard.Toast.IsCanceled || (!ToastCard.IsMouseOver && DateTime.Now > _timeLimit))
                {
                    Close();
                }
            }

            lock (_lock)
            {
                while (ToastCard == null && _queue.Count > 0)
                {
                    var toast = _queue.Dequeue();
                    Open(toast);
                }
            }
        }

        private void Open(Toast toast)
        {
            if (toast is null) return;

            if (toast.IsCanceled)
            {
                return;
            }

            ToastCard = new ToastCard(toast);
            _timeLimit = DateTime.Now + toast.DisplayTime;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void Close()
        {
            if (ToastCard != null)
            {
                _timer.Stop();
                ToastCard = null;
            }
        }

    }
}
