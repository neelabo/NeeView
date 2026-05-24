//#define LOCAL_DEBUG

using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.Maths;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrameBox 状態情報
    /// </summary>
    [LocalDebug]
    public partial class PageFrameContext : ObservableObject, IStaticFrame, IDisposable, IContentSizeCalculatorProfile
    {
        private readonly BookShareContext _shareContext;
        private readonly Config _config;
        private readonly BookSettingConfig _bookSetting;
        private readonly PageFrameProfile _frameProfile;
        private readonly SlideShow _slideshow;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;
        private readonly BooleanLockValue _isSnapAnchor = new();
        private readonly ViewScrollContext _viewScrollContext;
        private readonly bool _isMediaBook;
        private PageRange _autoStretchTarget = PageRange.Empty;
        private bool _ignoreScaleStretchTracking;


        public PageFrameContext(BookShareContext shareContext, bool isMediaBook)
        {
            _shareContext = shareContext;
            _config = shareContext.Config;
            _viewScrollContext = shareContext.ViewScrollContext;
            _isMediaBook = isMediaBook;
            _bookSetting = _config.BookSetting;
            _slideshow = SlideShow.Current;

            _frameProfile = new PageFrameProfile(_config);
            _disposables.Add(_frameProfile);

            LoupeScale = _config.Loupe.DefaultScale;

            _disposables.Add(_config.Book.SubscribePropertyChanged(BookConfig_PropertyChanged));
            _disposables.Add(_config.View.SubscribePropertyChanged(ViewConfig_PropertyChanged));
            _disposables.Add(_config.System.SubscribePropertyChanged(SystemConfig_PropertyChanged));
            _disposables.Add(_config.SlideShow.SubscribePropertyChanged(SlideShowConfig_PropertyChanged));
            _disposables.Add(_bookSetting.SubscribePropertyChanged((s, e) => AppDispatcher.BeginInvoke(() => BookSetting_PropertyChanged(s, e))));
            _disposables.Add(_frameProfile.SubscribePropertyChanged(FrameProfile_PropertyChanged));
            _disposables.Add(ImageResizeFilterConfig.SubscribePropertyChanged((s, e) => OnPropertyChanged(nameof(ImageResizeFilterConfig))));
            _disposables.Add(ImageCustomSizeConfig.SubscribePropertyChanged((s, e) => OnPropertyChanged(nameof(ImageCustomSizeConfig))));
            _disposables.Add(ImageTrimConfig.SubscribePropertyChanged((s, e) => OnPropertyChanged(nameof(ImageTrimConfig))));
            _disposables.Add(ImageDotKeepConfig.SubscribePropertyChanged((s, e) => OnPropertyChanged(nameof(ImageDotKeepConfig))));
            _disposables.Add(_config.Image.Standard.SubscribePropertyChanged(nameof(ImageStandardConfig.IsAspectRatioEnabled), (s, e) => OnPropertyChanged(nameof(IsAspectRatioEnabled))));
            _disposables.Add(() => _viewScrollContext.Clear());
            _disposables.Add(_slideshow.SubscribePropertyChanged(nameof(SlideShow.IsPlaying), SlideShowIsPlayngChanged));

            UpdatePageEndAction();
            UpdatePageChangeType();
        }

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanging;

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanged;


        public BookShareContext ShareContext => _shareContext;
        public ViewScrollContext ViewScrollContext => _viewScrollContext;

        public bool IsPanorama => _config.Book.IsPanorama;
        public PageFrameOrientation FrameOrientation => _config.Book.Orientation;
        public double FrameMargin => IsStaticFrame ? 1.0 : _config.Book.FrameSpace;
        public double DividePageRate => _config.Book.DividePageRate;

        // TODO: 更新イベントが余計に発生している？Propertyパターンにして抑制させることも可能(優先度低)
        public double ContentsSpace => FramePageSize == 2 ? _config.Book.ContentsSpace : 0.0;
        public PageStretchMode StretchMode => _config.Book.IsPanorama && _config.View.StretchMode == PageStretchMode.Uniform
            ? _config.Book.Orientation == PageFrameOrientation.Horizontal ? PageStretchMode.UniformToVertical : PageStretchMode.UniformToHorizontal
            : _config.View.StretchMode;

        public bool IsInsertDummyPage => _config.Book.IsInsertDummyPage;
        public bool IsInsertDummyFirstPage => _config.Book.IsInsertDummyPage && _config.Book.IsInsertDummyFirstPage;
        public bool IsInsertDummyLastPage => _config.Book.IsInsertDummyPage && _config.Book.IsInsertDummyLastPage;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(IsLoopPage))]
        public partial PageEndAction PageEndAction { get; private set; }

        public bool IsLoopPage => !_isMediaBook && PageEndAction == PageEndAction.SeamlessLoop;
        public bool CanPrioritizePageMove => _config.Book.IsPrioritizePageMove && !_slideshow.IsPlaying;
        public bool IsReadyToPageMove => _config.Book.IsReadyToPageMove && !_config.Book.IsPanorama;
        public bool IsNotifyPageLoop => _config.Book.IsNotifyPageLoop && !_slideshow.IsPlaying;
        public bool IsStaticWidePage => _config.Book.IsStaticWidePage && _bookSetting.PageMode == PageMode.WidePage;
        public WidePageStretch WidePageStretch => _config.Book.WidePageStretch;
        public WidePageVerticalAlignment WidePageVerticalAlignment => _config.Book.WidePageVerticalAlignment;

        public bool AllowFileContentAutoRotate => _config.View.AllowFileContentAutoRotate;
        public bool AllowEnlarge => _config.View.AllowStretchScaleUp;
        public bool AllowReduce => _config.View.AllowStretchScaleDown;
        public bool IsFlipLocked => _config.View.IsKeepFlip;
        public bool IsScaleStretchTracking => !_ignoreScaleStretchTracking && _config.View.IsScaleStretchTracking && !_config.View.IsKeepScale;
        public bool IsScaleLocked => _config.View.IsKeepScale;
        public bool IsAngleLocked => _config.View.IsKeepAngle;
        public bool IsIgnoreImageDpi => _config.System.IsIgnoreImageDpi;

        public bool IsAutoStretch => _config.MainView.IsFloating && _config.MainView.IsAutoStretch;
        public Locker ForceScaleStretchTracking { get; } = new();
        public bool ShouldScaleStretchTracking => IsScaleStretchTracking || ForceScaleStretchTracking.IsLocked;

        public PageMode PageMode => _bookSetting.PageMode;
        public int FramePageSize => _config.GetFramePageSize(_bookSetting.PageMode);
        public PageReadOrder ReadOrder => _bookSetting.BookReadOrder;
        public bool IsSupportedDividePage => _bookSetting.IsSupportedDividePage && _bookSetting.PageMode == PageMode.SinglePage;
        public bool IsSupportedWidePage => _bookSetting.IsSupportedWidePage && FramePageSize == 2;
        public bool IsSupportedSingleFirstPage => _bookSetting.IsSupportedSingleFirstPage && FramePageSize == 2;
        public bool IsSupportedSingleLastPage => _bookSetting.IsSupportedSingleLastPage && FramePageSize == 2;
        public bool IsRecursiveFolder => _bookSetting.IsRecursiveFolder;
        public AutoRotateType AutoRotate => _bookSetting.AutoRotate;
        public AutoRotatePolicy AutoRotatePolicy => _config.View.AutoRotatePolicy;
        public double BaseScale => _bookSetting.BaseScale;

        public bool IsStaticFrame => _frameProfile.IsStaticFrame;
        public Size CanvasSize => _frameProfile.CanvasSize;
        public Size ReferenceSize => _frameProfile.ReferenceSize;
        public DpiScale DpiScale => _frameProfile.DpiScale;

        public ViewConfig ViewConfig => _config.View;
        public PerformanceConfig PerformanceConfig => _config.Performance;
        public ImageResizeFilterConfig ImageResizeFilterConfig => _config.ImageResizeFilter;
        public ImageCustomSizeConfig ImageCustomSizeConfig => _config.ImageCustomSize;
        public ImageTrimConfig ImageTrimConfig => _config.ImageTrim;
        public ImageDotKeepConfig ImageDotKeepConfig => _config.ImageDotKeep;
        public bool IsAspectRatioEnabled => _config.Image.Standard.IsAspectRatioEnabled;

        public TimeSpan ScrollDuration => TimeSpan.FromSeconds(_config.View.ScrollDuration);

        [ObservableProperty]
        public partial PageMoveType PageChangeType { get; private set; }

        public TimeSpan PageChangeDuration => _config.Book.IsPanorama ? ScrollDuration : TimeSpan.FromSeconds(_slideshow.IsPlaying ? _config.SlideShow.PageMoveDuration : _config.View.PageMoveDuration);

        [ObservableProperty]
        public partial double LoupeScale { get; private set; }

        public BooleanLockValue IsSnapAnchor => _isSnapAnchor;

        public PageRange AutoStretchTarget => _autoStretchTarget;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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

        private void UpdatePageEndAction()
        {
            PageEndAction = _slideshow.IsPlaying ? _config.SlideShow.PageEndAction : _config.Book.PageEndAction;
        }

        private void UpdatePageChangeType()
        {
            if (PageChangeDuration == TimeSpan.Zero)
            {
                PageChangeType = PageMoveType.Scroll;
            }
            else
            {
                PageChangeType = _config.Book.IsPanorama ? PageMoveType.Scroll : _slideshow.IsPlaying ? _config.SlideShow.PageMoveType : _config.View.PageMoveType;
            }
        }

        private void BookConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookConfig.IsPanorama):
                    OnPropertyChanged(nameof(IsPanorama));
                    OnPropertyChanged(nameof(StretchMode));
                    OnPropertyChanged(nameof(IsReadyToPageMove));
                    UpdatePageChangeType();
                    break;

                case nameof(BookConfig.Orientation):
                    OnPropertyChanged(nameof(FrameOrientation));
                    OnPropertyChanged(nameof(StretchMode));
                    break;

                case nameof(BookConfig.FrameSpace):
                    OnPropertyChanged(nameof(FrameMargin));
                    break;

                case nameof(BookConfig.ContentsSpace):
                    OnPropertyChanged(nameof(ContentsSpace));
                    break;

                case nameof(BookConfig.DividePageRate):
                    OnPropertyChanged(nameof(DividePageRate));
                    break;

                case nameof(BookConfig.IsInsertDummyPage):
                    OnPropertyChanged(nameof(IsInsertDummyPage));
                    OnPropertyChanged(nameof(IsInsertDummyFirstPage));
                    OnPropertyChanged(nameof(IsInsertDummyLastPage));
                    break;

                case nameof(BookConfig.IsInsertDummyFirstPage):
                    OnPropertyChanged(nameof(IsInsertDummyFirstPage));
                    break;

                case nameof(BookConfig.IsInsertDummyLastPage):
                    OnPropertyChanged(nameof(IsInsertDummyLastPage));
                    break;

                case nameof(BookConfig.PageEndAction):
                    UpdatePageEndAction();
                    break;

                case nameof(BookConfig.IsReadyToPageMove):
                    OnPropertyChanged(nameof(IsReadyToPageMove));
                    break;

                case nameof(BookConfig.WidePageStretch):
                    OnPropertyChanged(nameof(WidePageStretch));
                    break;

                case nameof(BookConfig.WidePageVerticalAlignment):
                    OnPropertyChanged(nameof(WidePageVerticalAlignment));
                    break;
            }
        }

        private void SlideShowConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SlideShowConfig.PageEndAction):
                    UpdatePageEndAction();
                    break;

                case nameof(SlideShowConfig.PageMoveType):
                case nameof(SlideShowConfig.PageMoveDuration):
                    UpdatePageChangeType();
                    break;
            }
        }

        private void ViewConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewConfig.StretchMode):
                    _frameProfile.ResetReferenceSize();
                    OnPropertyChanged(nameof(StretchMode));
                    break;

                case nameof(ViewConfig.AllowFileContentAutoRotate):
                    _frameProfile.ResetReferenceSize();
                    OnPropertyChanged(nameof(AllowFileContentAutoRotate));
                    break;

                case nameof(ViewConfig.AllowStretchScaleUp):
                    _frameProfile.ResetReferenceSize();
                    OnPropertyChanged(nameof(AllowEnlarge));
                    break;

                case nameof(ViewConfig.AllowStretchScaleDown):
                    _frameProfile.ResetReferenceSize();
                    OnPropertyChanged(nameof(AllowReduce));
                    break;

                case nameof(ViewConfig.IsKeepFlip):
                    OnPropertyChanged(nameof(IsFlipLocked));
                    break;

                case nameof(ViewConfig.IsScaleStretchTracking):
                    OnPropertyChanged(nameof(IsScaleStretchTracking));
                    break;

                case nameof(ViewConfig.IsKeepScale):
                    OnPropertyChanged(nameof(IsScaleLocked));
                    OnPropertyChanged(nameof(IsScaleStretchTracking));
                    break;

                case nameof(ViewConfig.IsKeepAngle):
                    OnPropertyChanged(nameof(IsAngleLocked));
                    break;

                case nameof(ViewConfig.AutoRotatePolicy):
                    OnPropertyChanged(nameof(AutoRotatePolicy));
                    break;

                case nameof(ViewConfig.PageMoveType):
                case nameof(ViewConfig.PageMoveDuration):
                    UpdatePageChangeType();
                    break;
            }

            OnPropertyChanged(nameof(ViewConfig));
        }

        private void SystemConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemConfig.IsIgnoreImageDpi):
                    OnPropertyChanged(nameof(IsIgnoreImageDpi));
                    break;
            }
        }

        private void BookSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookSettingConfig.PageMode):
                    OnPropertyChanged(nameof(PageMode));
                    OnPropertyChanged(nameof(StretchMode));
                    OnPropertyChanged(nameof(ContentsSpace));
                    OnPropertyChanged(nameof(FramePageSize));
                    //RaisePropertyChanged(nameof(IsSupportedDividePage));
                    //RaisePropertyChanged(nameof(IsSupportedWidePage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleFirstPage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleLastPage));
                    break;

                case nameof(BookSettingConfig.BookReadOrder):
                    OnPropertyChanged(nameof(ReadOrder));
                    break;

                case nameof(BookSettingConfig.IsSupportedDividePage):
                    OnPropertyChanged(nameof(IsSupportedDividePage));
                    break;

                case nameof(BookSettingConfig.IsSupportedWidePage):
                    OnPropertyChanged(nameof(IsSupportedWidePage));
                    break;

                case nameof(BookSettingConfig.IsSupportedSingleFirstPage):
                    OnPropertyChanged(nameof(IsSupportedSingleFirstPage));
                    break;

                case nameof(BookSettingConfig.IsSupportedSingleLastPage):
                    OnPropertyChanged(nameof(IsSupportedSingleLastPage));
                    break;

                //case nameof(BookSettingConfig.SortMode):
                //    RaisePropertyChanged(nameof(SortMode));
                //    break;

                case nameof(BookSettingConfig.IsRecursiveFolder):
                    OnPropertyChanged(nameof(IsRecursiveFolder));
                    break;

                case nameof(BookSettingConfig.AutoRotate):
                    _frameProfile.ResetReferenceSize();
                    OnPropertyChanged(nameof(AutoRotate));
                    break;

                case nameof(BookSettingConfig.BaseScale):
                    OnPropertyChanged(nameof(BaseScale));
                    break;
            }
        }

        private void FrameProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // NOTE: PageMode変更なので余計なイベントを発生させない
                // TODO: 原則すべてのイベントを発生させるべき。受け取り側で除外する。
                //case nameof(PageFrameProfile.IsStaticFrame):
                //    RaisePropertyChanged(nameof(IsStaticFrame));
                //    break;

                case nameof(PageFrameProfile.CanvasSize):
                    OnPropertyChanged(nameof(CanvasSize));
                    break;

                case nameof(PageFrameProfile.ReferenceSize):
                    OnPropertyChanged(nameof(ReferenceSize));
                    break;

                case nameof(PageFrameProfile.DpiScale):
                    OnPropertyChanged(nameof(DpiScale));
                    break;
            }
        }

        private void SlideShowIsPlayngChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdatePageEndAction();
            UpdatePageChangeType();
        }


        public void SetCanvasSize(object sender, SizeChangedEventArgs e)
        {
            SizeChanging?.Invoke(this, e);
            _frameProfile.CanvasSize = e.NewSize;
            SizeChanged?.Invoke(this, e);
        }

        public void SetDpiScale(DpiScale dpiScale)
        {
            _frameProfile.DpiScale = dpiScale;
        }

        public void ResetReferenceSize()
        {
            _frameProfile.ResetReferenceSize();
        }

        public void ResetAutoStretchTarget()
        {
            SetAutoStretchTarget(PageRange.Empty);
        }

        public IDisposable IgnoreScaleStretchTracking()
        {
            _ignoreScaleStretchTracking = true;
            return new AnonymousDisposable(() => { _ignoreScaleStretchTracking = false; });
        }

        public void SetAutoStretchTarget(PageRange range)
        {
            LocalDebug.WriteLine($"AutoStretchTarget: {range}");
            _autoStretchTarget = range;
        }

        public bool IsDividePage(Page page)
        {
            return PageMode == PageMode.SinglePage
                && IsSupportedDividePage
                && AspectRatioTools.IsLandscape(new PageSizeCalculator(this, page.Content.PageDataSource).GetPageSize());
        }
    }
}
