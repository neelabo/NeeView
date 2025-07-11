//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.ComponentModel;
using NeeView;
using NeeView.Windows;
using NeeView.Maths;
using System.Globalization;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    /// <summary>
    /// ページフレーム表示
    /// </summary>
    [LocalDebug]
    public partial class PageFrameContent : IPageFrameContent, IDisposable, INotifyTransformChanged
    {
        private readonly ViewContentFactory _viewContentFactory;

        private readonly Canvas _canvas;
        private readonly Canvas _contentCanvas;
        private readonly GridLine _gridLine;
        private readonly SizeSource _sizeSource;
        private PageFrame _pageFrame;
        private readonly Page? _nextPage;
        private readonly PageFrameActivity _activity;

        /// <summary>
        /// レイアウトトランスフォーム 
        /// </summary>
        private readonly PageFrameTransformAccessor _transform;

        /// <summary>
        /// ルーペ用トランスフォーム
        /// </summary>
        private readonly LoupeTransformContext _loupeContext;

        private readonly PageFrameContext _context;
        private readonly List<Page> _pages;
        private List<ViewContent> _viewContents;

        /// <summary>
        /// レイアウト用。アニメーションあり
        /// </summary>
        private readonly TransformGroup _viewTransform = new();

        /// <summary>
        /// 計算用。アニメーションなし
        /// </summary>
        private readonly TransformGroup _calcTransform = new();

        /// <summary>
        /// Bounds計算用。Source
        /// </summary>
        private readonly TransformGroup _boundsTransform;


        /// <summary>
        /// 基底スケールのトランスフォーム
        /// </summary>
        private readonly BaseScaleTransform _baseScaleTransform;

        private readonly ContentSizeCalculator _calculator;
        private PageFrameDirtyLevel _dirtyLevel;
        
        private bool _disposedValue = false;
        private readonly DisposableCollection _disposables = new();


        /// <summary>
        /// PageFrameから構成されるPageFrameContainer用コンテンツ
        /// </summary>
        /// <param name="viewContentFactory">ViewContentファクトリ</param>
        /// <param name="context">固定フレーム情報。コンテナサイズを確定させる</param>
        /// <param name="pageFrame">PageFrame</param>
        /// <param name="nextPage">PageFrameの次のページ。フレーム再生成チェック用</param>
        /// <param name="activity">コンテナの状態。表示中、選択中等</param>
        /// <param name="transform">レイアウト用Transform</param>
        /// <param name="loupeContext">ルーペ用Transform</param>
        public PageFrameContent(ViewContentFactory viewContentFactory, PageFrameContext context, PageFrame pageFrame, Page? nextPage, PageFrameActivity activity, PageFrameTransformAccessor transform, LoupeTransformContext loupeContext, BaseScaleTransform baseScaleTransform, ContentSizeCalculator calculator)
        {
            _canvas = new Canvas();
            _canvas.RenderTransform = _viewTransform;

            _contentCanvas = new Canvas();
            _canvas.Children.Add(_contentCanvas);

            _viewContentFactory = viewContentFactory;
            _context = context;

            _pageFrame = pageFrame;
            _pages = _pageFrame.Elements.Select(e => e.Page).Distinct().ToList();
            foreach (var page in _pages)
            {
                _disposables.Add(page.SubscribeSizeChanged(AppDispatcher.BeginInvokeHandler(Page_SizeChanged)));
            }

            _nextPage = nextPage;
            if (_nextPage is not null)
            {
                _disposables.Add(_nextPage.SubscribeSizeChanged(AppDispatcher.BeginInvokeHandler(NextPage_SizeChanged)));
            }

            _activity = activity;

            _transform = transform;
            _disposables.Add(_transform);
            _disposables.Add(_transform.SubscribeTransformChanged(Transform_TransformChanged));

            _baseScaleTransform = baseScaleTransform;
            _disposables.Add(_baseScaleTransform.SubscribeScaleChanged(BaseScaleTransform_ScaleChanged));

            _calculator = calculator;

            _loupeContext = loupeContext;

            _boundsTransform = new TransformGroup();
            _boundsTransform.Children.Add(_baseScaleTransform.ScaleTransform);
            _boundsTransform.Children.Add(_transform.Transform);

            // grid line
            _sizeSource = new SizeSource(_pageFrame.StretchedSize);
            _gridLine = new GridLine(ImageGridTarget.Image);
            _disposables.Add(_gridLine);
            _sizeSource.BindTo(_gridLine);
            _canvas.Children.Add(_gridLine);

            CreateContents();

            // ページフレーム情報が既に古い？
            if (IsPageFrameDirty() || IsWidePageFrameDirty())
            {
                LocalDebug.WriteLine("PageFrameContent: PageFrame is dirty.");
                DirtyLevel = PageFrameDirtyLevel.Moderate;
            }
        }



        public event TransformChangedEventHandler? TransformChanged;

        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;

        // NOTE: コンテンツサイズの変更イベントだが、用途はフレームの作り直し要求なので DirtyLevel 変更イベントのほうが適切かも
        public event EventHandler? ContentSizeChanged;


        public FrameworkElement? Content => _canvas;

        public PageFrameActivity Activity => _activity;

        public bool IsLocked => false;

        public PageFrame PageFrame => _pageFrame;

        public PageRange FrameRange => _pageFrame.FrameRange;

        public IPageFrameTransform Transform => _transform;

        public Size FrameSize => _pageFrame.Size;
        public Transform FrameTransform => _transform.Transform;

        public bool IsFirstFrame => (_pageFrame.Terminal & PageTerminal.First) == PageTerminal.First;
        public bool IsLastFrame => (_pageFrame.Terminal & PageTerminal.Last) == PageTerminal.Last;

        public bool IsDirty => _dirtyLevel > PageFrameDirtyLevel.Clean;

        public PageFrameDirtyLevel DirtyLevel
        {
            get => _dirtyLevel;
            set => _dirtyLevel = _dirtyLevel < value ? value : _dirtyLevel;
        }

        public List<ViewContent> ViewContents => _viewContents;

        public int ViewContentsDirection => _pageFrame.Direction;

        public FrameworkElement ViewElement => _contentCanvas;
        public TransformGroup ViewTransform => _viewTransform;
        public TransformGroup CalcTransform => _calcTransform;
        public BaseScaleTransform BaseScaleTransform => _baseScaleTransform;

        public bool IsStaticFrame => _context.IsStaticFrame;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachTransform();
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

        private void Transform_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            TransformChanged?.Invoke(this, e);
        }

        private void BaseScaleTransform_ScaleChanged(object? sender, EventArgs e)
        {
            TransformChanged?.Invoke(this, new TransformChangedEventArgs(_baseScaleTransform, TransformCategory.BaseScale, TransformAction.Scale));
        }


        private void Page_SizeChanged(object? sender, EventArgs e)
        {
            LocalDebug.WriteLine($"Page.SizeChanged: Call, {((Page?)sender)?.Size}");
            RaiseContentSizeChanged();
        }

        private void NextPage_SizeChanged(object? sender, EventArgs e)
        {
            // ページ補填の余地あり？
            if (IsWidePageFrameDirty())
            {
                LocalDebug.WriteLine($"NextPage.SizeChanged: Call, {((Page?)sender)?.Size}");
                RaiseContentSizeChanged();
            }
            else
            {
                LocalDebug.WriteLine($"NextPage.SizeChanged: Skip");
            }
        }

        public void OnAttached()
        {
            if (_dirtyLevel > PageFrameDirtyLevel.Clean)
            {
                AppDispatcher.BeginInvoke(() => RaiseContentSizeChanged());
            }
        }

        public void OnDetached()
        {
        }

        private bool IsPageFrameDirty()
        {
            // ページサイズが変更されている可能性
            if (_pageFrame.Elements.Any(e => e.PageSize != e.Page.Size))
            {
                return true;
            }

            return false;
        }

        private bool IsWidePageFrameDirty()
        {
            if (_nextPage is null) return false;

            // 2ページモードで1ページだけ表示しているときに、次のページによって2ページ表示になる可能性
            if (_context.PageMode == PageMode.WidePage && _context.IsSupportedWidePage
                && _pageFrame.IsSinglePortraitElement()
                && AspectRatioTools.IsPortrait(_nextPage.Size))
            {
                return true;
            }

            return false;
        }

        private void RaiseContentSizeChanged()
        {
            DirtyLevel = PageFrameDirtyLevel.Moderate;
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSource(PageFrame pageFrame)
        {
            var isForce = DirtyLevel >= PageFrameDirtyLevel.Heavy;
            _dirtyLevel = PageFrameDirtyLevel.Clean;

            if (!_pageFrame.IsMatch(pageFrame)) throw new ArgumentException("Resources do not match");
            if (!isForce && _pageFrame.Equals(pageFrame)) return;

            _pageFrame = pageFrame;

            foreach ((var viewContent, var element) in _viewContents.Zip(_pageFrame.Elements))
            {
                viewContent.SetSource(element, CreateElementScale(), isForce);
            }
            UpdateTransform();
            UpdateElementLayout();
            Stretch(false);

            _sizeSource.SetSize(_pageFrame.StretchedSize);
        }

        private PageFrameElementScale CreateElementScale()
        {
            return PageFrameElementScaleFactory.Create(_pageFrame, _transform, _loupeContext, _baseScaleTransform, _context.DpiScale);
        }


        /// <summary>
        /// ストレッチスケールを適用した素材の矩形。自動回転は適用外
        /// </summary>
        /// <returns></returns>
        public Rect GetRawContentRect()
        {
            return _pageFrame.StretchedSize.ToRect();
        }

        /// <summary>
        /// ストレッチスケールと自動回転とレイアウト変換を適用した矩形
        /// </summary>
        /// <remarks>
        /// ルーペは適用されていません。
        /// </remarks>
        /// <returns></returns>
        public Rect GetContentRect()
        {
            var rect = _pageFrame.Size.ToRect();
            var bounds = _boundsTransform.TransformBounds(rect);
            return bounds;
        }

        /// <summary>
        /// コンテナフレームサイズ
        /// </summary>
        /// <remarks>
        /// 固定フレームモードでは表示エリアサイズを返す。
        /// コンテナ表示数を制限するためにサイズの下限あり。
        /// </remarks>
        /// <returns></returns>
        public Size GetFrameSize()
        {
            if (_context.IsStaticFrame)
            {
                return _context.CanvasSize;
            }
            else
            {
                var rect = _pageFrame.Size.ToRect();
                var bounds = _boundsTransform.TransformBounds(rect);

                // TODO: １つのコンテナの画面幅に対する最小の割合。同時に多すぎる表示を回避するため。Configに設定を。
                var limitScale = 0.10;
                var width = Math.Max(bounds.Width, _context.CanvasSize.Width * limitScale);
                var height = Math.Max(bounds.Height, _context.CanvasSize.Height * limitScale);
                LocalDebug.WriteLine($"# FrameSize: {_pageFrame.FrameRange} => {width:f1}x{height:f1}");
                return new Size(width, height);
            }
        }


        /// <summary>
        /// フレーム内容の更新
        /// </summary>
        [MemberNotNull(nameof(_viewContents))]
        private void CreateContents()
        {
            _contentCanvas.Children.Clear();

            // layout contents
            _viewContents = _pageFrame.Elements
                .Select((e, index) => _viewContentFactory.Create(e, CreateElementScale(), _activity, index))
                .ToList();
            _disposables.AddRange(_viewContents);
            foreach (var content in _viewContents.Direction(_pageFrame.Direction))
            {
                content.Initialize();
                _contentCanvas.Children.Add(content);
                _disposables.Add(content.SubscribeViewContentChanged(ViewContent_Changed));
            }

            UpdateTransform();
            UpdateElementLayout();
            Stretch(false);
        }

        private void ViewContent_Changed(object? sender, ViewContentChangedEventArgs e)
        {
            var action = ViewContentChangedActionExtensions.Min(GetViewContentState().ToChangedAction(), e.Action);
            ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(action, this, ViewContents, ViewContentsDirection) { InnerArgs = e });
        }

        private void UpdateTransform()
        {
            DetachTransform();

            _viewTransform.Children.Clear();
            _viewTransform.Children.Add(_baseScaleTransform.ScaleTransform);
            _viewTransform.Children.Add(_pageFrame.RotateTransform);
            _viewTransform.Children.Add(_transform.TransformView);
            _viewTransform.Children.Add(_loupeContext.GetContentTransform());

            _calcTransform.Children.Clear();
            _calcTransform.Children.Add(_baseScaleTransform.ScaleTransform);
            _calcTransform.Children.Add(_pageFrame.RotateTransform);
            _calcTransform.Children.Add(_transform.Transform);
            _calcTransform.Children.Add(_loupeContext.GetContentTransform());

            AttachTransform();
        }

        // TODO: _transform, _loupeContext は不変なのでAttach,Detachは不要？
        private void AttachTransform()
        {
            _transform.TransformChanged += ViewTransform_TransformChanged;
            _loupeContext.TransformChanged += ViewTransform_TransformChanged;
            _baseScaleTransform.ScaleChanged += ViewBaseScaleTransform_ScaleChanged;
        }


        private void DetachTransform()
        {
            _transform.TransformChanged -= ViewTransform_TransformChanged;
            _loupeContext.TransformChanged -= ViewTransform_TransformChanged;
            _baseScaleTransform.ScaleChanged -= ViewBaseScaleTransform_ScaleChanged;
        }

        private void ViewTransform_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            if (e.Action == TransformAction.Scale || e.Action == TransformAction.Angle)
            {
                var scale = CreateElementScale(); // GetRenderScale();
                LocalDebug.WriteLine($"{FrameRange}: Scale={scale:f2}");

                foreach (var viewContent in _viewContents)
                {
                    viewContent.SetSource(viewContent.Element, scale, false);
                    //viewContent.SetRenderScale(scale);
                }
            }
        }

        private void ViewBaseScaleTransform_ScaleChanged(object? sender, EventArgs e)
        {
            var scale = CreateElementScale();
            foreach (var viewContent in _viewContents)
            {
                viewContent.SetSource(viewContent.Element, scale, false);
            }
        }

        private void UpdateElementLayout()
        {
            var x = _pageFrame.StretchedSize.Width * -0.5;
            foreach (var content in _viewContents.Direction(_pageFrame.Direction))
            {
                var y = _context.WidePageVerticalAlignment switch
                {
                    WidePageVerticalAlignment.Top => _pageFrame.StretchedSize.Height * -0.5,
                    WidePageVerticalAlignment.Bottom => _pageFrame.StretchedSize.Height * 0.5 - content.Height,
                    _ => content.Height * -0.5,
                };
                Canvas.SetLeft(content, x);
                Canvas.SetTop(content, y);
                x += content.Width + _pageFrame.Span;
            }
        }

        public ViewContentState GetViewContentState()
        {
            return _viewContents.Select(e => e.State).Min();
        }

        /// <summary>
        /// ViewContent を Dispose
        /// </summary>
        /// <remarks>
        /// ファイル削除するときに対応するページのリソースを開放するために使用。
        /// その後の継続した使用は考慮されていない。
        /// </remarks>
        public void DisposeViewContent()
        {
            foreach (var content in _viewContents)
            {
                content.Dispose();
            }
        }

        /// <summary>
        /// ストレッチ追従であればストレッチする
        /// </summary>
        /// <param name="force">強制実行</param>
        public void Stretch(bool force)
        {
            Stretch(force, 1.0, TransformTrigger.None);
        }

        /// <summary>
        /// ストレッチ追従であればストレッチする
        /// </summary>
        /// <param name="force">強制実行</param>
        /// <param name="rate">ストレッチスケールのスケール倍率</param>
        public void Stretch(bool force, double rate)
        {
            Stretch(force, rate, TransformTrigger.None);
        }

        /// <summary>
        /// ストレッチ追従であればストレッチする
        /// </summary>
        /// <param name="force">強制実行</param>
        /// <param name="rate">ストレッチスケールのスケール倍率</param>
        /// <param name="trigger">トリガーアクション</param>
        public void Stretch(bool force, double rate, TransformTrigger trigger)
        {
            if (!force && !_context.ShouldScaleStretchTracking) return;

            trigger = trigger != TransformTrigger.None ? trigger : force ? TransformTrigger.Snap : TransformTrigger.SnapTracking;
            var scale = CalcStretchScale(_context.CanvasSize);
            _transform.SetScale(scale * rate, TimeSpan.Zero, trigger);
        }

        /// <summary>
        /// ストレッチスケールのスケール倍率を計算
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcStretchScaleRate(Size canvasSize)
        {
            var oldStretchScale = CalcStretchScale(canvasSize);
            if (oldStretchScale <= 0.0) return 1.0;
            return _transform.Scale / oldStretchScale;
        }

        /// <summary>
        /// ストレッチスケールを計算
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcStretchScale(Size canvasSize)
        {
            return _calculator.CalcModeStretchScale(_pageFrame.Size, new RotateTransform(_transform.Angle), canvasSize);
        }


        public override string ToString()
        {
            return _pageFrame.ToString();
        }

    }

}


