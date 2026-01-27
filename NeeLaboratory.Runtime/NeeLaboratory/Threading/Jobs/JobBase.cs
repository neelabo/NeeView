using NeeLaboratory.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{

    /// <summary>
    /// Job基底
    /// キャンセル、終了待機対応
    /// </summary>
    public abstract class JobBase : IJob, IDisposable
    {
        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 実行完了待ち用フラグ
        /// </summary>
        private readonly ManualResetEventSlim _complete = new(false);

        /// <summary>
        /// 実行結果
        /// </summary>
        private JobState _state;



        public JobBase()
        {
            _cancellationToken = _tokenSource.Token;
        }

        public JobBase(CancellationToken token)
        {
            _cancellationToken = token;
        }


        public event EventHandler<JobCompletedEventArgs>? Completed;


        /// <summary>
        /// 実行状態
        /// </summary>
        public JobState State
        {
            get { return _state; }
        }


        /// <summary>
        /// 実行状態設定
        /// </summary>
        private void SetState(JobState state, bool isCompleted)
        {
            _state = state;
            if (isCompleted)
            {
                _complete.Set();
                Completed?.Invoke(this, new JobCompletedEventArgs(state));
            }
        }

        /// <summary>
        /// キャンセル
        /// </summary>
        public void Cancel()
        {
            if (_disposedValue) return;

            _tokenSource.Cancel();

            if (_state == JobState.None)
            {
                SetState(JobState.Canceled, true);
            }
        }

        /// <summary>
        /// Job実行。エンジンから呼ばれる
        /// </summary>
        /// <param name="token">エンジンのキャンセルトークン</param>
        /// <returns></returns>
        public async ValueTask ExecuteAsync()
        {
            if (_disposedValue) return;

            if (_complete.IsSet) return;

            // cancel ?
            if (_cancellationToken.IsCancellationRequested)
            {
                SetState(JobState.Canceled, true);
                return;
            }

            // execute
            try
            {
                SetState(JobState.Run, false);
                await ExecuteAsync(_cancellationToken);
                SetState(JobState.Completed, true);
            }
            catch (OperationCanceledException)
            {
                SetState(JobState.Canceled, true);
                Debug.WriteLine($"Job {this}: canceled.");
                OnCanceled();
            }
            catch (Exception e)
            {
                SetState(JobState.Faulted, true);
                Debug.WriteLine($"Job {this}: excepted!!");
                OnException(e);
                throw;
            }
        }

        /// <summary>
        /// Job終了待機
        /// </summary>
        public async ValueTask WaitAsync()
        {
            await _complete.WaitHandle.AsTask();
        }

        public async ValueTask WaitAsync(CancellationToken token)
        {
            await _complete.WaitHandle.AsTask().WaitAsync(token);
        }


        /// <summary>
        /// Job実行(abstract)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract ValueTask ExecuteAsync(CancellationToken token);

        /// <summary>
        /// Jobキャンセル時
        /// </summary>
        protected virtual void OnCanceled()
        {
        }

        /// <summary>
        /// Job例外時
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnException(Exception e)
        {
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
                    Cancel();
                    _tokenSource.Dispose();
                    _complete.Dispose();
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



    public class JobCompletedEventArgs : EventArgs
    {
        public JobCompletedEventArgs(JobState state)
        {
            State = state;
        }

        public JobState State { get; }
    }
}
