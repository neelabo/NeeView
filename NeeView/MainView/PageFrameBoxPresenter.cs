using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Threading.Tasks;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class PageFrameBoxChangingEventArgs : BookChangingEventArgs
    {
        public PageFrameBoxChangingEventArgs(PageFrameBox? box, BookChangingEventArgs e)
            : base(e.Address)
        {
            Box = box;
        }

        public PageFrameBox? Box { get; }
    }

    public class PageFrameBoxChangedEventArgs : BookChangedEventArgs
    {
        public PageFrameBoxChangedEventArgs(PageFrameBox? box, BookChangedEventArgs e)
            : base(e.Address, e.Book, e.BookMementoType)
        {
            Box = box;
        }

        public PageFrameBox? Box { get; }
    }

    public partial interface INotifyPageFrameBoxChanged
    {
        [Subscribable]
        public event EventHandler<PageFrameBoxChangingEventArgs>? PageFrameBoxChanging;

        [Subscribable]
        public event EventHandler<PageFrameBoxChangedEventArgs>? PageFrameBoxChanged;
    }


    [NotifyPropertyChanged]
    public partial class PageFrameBoxPresenter : INotifyPropertyChanged, IDragTransformContextFactory, IBookPageContext, IDisposable, INotifyPageFrameBoxChanged
    {
        public static PageFrameBoxPresenter Current { get; } = new PageFrameBoxPresenter();

        private readonly Config _config;
        private readonly BookHub _bookHub;
        private readonly BookShareContext _shareContext;
        private bool _isLoading;
        private string? _emptyMessage;
        private readonly ReferenceCounter _loadRequestCount = new();
        private CancellationTokenSource _openCancellationTokenSource = new();

        private List<Page> _viewPages = new();
        private List<ViewContent> _viewContents = new();
        private readonly System.Threading.Lock _lock = new();
        private ViewPageChangedEventArgs? _viewPageChangedEventArgs;
        private bool _disposedValue;

        private PageFrameBoxContext? _box;
        private PageFrameBoxContext? _boxView;
        private PageFrameBoxContext? _boxNext;
        private DisposableCollection? _boxDisposables;


        private PageFrameBoxPresenter()
        {
            _config = Config.Current;
            _bookHub = BookHub.Current;

            _shareContext = new BookShareContext(_config);

            _bookHub.LoadRequesting += BookHub_LoadRequesting;
            _bookHub.LoadRequested += BookHub_LoadRequested;
            _bookHub.BookChanging += BookHub_BookChanging;
            _bookHub.BookChanged += BookHub_BookChanged;

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler<PageFrameBoxChangingEventArgs>? PageFrameBoxChanging;

        [Subscribable]
        public event EventHandler<PageFrameBoxChangedEventArgs>? PageFrameBoxChanged;

        [Subscribable]
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;

        [Subscribable]
        public event EventHandler? SelectedContainerLayoutChanged;

        [Subscribable]
        public event EventHandler? SelectedContentSizeChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;

        [Subscribable]
        public event EventHandler? StretchChanged;

        [Subscribable]
        public event SizeChangedEventHandler? ViewSizeChanged;

        // ロード中通知
        [Subscribable]
        public event EventHandler<BookPathEventArgs>? Loading;

        [Subscribable]
        public event EventHandler<ViewPageChangedEventArgs>? ViewPageChanged;

        [Subscribable]
        private event EventHandler<IsSortBusyChangedEventArgs>? IsSortBusyChanged;

        [Subscribable]
        public event EventHandler<PageTerminatedEventArgs>? PageTerminated;


        public ViewScrollContext ViewScrollContext { get; } = new();

        public bool IsEnabled => _box != null;

        public bool IsLoading => _isLoading;

        public Book? Book => _box?.Book;

        public IReadOnlyList<Page> Pages => _box?.Pages ?? new List<Page>();

        public PageRange SelectedRange
        {
            get => _box?.SelectedRange ?? new PageRange();
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get
            {
                var box = _box;
                if (box is null) return new List<Page>();
                return box.SelectedRange.CollectPositions().Select(e => box.Pages[e.Index]).Distinct().ToList();
            }
        }

        /// <summary>
        /// 安定した選択ページ<br/>
        /// TODO: 必要性の検証。SelectedPages で十分では？
        /// </summary>
        public IReadOnlyList<Page> ViewPages => _viewPages;

        public List<ViewContent> ViewContents => _viewContents;

        public PageFrameBox? ValidPageFrameBox => ValidBox();


        public PageFrameBox? View => _boxView?.Box;
        public PageFrameBox? ViewNext => _boxNext?.Box;
        public double ViewWidth => View?.ActualWidth ?? 0.0;
        public double ViewHeight => View?.ActualHeight ?? 0.0;

        public bool IsMedia => _box?.Book.IsMedia ?? false;

        public string? EmptyMessage
        {
            get { return _emptyMessage; }
            set { SetProperty(ref _emptyMessage, value); }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachPageFrameBoxContext();
                    ClearViewPages();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// ページ変更回数を無視して履歴保存を実行する
        /// </summary>
        public void ForceSaveBookMemento()
        {
            if (_box is null) return;

            _box.BookMementoControl.IsPageChangeCountEnabled = false;
            _box.BookMementoControl.SaveBookMemento();
        }

        /// <summary>
        /// 最後に開いたブックを記憶する
        /// </summary>
        public void SaveLastBookMemento()
        {
            if (_box is null) return;

            if (Config.Current.StartUp.IsOpenLastBook)
            {
                var bookMemento = _box.BookMementoControl.CreateBookMement();
                Config.Current.StartUp.LastBook = bookMemento;
            }
            else
            {
                Config.Current.StartUp.LastBook = null;
            }
        }

        private void BookHub_LoadRequesting(object? sender, BookPathEventArgs e)
        {
            _loadRequestCount.Increment();
        }

        private void BookHub_LoadRequested(object? sender, BookPathEventArgs e)
        {
            _loadRequestCount.Decrement();
        }


        private void BookHub_BookChanging(object? sender, BookChangingEventArgs e)
        {
            _openCancellationTokenSource.Cancel();
            _openCancellationTokenSource.Dispose();
            _openCancellationTokenSource = new();

            _box?.BookMementoControl?.SaveBookMemento();

            AppDispatcher.Invoke(() =>
            {
                View?.FlushLayout();
                DetachPageFrameBoxContext();
            });

            AppDispatcher.BeginInvoke(() =>
            {
                SetLoading(e.Address);
                PageFrameBoxChanging?.Invoke(this, new PageFrameBoxChangingEventArgs(null, e));
            });
        }


        private void BookHub_BookChanged(object? sender, BookChangedEventArgs e)
        {
            var token = _openCancellationTokenSource.Token;
            if (token.IsCancellationRequested) return;

            AppDispatcher.BeginInvoke(async () =>
            {
                try
                {
                    // NOTE: ブックの切替時にLOHを含めた積極的ガベージコレクションを行う
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);

                    EmptyMessage = null;
                    await OpenAsync(_bookHub.GetCurrentBook(), token);
                    SetLoading(null);
                    EmptyMessage = e.EmptyMessage;
                    PageFrameBoxChanged?.Invoke(this, new PageFrameBoxChangedEventArgs(_box?.Box, e));
                    RaiseViewPageChanged();
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        private void SetLoading(string? address)
        {
            _isLoading = !string.IsNullOrEmpty(address);
            RaisePropertyChanged(nameof(IsLoading));
            Loading?.Invoke(this, new BookPathEventArgs(address));
        }

        private async ValueTask OpenAsync(Book? book, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DetachPageFrameBoxContext();
            ClearViewPages();

            if (book is null || book.Pages.Count <= 0)
            {
                StagePageFrameBoxContext();
                ReleasePageFrameBoxContext();
                RaisePropertyChanged(null);
                PagesChanged?.Invoke(this, EventArgs.Empty);
                SelectedRangeChanged?.Invoke(this, PageRangeChangedEventArgs.Empty);

                ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(ViewContentChangedAction.ContentLoaded, null, Array.Empty<ViewContent>(), 1));
                RaiseViewPageChanged(new ViewPageChangedEventArgs(Array.Empty<Page>()));

                return;
            }

            var box = new PageFrameBoxContext(_shareContext, book);
            AttachPageFrameBoxContext(box);
            StagePageFrameBoxContext();

            // NOTE: 表示開始時の最初のサイズ変更を回避する
            using var key = PageFrameProfile.ReferenceSizeLocker.Lock();

            RaisePropertyChanged(nameof(ViewNext));

            await WaitStableAsync(box.Box, token);

            ReleasePageFrameBoxContext();

            box.BookMementoControl.TrySaveBookMemento();

            RaisePropertyChanged(null);

            PagesChanged?.Invoke(this, EventArgs.Empty);
            SelectedRangeChanged?.Invoke(this, new PageRangeChangedEventArgs(box.SelectedRange, PageRange.Empty));
        }


        /// <summary>
        /// ページ表示完了まで待機
        /// </summary>
        /// <param name="box"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async ValueTask WaitStableAsync(PageFrameBox box, CancellationToken token)
        {
            if (!IsLoading()) return;

            using var eventFlag = new ManualResetEventSlim();
            using var viewContentChangedEvent = SubscribeViewContentChanged((s, e) => eventFlag.Set());
            while (IsLoading())
            {
                await eventFlag.WaitHandle.AsTask().WaitAsync(token);
                eventFlag.Reset();
            }

            bool IsLoading()
            {
                return !box.IsStarted || box.IsSelectedPageFrameLoading();
            }
        }

        /// <summary>
        /// ブック安定まで待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async ValueTask WaitForBookStableAsync(CancellationToken token)
        {
            if (!IsLoading()) return;

            using var eventFlag = new ManualResetEventSlim();
            using var requestingEvent = _loadRequestCount.SubscribeChanged((s, e) => eventFlag.Set());
            using var loadingEvent = SubscribeLoading((s, e) => eventFlag.Set());
            using var movingEvent = FolderList.SubscribeIsMovingChanged((s, e) => eventFlag.Set());
            while (IsLoading())
            {
                await eventFlag.WaitHandle.AsTask().WaitAsync(token);
            }

            bool IsLoading()
            {
                return _loadRequestCount.IsActive || _isLoading || FolderList.IsMoving;
            }
        }

        /// <summary>
        /// 表示ページ安定まで待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask WaitForViewPageStableAsync(CancellationToken token)
        {
            await WaitForBookStableAsync(token);

            var box = _box;
            if (box is null) return;

            var boxToken = _openCancellationTokenSource.Token;

            try
            {
                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, boxToken);
                await WaitStableAsync(box.Box, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                if (boxToken.IsCancellationRequested)
                {
                    await WaitForViewPageStableAsync(token);
                    return;
                }
                throw;
            }
        }


        private void ClearViewPages()
        {
            _viewPages = [];
            _viewContents = [];
        }


        private void RaiseViewPageChanged()
        {
            lock (_lock)
            {
                if (_viewPageChangedEventArgs is null) return;
                RaiseViewPageChanged(_viewPageChangedEventArgs);
            }
        }

        private void RaiseViewPageChanged(ViewPageChangedEventArgs e)
        {
            lock (_lock)
            {
                _viewPageChangedEventArgs = e;
                if (!_isLoading)
                {
                    ViewPageChanged?.Invoke(this, _viewPageChangedEventArgs);
                    _viewPageChangedEventArgs = null;
                }
            }
        }

        #region Box events

        private void Box_IsSortBusyChanged(object? sender, IsSortBusyChangedEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => IsSortBusyChanged?.Invoke(this, e));
        }

        private void Box_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            ViewContentChanged?.Invoke(this, e);

            if (e.State < ViewContentState.Loaded) return;

            _viewContents = new List<ViewContent>(e.ViewContents);

            var pages = e.ViewContents.Select(e => e.Page).Distinct().ToList();
            if (_viewPages.SequenceEqual(pages)) return;

            _viewPages = pages;

            RaiseViewPageChanged(new ViewPageChangedEventArgs(_viewPages));
        }


        private void Box_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Category)
            {
                case TransformCategory.Loupe:
                    ShowLoupeTransformMessage(e.Source, e.Action);
                    break;
                case TransformCategory.Content:
                    var originalScale = e is OriginalScaleTransformChangedEventArgs arg ? arg.OriginalScale : 1.0;
                    ShowContentTransformMessage(e.Source, e.Action, e.Trigger, originalScale);
                    break;
            }

            TransformChanged?.Invoke(this, e);
        }

        private void Box_StretchChanged(object? sender, EventArgs e)
        {
            StretchChanged?.Invoke(this, e);
        }

        // TODO: Selected の情報をまとめたクラスみたいなものがほしいかも？
        private void Box_SelectedContainerLayoutChanged(object? sender, EventArgs e)
        {
            SelectedContainerLayoutChanged?.Invoke(this, e);
        }

        private void Box_SelectedContentSizeChanged(object? sender, EventArgs e)
        {
            SelectedContentSizeChanged?.Invoke(this, e);
        }

        private void Box_ViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewSizeChanged?.Invoke(this, e);
        }

        private void Box_PagesChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Pages));
            PagesChanged?.Invoke(this, e);
        }

        private void Box_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedRange));
            SelectedRangeChanged?.Invoke(this, e);
        }

        private void Box_PageTerminated(object? sender, PageTerminatedEventArgs e)
        {
            PageTerminated?.Invoke(this, e);
        }

        #endregion Box events

        private static void ShowLoupeTransformMessage(ITransformControlObject source, TransformAction action)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (scale != 1.0)
                    {
                        infoMessage.SetMessage(InfoMessageType.ViewTransform, $"×{scale:0.0}");
                    }
                    break;
            }
        }

        private void ShowContentTransformMessage(ITransformControlObject source, TransformAction action, TransformTrigger trigger, double originalScale)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;
            if (!trigger.IsManualTrigger()) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (Config.Current.Notice.IsOriginalScaleShowMessage)
                    {
                        var dpi = (Window.GetWindow(this.View) is IDpiScaleProvider dpiProvider) ? dpiProvider.GetDpiScale().ToFixedScale().DpiScaleX : 1.0;
                        scale = scale * originalScale * dpi;
                    }
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(scale * 100.0 + 0.1)}%");
                    break;
                case TransformAction.Angle:
                    var angle = ((IAngleControl)source).Angle;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(angle)}°");
                    break;
                case TransformAction.FlipHorizontal:
                    var isFlipHorizontal = ((IFlipControl)source).IsFlipHorizontal;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, TextResources.GetString("Notice.FlipHorizontal") + " " + (isFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformAction.FlipVertical:
                    var isFlipVertical = ((IFlipControl)source).IsFlipVertical;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, TextResources.GetString("Notice.FlipVertical") + " " + (isFlipVertical ? "ON" : "OFF"));
                    break;
            }
        }

        #region IPageFrameBox

        private PageFrameBox? ValidBox()
        {
            return IsLoading ? null : _box?.Box;
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(bool isPointContainer)
        {
            return _box?.CreateContentDragTransformContext(isPointContainer);
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(PageFrameContainer container)
        {
            return _box?.CreateContentDragTransformContext(container);
        }

        public LoupeDragTransformContext? CreateLoupeDragTransformContext()
        {
            return _box?.CreateLoupeDragTransformContext() ?? CreateLoupeDragTransformContextDummy();
        }

        private LoupeDragTransformContext? CreateLoupeDragTransformContextDummy()
        {
            var transformControl = new DummyTransformControl();
            var dragContext = new LoupeDragTransformContext(MainViewComponent.Current.MainView, transformControl, Config.Current.View, Config.Current.Mouse, Config.Current.Loupe);
            dragContext.Initialize(new Point(), System.Environment.TickCount);
            return dragContext;
        }

        // TODO: 呼ばれない？
        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin, bool allowScroll)
        {
            ValidBox()?.ScrollToNextFrame(direction, parameter, lineBreakStopMode, endMargin, allowScroll);
        }

        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter)
        {
            return ValidBox()?.ScrollToNext(direction, parameter) ?? false;
        }

        public void ResetTransform()
        {
            ValidBox()?.ResetTransform();
        }

        public void Stretch(bool ignoreViewOrigin)
        {
            ValidBox()?.Stretch(ignoreViewOrigin, TransformTrigger.None);
        }

        public void FlushLayout()
        {
            ValidBox()?.FlushLayout();
        }

        public PageFrameTransformAccessor? CreateSelectedTransform()
        {
            return _box?.Box.CreateSelectedTransform();
        }

        /// <summary>
        /// 選択中の <see cref="PageFrameContent"/> を取得
        /// </summary>
        /// <returns></returns>
        public PageFrameContent? GetSelectedPageFrameContent()
        {
            return _box?.Box.GetSelectedPageFrameContent();
        }

        #endregion IPageFrameBox


        #region PageFrameBoxContext

        private void AttachPageFrameBoxContext(PageFrameBoxContext boxContext)
        {
            DetachPageFrameBoxContext();

            _boxDisposables =
            [
                boxContext.SubscribeIsSortBusyChanged(Box_IsSortBusyChanged),
                boxContext.SubscribePagesChanged(Box_PagesChanged),
                boxContext.SubscribeSelectedRangeChanged(Box_SelectedRangeChanged),
                boxContext.SubscribeViewContentChanged(Box_ViewContentChanged),
                boxContext.SubscribeTransformChanged(Box_TransformChanged),
                boxContext.SubscribeStretchChanged(Box_StretchChanged),
                boxContext.SubscribeSelectedContainerLayoutChanged(Box_SelectedContainerLayoutChanged),
                boxContext.SubscribeSelectedContentSizeChanged(Box_SelectedContentSizeChanged),
                boxContext.SubscribeViewSizeChanged(Box_ViewSizeChanged),
                boxContext.SubscribePageTerminated(Box_PageTerminated),
            ];

            _box = boxContext;
        }

        private void DetachPageFrameBoxContext()
        {
            if (_box is null) return;
            _boxDisposables?.Dispose();
            _boxDisposables = null;
            _box.Dispose();
            _box = null;
        }

        // TODO: _boxNext は最初のページ表示までに必要なフレームサイズ取得用にVisualTreeに接続するためのものだが、接続せずに計算させる方法を考える
        private void StagePageFrameBoxContext()
        {
            _boxNext?.Dispose();
            _boxNext = _box;
            RaisePropertyChanged(nameof(ViewNext));
        }

        private void ReleasePageFrameBoxContext()
        {
            _boxView?.Dispose();
            _boxView = _boxNext;
            _boxView?.Box.SetVisibility(Visibility.Visible);
            _boxNext = null;
            RaisePropertyChanged(nameof(View));
            RaisePropertyChanged(nameof(ViewNext));
        }

        #endregion PageFrameBoxContext
    }

}
