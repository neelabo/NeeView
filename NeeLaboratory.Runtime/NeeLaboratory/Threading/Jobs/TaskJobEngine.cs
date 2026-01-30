//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Task-based Job Engine
    /// </summary>
    [LocalDebug]
    public partial class TaskJobEngine : BindableBase, IDisposable
    {
        private readonly Channel<IJobOperation> _jobQueue;
        private readonly CancellationTokenSource _cts = new();
        private bool _isProcessing;
        private int _pendingJobsCount;
        private bool _isBusy;
        private bool _disposedValue;


        public TaskJobEngine()
        {
            _jobQueue = Channel.CreateUnbounded<IJobOperation>();

            Task.Run(() => ProcessJobsAsync(_cts.Token));
        }


        public int PendingJobsCount
        {
            get { return _pendingJobsCount; }
            private set
            {
                if (SetProperty(ref _pendingJobsCount, value))
                {
                    UpdateBusy();
                }
            }
        }

        public bool IsProcessing
        {
            get { return _isProcessing; }
            private set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    UpdateBusy();
                }
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    LocalDebug.WriteLine($"IsBusy = {_isBusy}");
                }
            }
        }


        private void UpdateBusy() => IsBusy = IsProcessing || PendingJobsCount > 0;


        public JobOperation<int> AddJob(Func<CancellationToken, ValueTask> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(TaskJobEngine));

            return AddJob(InnerJob);

            async ValueTask<int> InnerJob(CancellationToken token)
            {
                await job(token);
                return 0;
            }
        }

        public JobOperation<T> AddJob<T>(Func<CancellationToken, ValueTask<T>> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(TaskJobEngine));

            var jobUnit = new JobOperation<T>(job);
            _jobQueue.Writer.TryWrite(jobUnit);
            PendingJobsCount = _jobQueue.Reader.Count;

            return jobUnit;
        }

        public async ValueTask WaitAsync(CancellationToken token)
        {
            await this.WaitPropertyAsync(nameof(IsBusy), e => !e.IsBusy, token);
        }

        private async Task ProcessJobsAsync(CancellationToken token)
        {
            try
            {
                await foreach (var job in _jobQueue.Reader.ReadAllAsync(token))
                {
                    try
                    {
                        IsProcessing = true;
                        PendingJobsCount = _jobQueue.Reader.Count;

                        LocalDebug.WriteLine("Job start...");
                        await job.InvokeAsync(token);

#if DEBUG
                        // [dev]
                        //await Task.Delay(500);
#endif

                        LocalDebug.WriteLine("Job done.");
                    }
                    catch (Exception ex)
                    {
                        // Do not stop the loop due to errors in individual jobs
                        Debug.WriteLine($"Job Error: {ex.Message}");
                    }
                    finally
                    {
                        IsProcessing = false;
                        PendingJobsCount = _jobQueue.Reader.Count;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"{nameof(TaskJobEngine)} has been shut down.");
            }
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
