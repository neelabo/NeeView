//#define LOCAL_DEBUG

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 遅延ページ移動
    /// </summary>
    public class PageFrameBoxDelayMove : IDisposable
    {
        private readonly System.Threading.Lock _lock = new();
        private readonly PageFrameBox _box;
        private readonly BookPageLoader _loader;
        private readonly PageLoading _pageLoading;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposedValue;


        public PageFrameBoxDelayMove(PageFrameBox box, BookPageLoader loader, PageLoading pageLoading)
        {
            _box = box;
            _loader = loader;
            _pageLoading = pageLoading;
        }


        public void Cancel()
        {
            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// 遅延移動
        /// </summary>
        /// <param name="parameter">移動パラメータ</param>
        /// <param name="container"></param>
        /// TODO: 引数の意味が重複している。container から parameter を生成する方向で？
        public void MoveTo(PageFrameMoveParameter parameter, PageFrameContainer container)
        {
            if (_disposedValue) return;

            var item = (PageFrameContent)container.Content;
            _pageLoading.Message = item.PageFrame.Elements.FirstOrDefault()?.Page.EntryLastName ?? "Loading...";

            var lockKey = _pageLoading.Lock();

            CancellationToken token;
            lock (_lock)
            {
                Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                token = _cancellationTokenSource.Token;
            }

            Task.Run(() => MoveToAsync(parameter, container, lockKey, token));
        }

        /// <summary>
        /// 遅延移動メイン
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="item"></param>
        /// <param name="lockKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task MoveToAsync(PageFrameMoveParameter parameter, PageFrameContainer container, Locker.Key lockKey, CancellationToken token)
        {
            try
            {
                _loader.RequestLoad(container.FrameRange, parameter.Direction.ToSign());
                await container.WaitLoadAsync(token);
                _ = AppDispatcher.BeginInvoke(() => _box.MoveTo(parameter));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                lockKey.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
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
