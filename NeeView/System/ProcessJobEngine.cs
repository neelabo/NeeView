//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 非同期処理をJOBとして順番に実行する
    /// </summary>
    /// <remake>
    /// 時間のかかる初期化処理を非同期でおこなうときに使用する
    /// </remake>
    [LocalDebug]
    public partial class ProcessJobEngine : BindableBase, IDisposable
    {
        public static Lazy<ProcessJobEngine> _current = new();
        public static ProcessJobEngine Current => _current.Value;

        public record class JobUnit(string Name, Func<ValueTask> Job)
        {
        }

        private readonly Channel<JobUnit> _jobQueue;
        private readonly CancellationTokenSource _cts = new();
        private bool _isProcessing;
        private bool _disposedValue;


        public ProcessJobEngine()
        {
            _jobQueue = Channel.CreateUnbounded<JobUnit>();

            Task.Run(() => ProcessJobsAsync(_cts.Token));
        }


        public IProgress<string>? Progress { get; set; }

        public bool IsProcessing
        {
            get => _isProcessing;
            private set
            {
                _isProcessing = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsBusy));
            }
        }

        public int PendingJobsCount => _jobQueue.Reader.Count;

        public bool IsBusy => IsProcessing || PendingJobsCount > 0;


        public void AddJob(string name, Func<ValueTask> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            _jobQueue.Writer.TryWrite(new JobUnit(name, job));
            NotifyChange();
        }

        public void AddJob(string name, Func<Task> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            _jobQueue.Writer.TryWrite(new JobUnit(name, async () => await job()));
            NotifyChange();
        }

        public void AddJob(string name, Action job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            _jobQueue.Writer.TryWrite(new JobUnit(name, () => { job(); return default; }));
            NotifyChange();
        }

        public async ValueTask AddJobAsync(string name, Func<ValueTask> job, CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            await _jobQueue.Writer.WriteAsync(new JobUnit(name, job), token);
            NotifyChange();
        }

        public async ValueTask AddJobAsync(string name, Func<Task> job, CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            await _jobQueue.Writer.WriteAsync(new JobUnit(name, async () => await job()), token);
            NotifyChange();
        }

        public async ValueTask AddJobAsync(string name, Action job, CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            await _jobQueue.Writer.WriteAsync(new JobUnit(name, () => { job(); return default; }), token);
            NotifyChange();
        }

        private async Task ProcessJobsAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            try
            {
                await foreach (var job in _jobQueue.Reader.ReadAllAsync(token))
                {
                    NVDebug.AssertMTA();
                    try
                    {
                        IsProcessing = true;
                        NotifyChange();

                        LocalDebug.WriteLine("Job start...");
                        Progress?.Report(job.Name);
                        await job.Job();

#if DEBUG
                        // [開発用]
                        //await Task.Delay(500);
#endif

                        LocalDebug.WriteLine("Job done.");
                    }
                    catch (Exception ex)
                    {
                        // 個別のJobのエラーでループを止めない
                        Debug.WriteLine($"Job Error: {ex.Message}");
                    }
                    finally
                    {
                        IsProcessing = false;
                        NotifyChange();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"{nameof(ProcessJobEngine)} has been shut down.");
            }
        }

        private void NotifyChange()
        {
            RaisePropertyChanged(nameof(PendingJobsCount));
            RaisePropertyChanged(nameof(IsBusy));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _jobQueue.Writer.TryComplete();
                    _cts.Cancel();
                    _cts.Dispose();
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
