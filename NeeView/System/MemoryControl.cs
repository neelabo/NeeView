using NeeView.Data;
using NeeView.Threading;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// ヒープ管理
    /// </summary>
    public class MemoryControl : IDisposable
    {
        static MemoryControl() => Current = new MemoryControl();
        public static MemoryControl Current { get; }


        private readonly DelayAction _delayAction;


        private MemoryControl()
        {
            _delayAction = new DelayAction(GarbageCollectCore, TimeSpan.FromSeconds(0.2));
        }


        /// <summary>
        /// 自動GCフラグ
        /// </summary>
        [PropertyMember]
        public bool IsAutoGC { get; set; } = false;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delayAction.Dispose();
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


        /// <summary>
        /// GCメイン
        /// </summary>
        private void GarbageCollectCore()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce; // cost +20ms
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// GCリクエスト
        /// </summary>
        public void GarbageCollect(bool force = false)
        {
            if (force)
            {
                _delayAction.Cancel();
                GarbageCollectCore();
                return;
            }

            if (IsAutoGC) return;

            _delayAction.Request();
        }

        /// <summary>
        /// OutOfMemory発生でメモリクリーンアップしてリトライ
        /// </summary>
        public void RetryActionWithMemoryCleanup(Action action)
        {
            try
            {
                action();
            }
            catch (OutOfMemoryException)
            {
                CleanupDeep();
                action();
            }
        }

        /// <summary>
        /// OutOfMemory発生でメモリクリーンアップしてリトライ
        /// </summary>
        public T RetryFuncWithMemoryCleanup<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (OutOfMemoryException)
            {
                CleanupDeep();
                return func();
            }
        }

        /// <summary>
        /// OutOfMemory発生でメモリクリーンアップしてリトライ (タスク)
        /// </summary>
        public async ValueTask<T> RetryFuncWithMemoryCleanupAsync<T>(ValueTask<T> task)
        {
            try
            {
                return await task;
            }
            catch (OutOfMemoryException)
            {
                CleanupDeep();
                return await task;
            }
        }

        /// <summary>
        /// キャッシュメモリクリーンアップ
        /// </summary>
        private void CleanupDeep()
        {
            // TODO: サムネイルキャッシュ開放
            Debug.WriteLine($">> OutOfMemory -> CleanUp");

            Book.Default?.BookMemoryService.CleanupDeep();
            GarbageCollect(true);
        }
    }
}
