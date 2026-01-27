using NeeLaboratory.Diagnostics;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// SingleJobエンジン
    /// </summary>
    public partial class SingleJobEngine : IEngine, IDisposable
    {
        /// <summary>
        /// ワーカータスクのキャンセルトークン
        /// </summary>
        private readonly CancellationTokenSource _engineCancellationTokenSource;

        /// <summary>
        /// エンジンON/OFF
        /// </summary>
        private readonly ManualResetEventSlim _activeEvent = new(false);

        /// <summary>
        /// 予約Job存在通知
        /// </summary>
        private readonly ManualResetEventSlim _readyEvent = new(false);

        /// <summary>
        /// 予約Jobリスト
        /// </summary>
        private Queue<IJob> _queue = new();

        /// <summary>
        /// 実行中Job
        /// </summary>
        private volatile IJob? _currentJob;

        /// <summary>
        /// 排他処理用オブジェクト
        /// </summary>
        private readonly System.Threading.Lock _lock = new();

        /// <summary>
        /// ワーカースレッド
        /// </summary>
        private readonly Thread? _thread;

        /// <summary>
        /// 開発用：ログ
        /// </summary>
        /// TODO: LOGのあつかいもっとスマートにできそう
        private readonly Log? _log;



        public SingleJobEngine(string name, bool isLogging = true)
        {
            _log = isLogging ? new Log(name, 0) : null;
            _log?.Trace($"start...");

            _engineCancellationTokenSource = new CancellationTokenSource();

            _thread = new Thread(() =>
            {
                try
                {
                    Worker(_engineCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _log?.Trace($"canceled.");
                }
                catch (Exception ex)
                {
                    _log?.Trace(TraceEventType.Critical, $"excepted: {ex.Message}");
                    var args = new JobErrorEventArgs(ex, null);
                    JobEngineError?.Invoke(this, args);
                    if (!args.Handled)
                    {
                        throw;
                    }
                }
                finally
                {
                    _log?.Trace($"stopped.");
                    ////Debug.WriteLine($"{this}: worker thread terminated.");
                }
            });

            _thread.IsBackground = true;
            _thread.Name = name;
            _thread.Start();
        }


        /// <summary>
        /// JOBエラー発生時のイベント
        /// </summary>
        [Subscribable]
        public event EventHandler<JobErrorEventArgs>? JobError;

        /// <summary>
        /// 例外によってJobEngineが停止した時に発生するイベント
        /// </summary>
        [Subscribable]
        public event EventHandler<JobErrorEventArgs>? JobEngineError;

        /// <summary>
        /// IsBusyプロパティ変更EVENT
        /// </summary>
        [Subscribable]
        public event EventHandler<JobIsBusyChangedEventArgs>? IsBusyChanged;


        /// <summary>
        /// 現在のJob数
        /// </summary>
        public int Count
        {
            get { return _queue.Count + (_currentJob != null ? 1 : 0); }
        }

        /// <summary>
        /// 活動中
        /// </summary>
        public bool IsBusy => Count > 0;

        /// <summary>
        /// エンジン自体のキャンセルトークン
        /// </summary>
        public CancellationToken CancellationToken => _engineCancellationTokenSource.Token;


        /// <summary>
        /// 実行中を含む全てのJobを取得
        /// </summary>
        public List<IJob> AllJobs()
        {
            var jobs = new List<IJob>();

            lock (_lock)
            {
                var job = _currentJob;
                if (job != null)
                {
                    jobs.Add(job);
                }
                jobs.AddRange(_queue);
            }

            return jobs;
        }


        /// <summary>
        /// Job登録
        /// </summary>
        public void Enqueue(IJob job)
        {
            Debug.Assert(job is not null);
            if (job is null) return;

            ThrowIfDisposed();

            lock (_lock)
            {
                _queue = Enqueue(job, _queue);
                if (_queue.Count > 0)
                {
                    _log?.Trace($"Job entry: {job}");
                    IsBusyChanged?.Invoke(this, new JobIsBusyChangedEventArgs(Count)); // lock中にイベントを呼ぶのは...?
                    _readyEvent.Set();
                }
                else
                {
                    _log?.Trace($"Job entry canceled: {job}");
                }
            }
        }

        /// <summary>
        /// Job登録
        /// </summary>
        protected virtual Queue<IJob> Enqueue(IJob job, Queue<IJob> queue)
        {
            Debug.Assert(job is not null);
            Debug.Assert(queue is not null);

            queue.Enqueue(job);
            return queue;
        }


        /// <summary>
        /// ワーカータスク
        /// </summary>
        private void Worker(CancellationToken token)
        {
            while (true)
            {
                Debug.Assert(_currentJob is null);
                token.ThrowIfCancellationRequested();

                _readyEvent.Wait(token);
                token.ThrowIfCancellationRequested();

                _activeEvent.Wait(token);
                token.ThrowIfCancellationRequested();

                lock (_lock)
                {
                    if (_queue.Count <= 0)
                    {
                        _readyEvent.Reset();
                        continue;
                    }
                    _currentJob = _queue.Dequeue();
                }

                try
                {
                    _log?.Trace($"Job execute: {_currentJob}");
                    _currentJob?.ExecuteAsync().AsTask().Wait(token);
                }
                catch (OperationCanceledException)
                {
                    _log?.Trace(TraceEventType.Information, $"Job canceled: {_currentJob}");
                }
                catch (Exception ex)
                {
                    _log?.Trace(TraceEventType.Error, $"Job excepted: {_currentJob}");
                    HandleJobException(ex, _currentJob);
                }

                lock (_lock)
                {
                    _currentJob?.Dispose();
                    _currentJob = null;
                }

                IsBusyChanged?.Invoke(this, new JobIsBusyChangedEventArgs(Count));
            }
        }

        /// <summary>
        /// JOBで発生した例外の処理
        /// </summary>
        private void HandleJobException(Exception exception, IJob job)
        {
            Debug.Assert(exception is not null);
            Debug.Assert(job is not null);

            var args = new JobErrorEventArgs(exception, job);
            JobError?.Invoke(this, args);
            if (!args.Handled)
            {
                throw new JobException($"Job Exception: {job}", exception, job);
            }
        }

        #region IEngine Support
        public void StartEngine()
        {
            _activeEvent.Set();
        }

        public virtual void StopEngine()
        {
            _activeEvent.Reset();
        }
        #endregion IEngine Support

        #region IDisposable Support
        private bool _disposedValue = false;

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
                    _engineCancellationTokenSource.Cancel();
                    _engineCancellationTokenSource.Dispose();

                    _activeEvent.Dispose();
                    _readyEvent.Dispose();

                    foreach (var job in AllJobs())
                    {
                        job.Dispose();
                    }

                    _log?.Dispose();
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

    public class JobIsBusyChangedEventArgs : EventArgs
    {
        public JobIsBusyChangedEventArgs(int count)
        {
            Debug.Assert(count >= 0);

            Count = count;
        }

        public int Count { get; }
        public bool IsBusy => Count > 0;

    }
}
