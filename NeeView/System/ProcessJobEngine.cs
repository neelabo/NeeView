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



        private readonly Channel<IJobOperation> _jobQueue;
        private readonly CancellationTokenSource _cts = new();
        private bool _isProcessing;
        private bool _disposedValue;


        public ProcessJobEngine()
        {
            _jobQueue = Channel.CreateUnbounded<IJobOperation>();

            Task.Run(() => ProcessJobsAsync(_cts.Token));
        }


        public IProgress<ProgressContext>? Progress { get; set; }

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


        public JobOperation AddJob(string name, Action job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            var innerJob = async ValueTask (IProgress<ProgressContext>? progress, CancellationToken token) =>
            {
                progress?.Report(new ProgressContext(name));
                job.Invoke();
            };

            return AddJob(innerJob);
        }

        public JobOperation AddJob(string name, Func<CancellationToken, ValueTask> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            var innerJob = async ValueTask (IProgress<ProgressContext>? progress, CancellationToken token) =>
            {
                progress?.Report(new ProgressContext(name));
                await job.Invoke(token);
            };

            return AddJob(innerJob);
        }

        public JobOperation AddJob(Func<IProgress<ProgressContext>?, CancellationToken, ValueTask> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            var jobUnit = new JobOperation(job);
            _jobQueue.Writer.TryWrite(jobUnit);
            NotifyChange();

            return jobUnit;
        }

        public JobOperation<T> AddJob<T>(Func<IProgress<ProgressContext>?, CancellationToken, ValueTask<T>> job)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(ProcessJobEngine));

            var jobUnit = new JobOperation<T>(job);
            _jobQueue.Writer.TryWrite(jobUnit);
            NotifyChange();

            return jobUnit;
        }

        public async ValueTask WaitAsync(CancellationToken token)
        {
            await this.WaitPropertyAsync(nameof(IsBusy), e => !e.IsBusy, token);
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
                        await job.InvokeAsync(Progress, token);

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


    public class ProgressContext
    {
        public ProgressContext(string message)
            : this(message, 0.0, false)
        {
        }

        public ProgressContext(string message, double progress)
            : this(message, progress, true)
        {
        }

        public ProgressContext(string message, double progress, bool isProgressVisible)
        {
            Message = message;
            ProgressValue = progress;
            IsProgressVisible = isProgressVisible;
        }


        public string Message { get; set; } = "";
        public double ProgressValue { get; set; }
        public bool IsProgressVisible { get; set; }
    }


    public interface IJobOperation
    {
        JobState State { get; }
        ValueTask InvokeAsync(IProgress<ProgressContext>? progress, CancellationToken token);
    }


    public class JobOperation : BindableBase, IJobOperation
    {
        private JobState _state;
        private Func<IProgress<ProgressContext>?, CancellationToken, ValueTask> _job;


        public JobOperation(Func<IProgress<ProgressContext>?, CancellationToken, ValueTask> job)
        {
            _job = job;
        }


        public JobState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public async ValueTask InvokeAsync(IProgress<ProgressContext>? progress, CancellationToken token)
        {
            try
            {
                State = JobState.Run;
                await _job(progress, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                State = JobState.Closed;
            }
        }

        public async ValueTask WaitAsync(CancellationToken token)
        {
            await this.WaitPropertyAsync(nameof(State), e => e.State == JobState.Closed, token);
        }
    }


    public class JobOperation<T> : BindableBase, IJobOperation
    {
        private JobState _state;
        private Func<IProgress<ProgressContext>?, CancellationToken, ValueTask<T>> _job;


        public JobOperation(Func<IProgress<ProgressContext>?, CancellationToken, ValueTask<T>> job)
        {
            _job = job;
        }


        public JobState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public T? Result { get; private set; }


        public async ValueTask InvokeAsync(IProgress<ProgressContext>? progress, CancellationToken token)
        {
            try
            {
                State = JobState.Run;
                Result = await _job(progress, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                State = JobState.Closed;
            }
        }

        public async ValueTask<T?> WaitAsync(CancellationToken token)
        {
            await this.WaitPropertyAsync(nameof(State), e => e.State == JobState.Closed, token);
            return Result;
        }
    }
}
