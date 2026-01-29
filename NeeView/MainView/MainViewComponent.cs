//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using NeeView.PageFrames;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace NeeView
{
    [LocalDebug]
    [NotifyPropertyChanged]
    public partial class MainViewComponent : INotifyPropertyChanged, IDisposable
    {
        private static MainViewComponent? _current;
        public static MainViewComponent Current => _current ?? throw new InvalidOperationException();


        private readonly MainView _mainView;
        private readonly TouchEmurlateController _touchEmulateController = new();
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private readonly IntervalDelayAction _delayAction = new();
        private readonly Locker _activeLocker = new();
        private readonly Locker _processLocker = new();
        private bool _isProcessing;

        public static void Initialize()
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = new MainViewComponent();
        }

        // TODO: MainView依存はおかしい
        // TODO: 各種シングルトン依存の排除
        private MainViewComponent()
        {
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            var bookHub = BookHub.Current;

            _mainView = new MainView();
            _disposables.Add(_mainView);

            PageFrameBoxPresenter = PageFrameBoxPresenter.Current;

            DragTransformControl = new DragTransformControlProxy(PageFrameBoxPresenter, new DummyDragTransformContextFactory(_mainView, Config.Current.View, Config.Current.Mouse));
            LoupeContext = new LoupeContext(this, Config.Current.Loupe);

            TouchInput = new TouchInput(new TouchInputContext(_mainView.View, _mainView, mouseGestureCommandCollection, PageFrameBoxPresenter, PageFrameBoxPresenter, DragTransformControl, LoupeContext, ViewScrollContext));
            MouseInput = new MouseInput(new MouseInputContext(_mainView.View, _mainView, mouseGestureCommandCollection, PageFrameBoxPresenter, PageFrameBoxPresenter, DragTransformControl, LoupeContext, ViewScrollContext));

            PrintController = new PrintController(this, _mainView, PageFrameBoxPresenter);
            ViewTransformControl = new ViewTransformControl(PageFrameBoxPresenter);
            ViewLoupeControl = new ViewLoupeControl(this);
            ViewAutoScrollControl = new ViewAutoScrollControl(this);
            ViewWindowControl = new ViewWindowControl(this);
            ViewPropertyControl = new ViewPropertyControl(Config.Current.View, Config.Current.BookSetting);
            ViewCopyImage = new ViewCopyImage(PageFrameBoxPresenter);

            PageFrameBoxPresenter.SelectedRangeChanged += PageFrameBoxPresenter_SelectedRangeChanged;
            PageFrameBoxPresenter.SelectedContainerLayoutChanged += PageFrameBoxPresenter_SelectedContainerLayoutChanged;
            PageFrameBoxPresenter.SelectedContentSizeChanged += PageFrameBoxPresenter_SelectedContentSizeChanged;
            PageFrameBoxPresenter.ViewContentChanged += PageFrameBoxPresenter_ViewContentChanged;

            _mainView.DataContext = new MainViewViewModel(this);

            _activeLocker.LockCountChanged += ActiveLocker_LockCountChanged;
            _processLocker.LockCountChanged += ProcessLocker_LockCountChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// コンテキストメニューを開く要求イベント
        /// </summary>
        [Subscribable]
        public event EventHandler? OpenContextMenuRequest;

        /// <summary>
        /// MainViewにフォーカスを移す要求イベント
        /// </summary>
        [Subscribable]
        public event EventHandler? FocusMainViewRequest;


        public MainView MainView => _mainView;

        public PageFrameBackground Background => MainView.PageFrameBackground;

        // ##
        public Size ViewSize => new Size(_mainView.ActualWidth, _mainView.ActualHeight);

        public PageFrameBoxPresenter PageFrameBoxPresenter { get; private set; }

        public IDragTransformControl DragTransformControl { get; private set; }

        public LoupeContext LoupeContext { get; private set; }
        public ViewScrollContext ViewScrollContext => PageFrameBoxPresenter.ViewScrollContext;

        public MouseInput MouseInput { get; private set; }
        public TouchInput TouchInput { get; private set; }

        public PrintController PrintController { get; private set; }

        public IViewTransformControl ViewTransformControl { get; private set; }
        public IViewLoupeControl ViewLoupeControl { get; private set; }
        public IViewAutoScrollControl ViewAutoScrollControl { get; private set; }
        public IViewWindowControl ViewWindowControl { get; private set; }
        public IViewPropertyControl ViewPropertyControl { get; private set; }
        public IViewCopyImage ViewCopyImage { get; private set; }

        public bool IsLoupeMode => ViewLoupeControl.GetLoupeMode();

        public bool IsProcessing
        {
            get { return _isProcessing; }
            set { SetProperty(ref _isProcessing, value); }
        }


        public Window GetWindow()
        {
            return Window.GetWindow(_mainView);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    //ContentCanvas.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Locker.Key LockActiveMarker()
        {
            return _activeLocker.Lock();
        }

        public Locker.Key LockProcessing()
        {
            return _processLocker.Lock();
        }

        private void ActiveLocker_LockCountChanged(object? sender, LockCountChangedEventArgs e)
        {
            _mainView.ActiveMarker.IsActive = e.LockCount > 0;
        }

        private void ProcessLocker_LockCountChanged(object? sender, LockCountChangedEventArgs e)
        {
            IsProcessing = e.LockCount > 0;
        }

        private void PageFrameBoxPresenter_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            var box = PageFrameBoxPresenter.View;
            if (box is null) return;

            LocalDebug.WriteLine($"ViewContentChanged={e}, {e.PageFrameContent?.GetContentRect():f0}");

            // AutoStretch
            if (e.State == ViewContentState.Loaded && e.PageFrameContent is not null && box.Context.AutoStretchTarget.Conflict(e.PageFrameContent.FrameRange))
            {
                LocalDebug.WriteLine($"ViewContentChanged: PageFrameContent={e.PageFrameContent}");
                box.Context.ResetAutoStretchTarget();
                if (Config.Current.MainView.IsFloating && Config.Current.MainView.IsAutoStretch)
                {
                    // NOTE: StretchWindow() はさらなるページ処理を呼ぶ可能性があるためタイミングをずらす
                    // NOTE: 連続して要求が来ることがあるので遅延させる
                    _delayAction.Request(() => _mainView.AutoStretchWindow(), 1, 100);
                }
            }
        }

        // TODO: Selected 情報をまとめたなにか
        private void PageFrameBoxPresenter_SelectedContentSizeChanged(object? sender, EventArgs e)
        {
            MouseInput.UpdateSelectedFrame(FrameChangeType.Size);
            TouchInput.UpdateSelectedFrame(FrameChangeType.Size);
        }

        private void PageFrameBoxPresenter_SelectedContainerLayoutChanged(object? sender, EventArgs e)
        {
            MouseInput.UpdateSelectedFrame(FrameChangeType.Layout);
            TouchInput.UpdateSelectedFrame(FrameChangeType.Layout);
        }

        private void PageFrameBoxPresenter_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            //Debug.WriteLine($"SelectedRangeChanged: {e}");
            var changeType = e.IsMatchAnyEdge() ? FrameChangeType.RangeSize : FrameChangeType.Range;
            MouseInput.UpdateSelectedFrame(changeType);
            TouchInput.UpdateSelectedFrame(changeType);
        }

        public void RaiseOpenContextMenuRequest()
        {
            OpenContextMenuRequest?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseFocusMainViewRequest()
        {
            FocusMainViewRequest?.Invoke(this, EventArgs.Empty);
        }

        public void TouchInputEmulate(object? sender)
        {
            _touchEmulateController.Execute(sender);
        }

        /// <summary>
        /// メインコンテンツの元サイズからの表示スケール値を求める
        /// </summary>
        /// <returns></returns>
        public double GetScaleBaseOnOriginal()
        {
            var presenter = PageFrameBoxPresenter;
            var dpiScaleProvider = MainView.DpiProvider;

            var frameContent = presenter.GetSelectedPageFrameContent();
            var viewContentDirection = frameContent?.ViewContentsDirection ?? +1;
            var contents = (frameContent?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).Direction(viewContentDirection).ToList();
            var viewScale = frameContent?.Transform.Scale ?? 1.0;

            var dpiScaleX = dpiScaleProvider.GetDpiScale().ToFixedScale().DpiScaleX;
            var scale0 = viewScale * GetOriginalScale(contents.ElementAtOrDefault(0)) * dpiScaleX;
            var scale1 = viewScale * GetOriginalScale(contents.ElementAtOrDefault(1)) * dpiScaleX;

            bool isMainContent0 = contents.Count <= 1 || viewContentDirection == 1;  //mainContent == contents[0];
            return isMainContent0 ? scale0 : scale1;

            double GetOriginalScale(ViewContent? content)
            {
                if (content is null) return 1.0;
                var pageElement = content.Element;
                return (frameContent?.PageFrame.Scale ?? 1.0) * pageElement.Scale;
            }
        }
    }
}
