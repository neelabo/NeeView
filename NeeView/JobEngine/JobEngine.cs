using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// TODO: Jobの状態パラメータ(Status?)

namespace NeeView
{
    /// <summary>
    /// JobEngine
    /// </summary>
    public class JobEngine : BindableBase, IDisposable
    {
        static JobEngine() => Current = new JobEngine();
        public static JobEngine Current { get; }


        private bool _isBusy;
        private readonly int _maxWorkerSize;
        private int _workerSize = 2;
        private readonly DisposableCollection _disposables = new();


        // コンストラクタ
        private JobEngine()
        {
            InitializeScheduler();

            _maxWorkerSize = Config.Current.Performance.GetMaxJobWorkerSzie();
            _workerSize = Config.Current.Performance.JobWorkerSize;

            _disposables.Add(Config.Current.Performance.SubscribePropertyChanged(nameof(PerformanceConfig.JobWorkerSize),
                (s, e) =>
                {
                    _workerSize = Config.Current.Performance.JobWorkerSize;
                    ChangeWorkerSize(_workerSize);
                }));

            Workers = new JobWorker[_maxWorkerSize];

            ChangeWorkerSize(_workerSize);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public JobWorker?[] Workers { get; set; }


        // 稼働ワーカー数変更
        public void ChangeWorkerSize(int size)
        {
            if (_disposedValue) return;

            Debug.Assert(0 <= size && size <= _maxWorkerSize);
            Debug.WriteLine("JobEngine: WorkerSize=" + size);

            var primaryCount = (size > 2) ? 2 : size - 1;
            var isLimited = primaryCount <= 1;

            for (int i = 0; i < _maxWorkerSize; ++i)
            {
                var worker = Workers[i];
                if (i < size)
                {
                    if (worker == null)
                    {
                        worker = new JobWorker(_scheduler);
                        worker.IsBusyChanged += Worker_IsBusyChanged;
                        worker.Run();
                        Workers[i] = worker;
                        Debug.WriteLine($"JobEngine: Create Worker[{i}]");
                    }

                    worker.IsPrimary = i < primaryCount;
                    worker.IsLimited = isLimited;
                }
                else
                {
                    if (worker != null)
                    {
                        worker.IsBusyChanged -= Worker_IsBusyChanged;
                        worker.Cancel();
                        worker.Dispose();
                        Workers[i] = null;
                        Debug.WriteLine($"JobEngine: Delete Worker[{i}]");
                    }
                }
            }

            // イベント待ち解除
            _scheduler.RaiseQueueChanged();

            RaisePropertyChanged(nameof(Workers));
        }

        private void Worker_IsBusyChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            this.IsBusy = this.Workers.Any(e => e != null && e.IsBusy);
        }

        #region Scheduler
        private JobScheduler _scheduler;

        public JobScheduler Scheduler => _scheduler;

        [MemberNotNull(nameof(_scheduler))]
        private void InitializeScheduler()
        {
            _scheduler = new JobScheduler();
        }

        public void RegistClient(JobClient client)
        {
            if (_disposedValue) return;

            _scheduler.RegistClent(client);
        }

        public void UnregistClient(JobClient client)
        {
            if (_disposedValue) return;

            _scheduler.Order(client, new List<JobOrder>());
            _scheduler.UnregistClient(client);
        }

        public void CancelOrder(JobClient client)
        {
            if (_disposedValue) return;

            _scheduler.Order(client, new List<JobOrder>());
        }

        public List<JobSource> Order(JobClient client, List<JobOrder> orders)
        {
            if (_disposedValue) return new List<JobSource>();

            return _scheduler.Order(client, orders);
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    ChangeWorkerSize(0);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
