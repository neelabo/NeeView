using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ジョブワーカー
    /// </summary>
    public partial class JobWorker : BindableBase, IDisposable
    {
        #region 開発用

        public DebugSimpleLog? DebugLog { get; private set; }

        [Conditional("DEBUG")]
        public void Log(string message)
        {
            DebugLog = DebugLog ?? new DebugSimpleLog();
            DebugLog.WriteLine(Thread.CurrentThread.Priority + ": " + $"{_jobPriorityMin}-{_jobPriorityMax}: " + message);
            RaisePropertyChanged(nameof(DebugLog));
        }

        #endregion

        // スケジューラー
        private readonly JobScheduler _scheduler;

        // ワーカータスクのキャンセルトークン
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // ジョブ待ちフラグ
        private readonly ManualResetEventSlim _event = new(false);

        // ワーカースレッド
        private Thread? _thread;

        private bool _isBusy;
        private bool _isPrimary;
        private bool _isLimited;
        private int _jobPriorityMin;
        private int _jobPriorityMax;


        // コンストラクタ
        public JobWorker(JobScheduler scheduler)
        {
            _scheduler = scheduler;
            _scheduler.QueueChanged += Context_JobChanged;

            UpdateJobPriorityRange();
        }


        [Subscribable]
        public event EventHandler? IsBusyChanged;


        /// <summary>
        /// IsBusy property.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (IsDisposed()) return;

                if (_isBusy != value)
                {
                    _isBusy = value;
                    IsBusyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 優先ワーカー.
        /// 現在開いているフォルダーに対してのジョブのみ処理する
        /// </summary>
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set
            {
                if (IsDisposed()) return;

                if (SetProperty(ref _isPrimary, value))
                {
                    UpdateThreadPriority();
                    UpdateJobPriorityRange();
                }
            }
        }

        /// <summary>
        /// JobWorkerの総数が少ない状態
        /// </summary>
        public bool IsLimited
        {
            get { return _isLimited; }
            set
            {
                if (IsDisposed()) return;

                if (SetProperty(ref _isLimited, value))
                {
                    UpdateJobPriorityRange();
                }
            }
        }


        private void Context_JobChanged(object? sender, EventArgs e)
        {
            if (IsDisposed()) return;

            _event.Set();
        }

        private void UpdateJobPriorityRange()
        {
            if (IsPrimary)
            {
                _jobPriorityMin = 10;
                _jobPriorityMax = 99;
            }
            else
            {
                _jobPriorityMin = 0;
                _jobPriorityMax = IsLimited ? 99 : 9;
            }
        }

        private void UpdateThreadPriority()
        {
            if (_thread != null)
            {
                _thread.Priority = IsPrimary ? ThreadPriority.Normal : ThreadPriority.BelowNormal;
            }
        }

        // ワーカータスク開始
        public void Run()
        {
            Log($"Run");

            _thread = new Thread(() => WorkerExecuteAsync(_cancellationTokenSource.Token));
            _thread.IsBackground = true;
            _thread.Name = "JobWorker";
            _thread.Start();

            UpdateThreadPriority();
        }

        // ワーカータスク廃棄
        public void Cancel()
        {
            if (IsDisposed()) return;

            _cancellationTokenSource.Cancel();
        }

        // ワーカータスクメイン
        private void WorkerExecuteAsync(CancellationToken token)
        {
            try
            {
                WorkerExecuteAsyncCore(token);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"JOB TASK CANCELED.");
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine($"JOB TASK DISPOSED.");
            }
        }

        // ワーカータスクメイン
        private void WorkerExecuteAsyncCore(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                Log($"get Job ...");
                Job? job;

                lock (_scheduler.Lock)
                {
                    ThrowIfDisposed();

                    // ジョブ取り出し
                    job = _scheduler.FetchNextJob(_jobPriorityMin, _jobPriorityMax);

                    // ジョブが無い場合はイベントリセット
                    if (job == null)
                    {
                        _event.Reset();
                    }
                }

                // イベント待ち
                if (job == null)
                {
                    IsBusy = false;
                    Log($"wait event ...");
                    _event.Wait(token);
                    continue;
                }

                // JOB実行
                IsBusy = true;
                Log($"Job({job.SerialNumber}) execute ...");
                job.Execute();
                Log($"Job({job.SerialNumber}) execute done. : {job.Result}");
            }
        }

        #region IDisposable Support
        private int _disposedValue;

        private bool IsDisposed()
        {
            return _disposedValue != 0;
        }

        private void ThrowIfDisposed()
        {
            if (_disposedValue != 0) throw new ObjectDisposedException(nameof(JobWorker));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposedValue, 1) == 0)
            {
                if (disposing)
                {
                    _scheduler.QueueChanged -= Context_JobChanged;

                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();

                    _event.Set();
                    _event.Dispose();
                }
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
