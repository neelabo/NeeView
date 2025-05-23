using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public enum JobState
    {
        None,
        Run,
        Closed,
    }

    public enum JobResult
    {
        None,
        Completed,
        Canceled,
        Failed,
    }

    /// <summary>
    /// ジョブ
    /// </summary>
    public class Job : BindableBase, IDisposable
    {
        private readonly ManualResetEventSlim _completed = new();
        private readonly CancellationToken _cancellationToken;
        private readonly IJobCommand _command;
        private JobState _state;
        private JobResult _result;


        private Job(int serialNumber, IJobCommand command, CancellationToken token)
        {
            SerialNumber = serialNumber;
            _command = command;
            _cancellationToken = token;
        }


        // シリアル番号(開発用..HashCodeで代用可能か)
        public int SerialNumber { get; private set; }

        public bool IsCompleted
        {
            get { return _completed.IsSet; }
        }

        public JobState State
        {
            get { return _state; }
            private set { SetProperty(ref _state, value); }
        }

        public JobResult Result
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
        }


        /// <summary>
        /// JOB終了状態を設定
        /// </summary>
        public void SetResult(JobResult result)
        {
            if (_disposedValue) return;
            if (_completed.IsSet) return;

            Result = result;
            State = JobState.Closed;
            _completed.Set();
        }

        public void Execute()
        {
            if (_disposedValue) return;

            if (_completed.IsSet)
            {
                Log("IsCompleted.");
                return;
            }

            Log("Run...");

            try
            {
                _cancellationToken.ThrowIfCancellationRequested();
                State = JobState.Run;
                _command.Execute(_cancellationToken);
                SetResult(JobResult.Completed);
            }
            catch (OperationCanceledException ex)
            {
                Log($"Exception: {ex.Message}");
                SetResult(JobResult.Canceled);
            }
            catch (AggregateException ex)
            {
                foreach (var iex in ex.InnerExceptions)
                {
                    Log($"Exception: {iex.Message}");
                }
                SetResult(ex.InnerExceptions.Any(e => e is OperationCanceledException) ? JobResult.Canceled : JobResult.Failed);
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message}");
                SetResult(JobResult.Failed);
            }

            Log($"Done: {Result}");
        }

        public async ValueTask WaitAsync(CancellationToken token)
        {
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _cancellationToken))
            {
                await _completed.WaitHandle.AsTask().WaitAsync(token);
            }
        }

        public async ValueTask WaitAsync(int millisecondsTimeout, CancellationToken token)
        {
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _cancellationToken))
            {
                var span = TimeSpan.FromMilliseconds(millisecondsTimeout);
                await _completed.WaitHandle.AsTask().WaitAsync(span, token);
            }
        }


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
                    SetResult(JobResult.Canceled);
                    _completed.Dispose();
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

        #region Helper

        private static int _serialNumber;

        public static Job Create(IJobCommand command, CancellationToken token)
        {
            var job = new Job(_serialNumber++, command, token);
            return job;
        }

        #endregion

        #region for Debug

        public DebugSimpleLog? DebugLog { get; private set; }

        [Conditional("DEBUG")]
        public void Log(string msg)
        {
            DebugLog = DebugLog ?? new DebugSimpleLog();
            DebugLog.WriteLine(msg);
            RaisePropertyChanged(nameof(DebugLog));
        }

        #endregion
    }

}
