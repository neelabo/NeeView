//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.PageFrames;

namespace NeeView
{
    // TODO: BookMemoryService: RawSourceとViewSource を同列に管理。ページ単位で増減させる方向で。
    [LocalDebug]
    [NotifyPropertyChanged]
    public partial class BookPageLoader : IDisposable, INotifyPropertyChanged
    {
        private readonly BookContext _bookContext;
        private readonly PageFrameFactory _frameFactory;
        private readonly ViewSourceMap _viewSourceMap;
        private readonly PageFrameElementScaleFactory _elementScaleFactory;
        private readonly BookMemoryService _bookMemoryService;
        private readonly PerformanceConfig _performanceConfig;
        private CancellationTokenSource? _cancellationTokenSource;

        private readonly DisposableCollection _disposables = new();
        private bool _disposingValue;
        private bool _disposedValue;

        private readonly PageContentJobClient _jobClient;
        private readonly PageContentJobClient _jobAheadClient;

        private readonly List<Page> _viewPages = new();
        private readonly List<Page> _aheadPages = new();

        private readonly System.Threading.Lock _lock = new();

        private bool _isEnabled = true;
        private BookLoadContext _loadContext = new();
        private LatestValue<BookLoadContext> _latestContext = new();

        private bool _isBusy;
        private int _busyCount;

        public BookPageLoader(BookContext bookContext, PageFrameFactory frameFactory, ViewSourceMap viewSourceMap, PageFrameElementScaleFactory elementScaleFactory, BookMemoryService bookMemoryService, PerformanceConfig performanceConfig)
        {
            _bookContext = bookContext;
            _frameFactory = frameFactory;
            _viewSourceMap = viewSourceMap;
            _elementScaleFactory = elementScaleFactory;
            _bookMemoryService = bookMemoryService;
            _performanceConfig = performanceConfig;

            _jobClient = new("View", JobCategories.PageViewContentJobCategory);
            _disposables.Add(_jobClient);

            _jobAheadClient = new("Ahead", JobCategories.PageAheadContentJobCategory);
            _disposables.Add(_jobAheadClient);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }


        private void IncrementBusyCount()
        {
            lock (_lock)
            {
                IsBusy = ++_busyCount > 0;
            }
        }

        private void DecrementBusyCount()
        {
            lock (_lock)
            {
                IsBusy = --_busyCount > 0;
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposingValue = true;
                if (disposing)
                {
                    Cancel();
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void Pause()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _isEnabled = false;
                _loadContext = new BookLoadContext();
            }
        }

        public void Resume()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _isEnabled = true;
                if (_loadContext.IsValid)
                {
                    RequestLoad(_loadContext);
                }
            }
        }

        public void RequestLoad(PageRange range, int direction)
        {
            if (_disposedValue) return;

            RequestLoad(new BookLoadContext(range, direction, _performanceConfig.PreLoadSize));
        }

        public void RequestLoad(BookLoadContext context)
        {
            if (_disposedValue) return;
            if (context.PageRange.IsEmpty()) return;

            lock (_lock)
            {
                if (!_isEnabled)
                {
                    _loadContext = context;
                    return;
                }
            }

            Task.Run(() => LoadAsync(context, CancellationToken.None));
        }

        private async Task LoadAsync(BookLoadContext context, CancellationToken token)
        {
            Debug.Assert(context.Direction is 1 or -1);

            if (_disposedValue || _disposingValue) return;

            var operation = _latestContext.CompareSet(context);
            if (operation is null) return;

            lock (_lock)
            {
                LocalDebug.WriteLine($"LoadAsync: {context}");
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            var (range, direction, limit) = context;
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);
            linkedTokenSource.Token.ThrowIfCancellationRequested();

            try
            {
                IncrementBusyCount();

                linkedTokenSource.Token.ThrowIfCancellationRequested();

                // ページ状態クリア
                ClearPageState();

                // 指定分読み込み
                LocalDebug.WriteLine($"LoadMainAsync...");
                await LoadMainAsync(range, direction, linkedTokenSource.Token);
                LocalDebug.WriteLine($"LoadMainAsync done.");

                // 先読み：次１ページ
                var next = await LoadAheadAsync(range.Next(direction), direction, 1, linkedTokenSource.Token);
                var nextCount = next.Count;

                // 先読み：前１ページ
                var prev = await LoadAheadAsync(range.Next(-direction), -direction, 1, linkedTokenSource.Token);
                var prevCount = prev.Count;

                // 先読み：次ページ残り
                next = await LoadAheadAsync(next.Position, direction, limit - nextCount, linkedTokenSource.Token);
                nextCount += next.Count;

                // 先読み：前ページ残り
                var rest = limit - (next.Count + prevCount);
                if (0 < rest)
                {
                    await LoadAheadAsync(prev.Position, -direction, rest, linkedTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                //Debug.WriteLine($"BookPageLoader.Canceled: {ex.StackTrace}");
            }
            finally
            {
                operation.Dispose();
                DecrementBusyCount();
            }

            //Debug.WriteLine($"BookPageLoader: Views.Count = {_book.Pages.Count(e => e.State == PageContentState.View)}");
            //Debug.WriteLine($"BookPageLoader: Ahead.Count = {_book.Pages.Count(e => e.State == PageContentState.Ahead)}");
        }

        private void ClearPageState()
        {
            lock (_lock)
            {
                foreach (var page in _viewPages)
                {
                    page.State = PageContentState.None;
                }
                foreach (var page in _aheadPages)
                {
                    page.State = PageContentState.None;
                }

                _viewPages.Clear();
                _aheadPages.Clear();

                Debug.Assert(_bookContext.Pages.All(e => e.State == PageContentState.None));
            }
        }


        public void Cancel()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _loadContext = new BookLoadContext();
            }
        }


        /// <summary>
        /// 表示ページ読み込み
        /// </summary>
        /// <param name="range">ページ範囲</param>
        /// <param name="direction">ページ方向</param>
        /// <param name="token">キャンセルトークン</param>
        private async Task LoadMainAsync(PageRange range, int direction, CancellationToken token)
        {
            var indexes = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1).ToList();
            var pages = indexes.Direction(direction).Select(e => _bookContext.GetPage(e, true)).WhereNotNull().ToList();

            lock (_lock)
            {
                token.ThrowIfCancellationRequested();
                foreach (var page in pages)
                {
                    _viewPages.Add(page);
                    page.State = PageContentState.View;
                }
            }

            var contents = pages.Cast<IPageContentLoader>().ToList();
            //foreach (var page in pages)
            //{
            //    Debug.WriteLine($"BookPageLoader.LoadMainAsync: Job.{page}");
            //}
            _jobClient.Order(contents);
            await _jobClient.WaitAsync(contents, -1, token);

            // NOTE: ViewSource は表示部で作成される
        }

        /// <summary>
        /// ページ先読み
        /// </summary>
        /// <param name="position">先読み開始位置</param>
        /// <param name="direction">先読み方向</param>
        /// <param name="limit">先読みページ数上限</param>
        /// <param name="token">キャンセルトークン</param>
        private async Task<(int Count, PagePosition Position)> LoadAheadAsync(PagePosition position, int direction, int limit, CancellationToken token)
        {
            var count = 0;
            var pos = position;

            // 開始位置をページ先頭に補正
            if (pos.Part == 0 && direction == -1)
            {
                pos = pos - 1;
            }
            else if (pos.Part == 1 && direction == 1)
            {
                pos = pos + 1;
            }
            if (!_bookContext.ContainsIndex(pos.Index)) return (0, position);
            Debug.Assert((pos.Part == 0 && direction == 1) || (pos.Part == 1 && direction == -1));

            while (count < limit)
            {
                var frame = await LoadAheadCoreAsync(pos, direction, token);
                if (frame is null) break;

                count += frame.Elements.Count;
                pos = frame.FrameRange.Next(direction);
            }

            return (count, pos);
        }

        public async Task<PageFrame?> LoadAheadCoreAsync(PagePosition pos, int direction, CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            // メモリ許容チェック
            _bookMemoryService.Cleanup(pos.Index, direction);
            if (_bookMemoryService.IsFull)
            {
                Debug.WriteLine($"BookPageLoader: Memory full.");
                return null;
            }

            // つぎのフレーム作成
            var frame = _frameFactory.CreatePageFrame(pos, direction);
            if (frame is null) return null;

            // ページ読み込み
            var pages = frame.Elements.Select(e => e.Page).ToList();

            lock (_lock)
            {
                token.ThrowIfCancellationRequested();
                foreach (var page in pages)
                {
                    _aheadPages.Add(page);
                    page.State = PageContentState.Ahead;
                }
            }

            var contents = pages.Cast<IPageContentLoader>().ToList();
            //foreach (var page in pages)
            //{
            //    Debug.WriteLine($"BookPageLoader.LoadAheadAsync[{Thread.CurrentThread.ManagedThreadId}]: Job.{page}");
            //}
            _jobAheadClient.Order(contents);
            await _jobAheadClient.WaitAsync(contents, -1, token);

            // フレーム確定
            frame = _frameFactory.CreatePageFrame(pos, direction);
            if (frame is null) return null;

            // ViewSourceを作成
            foreach (var element in frame.Elements)
            {
                token.ThrowIfCancellationRequested();
                var viewSource = _viewSourceMap.Get(element.Page, element.PagePart, element.PageDataSource);
                var viewContentSize = ViewContentSizeFactory.Create(element, _elementScaleFactory.Create(frame));
                var pictureSize = viewContentSize.GetPictureSize();
                await viewSource.LoadAsync(pictureSize, token);
            }
        
            // 先読み完了したフレームを返す
            return frame;
        }

    }


    public record class BookLoadContext(PageRange PageRange, int Direction, int Limit)
    {
        public BookLoadContext() : this(PageRange.Empty, 0, 0) { }

        public bool IsValid => !PageRange.IsEmpty();
    }



    public class BookPageLoadPause : IDisposable
    {
        public BookPageLoader _loader;

        public BookPageLoadPause(BookPageLoader loader)
        {
            _loader = loader;
            _loader.Pause();
        }

        public void Dispose()
        {
            _loader.Resume();
        }
    }


}
