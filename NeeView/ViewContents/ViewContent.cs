//#define LOCAL_DEBUG

using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView;
using System.Windows.Input;
using System.Windows.Threading;
using NeeView.Windows;
using NeeLaboratory.Generators;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;

namespace NeeView
{

    public partial class ViewContent : ContentControl, IDisposable, IHasImageSource, IHasViewContentMediaPlayer, IHasScalingMode
    {
        private bool _initialized;
        private readonly PageFrameContext _context;
        private PageFrameElement _element;
        private PageFrameElementScale _scale;
        private readonly PageFrameActivity _activity;
        private readonly ViewSource _viewSource;
        private readonly ViewContentSize _viewContentSize;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private readonly SizeSource _sizeSource;
        private readonly PageBackgroundSource _backgroundSource;
        private readonly int _index;
        private IViewContentStrategy? _strategy;



        /// <summary>
        /// ページ要素の表示物
        /// </summary>
        /// <param name="element">ページ要素</param>
        /// <param name="scale">ページ要素スケール</param>
        /// <param name="viewSource">表示ソース</param>
        /// <param name="activity">表示状態</param>
        /// <param name="backgroundSource">ページ背景</param>
        /// <param name="index">フレーム内のページ要素番号</param>
        public ViewContent(PageFrameContext context, PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
        {
            this.Focusable = false;
            _context = context;
            _element = element;
            _scale = scale;
            _viewSource = viewSource;
            _activity = activity;
            _index = index;

            _viewContentSize = ViewContentSizeFactory.Create(element, scale);

            _sizeSource = new SizeSource(LayoutSize);
            _sizeSource.BindTo(this);

            _backgroundSource = backgroundSource;
        }


        [Subscribable]
        public event EventHandler<ViewContentChangedEventArgs>? ViewContentChanged;


        public Page Page => _element.Page;
        public PageFrameActivity Activity => _activity;
        public ArchiveEntry ArchiveEntry => _element.Page.ArchiveEntry;
        public PageFrameElement Element => _element;
        public int ElementIndex => _index;
        public ViewContentSize ViewContentSize => _viewContentSize;
        public Size LayoutSize => _viewContentSize.LayoutSize;
        public ViewSource ViewSource => _viewSource;
        public ViewContentState State { get; private set; }
        public PageBackgroundSource BackgroundSource => _backgroundSource;

        public ViewContentMediaPlayer? Player => (_strategy as IHasViewContentMediaPlayer)?.Player;
        public ImageSource? ImageSource => (_strategy as IHasImageSource)?.ImageSource;
        public BitmapScalingMode? ScalingMode
        {
            get { return (_strategy as IHasScalingMode)?.ScalingMode; }
            set { if (_strategy is IHasScalingMode scalable) scalable.ScalingMode = value; }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _strategy?.Dispose();
                    _disposables.Dispose();

                    (Content as IDisposable)?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public virtual void Initialize()
        {
            if (_disposedValue) return;

            if (_initialized) return;
            _initialized = true;

            _disposables.Add(_viewSource.SubscribeDataSourceChanged(ViewSource_DataChanged));
            _disposables.Add(_element.Page.SubscribeContentChanged(AppDispatcher.BeginInvokeHandler(Page_ContentChanged)));

            UpdateContent(_viewSource.DataSource);

            Trace($"InitializeContentAsync: {ArchiveEntry}");
            RequestLoadViewSource(CancellationToken.None);
        }


        /// <summary>
        /// Source情報(PageFrameElement)の更新。
        /// </summary>
        /// <param name="element">互換性のあるPageFrameElement</param>
        /// <param name="scale">表示のスケール情報</param>
        /// <param name="force">強制更新</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetSource(PageFrameElement element, PageFrameElementScale scale, bool force)
        {
            if (_disposedValue) return;

            if (!_element.IsMatch(element)) throw new ArgumentException("Resources do not match");
            if (!force && _element.Equals(element) && _scale == scale) return;

            _element = element;
            _scale = scale;
            _viewContentSize.SetSource(_element, scale);

            // 強制更新であれば読み込み前に一度コンテンツ生成をしておく
            if (force)
            {
                UpdateContent(_viewSource.DataSource);
            }
            else
            {
                if (UpdateSize())
                {
                    ViewContentChanged?.Invoke(this, new ViewContentChangedEventArgs(ViewContentChangedAction.Size, this));
                }
            }

            Trace($"OnSourceChanged: {ArchiveEntry} ({LayoutSize})");
            OnSourceChanged();
        }

        private void OnSourceChanged()
        {
            if (_disposedValue) return;

            _strategy?.OnSourceChanged();
        }


        private void Page_ContentChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            if (_element.Page.Content.IsLoaded)
            {
                //Debug.WriteLine($"OnContentChanged: {ArchiveEntry}");
                OnContentChanged();
            }
        }

        protected virtual void OnContentChanged()
        {
            if (_disposedValue) return;

            RequestLoadViewSource(CancellationToken.None);
        }


        private void ViewSource_DataChanged(object? sender, DataSourceChangedEventArgs e)
        {
            if (_disposedValue) return;

            // NOTE: Unload()等でデータがないときは更新しない
            if (e.DataSource.DataState == DataState.None) return;

            AppDispatcher.BeginInvoke(() => UpdateContent(e.DataSource));
        }


        // コントロールサイズの更新
        protected bool UpdateSize()
        {
            bool sizeChanged = (Width != LayoutSize.Width || Height != LayoutSize.Height);

            _sizeSource.Width = LayoutSize.Width;
            _sizeSource.Height = LayoutSize.Height;

            return sizeChanged;
        }

        /// <summary>
        /// ViewSource の更新
        /// </summary>
        /// <param name="token"></param>
        public void RequestLoadViewSource(CancellationToken token)
        {
            if (_disposedValue) return;

            if (!_element.Page.Content.IsLoaded) return;
            Task.Run(() => LoadViewSourceAsync(token));
        }

        /// <summary>
        /// Load ViewSource
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadViewSourceAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            NVDebug.AssertMTA();
            var pictureSize = _viewContentSize.GetPictureSize();
            Trace($"LoadViewSourceAsync: {_element.Page}, PictureSize = {pictureSize:f0}");
            await _viewSource.LoadAsync(pictureSize, token);
        }

        /// <summary>
        /// Wait Load ViewSource
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitLoadAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            using var disposables = new DisposableCollection();
            var tcs = new TaskCompletionSource();
            disposables.Add(token.Register(() => tcs.TrySetCanceled()));
            disposables.Add(_viewSource.SubscribeDataSourceChanged((s, e) => { if (_viewSource.IsLoaded) { tcs.TrySetResult(); } }));
            if (_viewSource.IsLoaded) return;
            await tcs.Task;
        }

        /// <summary>
        /// Update ViewContent
        /// </summary>
        private void UpdateContent(PageDataSource data)
        {
            if (_disposedValue) return;

            NVDebug.AssertSTA();
            var unit = CreateContent(_sizeSource, data);
            Content = unit.Content;
            State = unit.State;
            UpdateSize();
            ViewContentChanged?.Invoke(this, new ViewContentChangedEventArgs(State.ToChangedAction(), this));
        }

        private ViewContentData CreateContent(SizeSource size, PageDataSource data)
        {
            Debug.Assert(_initialized);

            try
            {
                if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

                switch (data.DataState)
                {
                    case DataState.None:
                        Trace($"CreateContent.Ready: {ArchiveEntry}");
                        bool isBlackBackground = this.Page.Content is MediaPageContent;
                        return new ViewContentData(ViewContentTools.CreateLoadingContent(Element, isBlackBackground, !_context.IsReadyToPageMove), ViewContentState.Loading);

                    case DataState.Loaded:
                        Trace($"CreateContent.Loaded: {ArchiveEntry}");
                        Debug.Assert(data.Data is not null);
                        PageContent.UndefinedSize = data.Size;
                        return new ViewContentData(CreateLoadedContent(data.Data), ViewContentState.Loaded);

                    case DataState.Failed:
                        Trace($"CreateContent.Failed: {ArchiveEntry}");
                        return new ViewContentData(ViewContentTools.CreateErrorContent(Element, data.ErrorMessage), ViewContentState.Failed);

                    default:
                        throw new InvalidOperationException("Invalid DataState");
                }
            }
            catch (Exception ex)
            {
                return new ViewContentData(ViewContentTools.CreateErrorContent(Element, ex.Message), ViewContentState.Failed);
            }
        }


        private FrameworkElement CreateLoadedContent(object data)
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

            _strategy = _strategy ?? CreateStrategy(data);
            return _strategy.CreateLoadedContent(data);
        }

        private IViewContentStrategy CreateStrategy(object data)
        {
            if (_element.IsDummy)
            {
                return new DummyViewContentStrategy();
            }

            return data switch
            {
                ImageViewData _ => new ImageViewContentStrategy(this),
                AnimatedViewData _ => new AnimatedViewContentStrategy(this),
                MediaViewData _ => new MediaViewContentStrategy(this),
                ArchiveViewData => new ArchiveViewContentStrategy(this),
                FileViewData => new FileViewContentStrategy(this),
                EmptyViewData => new EmptyViewContentStrategy(this),
                _ => throw new NotSupportedException(),
            };
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(CultureInfo.InvariantCulture, s, args)}");
        }
    }


    public record class ViewContentData(FrameworkElement Content, ViewContentState State);

}
