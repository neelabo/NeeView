using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// JOB発行管理用単位
    /// </summary>
    public class JobSource : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public JobSource(JobCategory category, object key)
        {
            Category = category;
            Key = key;
            Job = Category.CreateJob(Key, _cancellationTokenSource.Token); 
        }

        /// <summary>
        /// JOBの種類。スケジューラの区分に使用される。
        /// </summary>
        public JobCategory Category { get; }

        /// <summary>
        /// JOBに関連付けられたキー
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// JOB本体
        /// </summary>
        public Job Job { get; }

        /// <summary>
        /// JobWorkerで処理が開始された
        /// </summary>
        public bool IsProcessed { get; set; }


        /// <summary>
        /// キャンセル
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// キャンセルリクエスト判定
        /// </summary>
        public bool IsCancellationRequested
        {
            get { return _cancellationTokenSource.IsCancellationRequested; }
        }

        /// <summary>
        /// JOB完了まで待機
        /// </summary>
        public async ValueTask WaitAsync(CancellationToken token)
        {
            await Job.WaitAsync(token);
        }

        /// <summary>
        /// JOB完了まで待機
        /// </summary>
        public async ValueTask WaitAsync(int millisecondTimeout, CancellationToken token)
        {
            await Job.WaitAsync(millisecondTimeout, token);
        }

        public override string ToString()
        {
            return Category.ToString() + "." + Key.ToString();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                    }
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
