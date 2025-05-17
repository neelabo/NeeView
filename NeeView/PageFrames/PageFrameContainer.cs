using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;


namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameActivity : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isVisible = true;

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
    }

#if DEBUG
    public static class PageFrameDebug
    {
        public static Visibility Visibility { get; set; } = Visibility.Collapsed;
    }
#endif



    public partial class PageFrameContainer : Grid, IDisposable, IComparable<PageFrameContainer>, INotifyTransformChanged, IScaleControl, IAngleControl, IPointControl, IFlipControl, IScrollable
    {
        private double _ms = 0.0;

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _disposedValue;

        private readonly PageFrameActivity _activity;
        private readonly ViewScrollContext _viewScrollContext;
        private readonly ContentControl _contentControl;
        private IPageFrameContent _content;

#if DEBUG
        private readonly TextBlock _textBlock;
#endif


        public PageFrameContainer(IPageFrameContent content, PageFrameActivity activity, ViewScrollContext viewScrollContext)
        {
            _activity = activity;
            _viewScrollContext = viewScrollContext;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            ClipToBounds = true;
            Visibility = Visibility.Collapsed;

            Canvas.SetTop(this, _x);
            Canvas.SetLeft(this, _y);
            Width = _width;
            Height = _height;

            _contentControl = new ContentControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Focusable = false,
            };
            Children.Add(_contentControl);

#if DEBUG
            // [DEV]
            _textBlock = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 16.0,
                Foreground = Brushes.Orange,
                Visibility = PageFrameDebug.Visibility,
            };
            Children.Add(_textBlock);
#endif

            AttachContent(content);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachContent();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        [Subscribable] 
        public event TransformChangedEventHandler? TransformChanged;
        
        [Subscribable]
        public event EventHandler? ContentSizeChanged;

        [Subscribable]
        public event EventHandler? ContainerLayoutChanged;
        
        [Subscribable]
        public event EventHandler? ContentChanged;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;


        public PageFrameActivity Activity => _activity;

        public IPageFrameTransform Transform => _content.Transform;

        public bool IsLocked => _content.IsLocked;
        public bool IsDirty => _content.DirtyLevel > PageFrameDirtyLevel.Clean;
        public bool IsLayouted { get; set; }

        public PageFrameDirtyLevel DirtyLevel
        {
            get => _content.DirtyLevel;
            set => _content.DirtyLevel = value;
        }

        /// <summary>
        /// ページ遅延移動用。
        /// </summary>
        /// <remarks>
        /// 遅延ページ移動に使用される。通常のコンテナとして使用されるようになれば true を設定する。
        /// </remarks>
        public bool IsStable { get; private set; }

        // TODO: これどうなん？
        public PagePosition Identifier => FrameRange.Min;
        public PageRange FrameRange => _content.FrameRange;

        public bool IsFirstFrame => _content.IsFirstFrame;
        public bool IsLastFrame => _content.IsLastFrame;

        public bool IsHorizontalAnimationEnabled { get; set; }
        public bool IsVerticalAnimationEnabled { get; set; }


        public IPageFrameContent Content
        {
            get => _content;
            set => AttachContent(value);
        }

        public double Duration
        {
            get => _ms;
            set => _ms = value;
        }

        public double X
        {
            get => _x;
            set => SetX(value, IsHorizontalAnimationEnabled ? _ms : 0.0);
        }

        public double Y
        {
            get => _y;
            set => SetY(value, IsVerticalAnimationEnabled ? _ms : 0.0);
        }

        public new double Width
        {
            get => _width;
            set => SetWidth(value, IsHorizontalAnimationEnabled ? _ms : 0.0);
        }

        public new double Height
        {
            get => _height;
            set => SetHeight(value, IsVerticalAnimationEnabled ? _ms : 0.0);
        }

        public Rect Rect
        {
            get => new Rect(_x, _y, _width, _height);
        }

        public Point Center
        {
            get => new Point(_x + _width * 0.5, _y + _height * 0.5);
            set
            {
                X = value.X - _width * 0.5;
                Y = value.Y - _height * 0.5;
            }
        }

        public Point Point => Transform.Point;

        public Point ViewPoint => new Point(Transform.TransformView.Value.OffsetX, Transform.TransformView.Value.OffsetY);

        public double Angle => Transform.Angle;

        public double Scale => Transform.Scale;

        public bool IsFlipHorizontal => Transform.IsFlipHorizontal;

        public bool IsFlipVertical => Transform.IsFlipVertical;


        public int CompareTo(PageFrameContainer? other)
        {
            if (other == null) return 1;

            return Identifier.CompareTo(other.Identifier);
        }

        public static int CompareTo(PageFrameContainer? x, PageFrameContainer? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;

            return x.CompareTo(y);
        }

        public override string? ToString()
        {
            return _content.ToString();
        }

        public void SetX(double x, double ms)
        {
            //if (this.FrameRange.Max == new PagePosition(0, 1))
            //{
            //    Debug.WriteLine($"#{this.FrameRange}: X={_x:f0} to {x:f0} ({Math.Abs(x - _x):f0})");
            //    if (Math.Abs(x - _x) > 100)
            //    {
            //        Debug.WriteLine("!!");
            //    }
            //}

            if (_x != x)
            {
                _x = x;
                BeginAnimation(Canvas.LeftProperty, _x, ms);
                ContainerLayoutChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetY(double y, double ms)
        {
            if (_y != y)
            {
                _y = y;
                BeginAnimation(Canvas.TopProperty, _y, ms);
                //Debug.WriteLine($"# {FrameRange}: Y: {_y} / {_height}");
                ContainerLayoutChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetWidth(double width, double ms)
        {
            if (_width != width)
            {
                _width = width;
                BeginAnimation(WidthProperty, width, ms);
                ContainerLayoutChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetHeight(double height, double ms)
        {
            if (_height != height)
            {
                _height = height;
                BeginAnimation(HeightProperty, height, ms);
                ContainerLayoutChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void FlushLayout()
        {
            BeginAnimation(Canvas.LeftProperty, _x, 0.0);
            BeginAnimation(Canvas.TopProperty, _y, 0.0);
            BeginAnimation(WidthProperty, _width, 0.0);
            BeginAnimation(HeightProperty, _height, 0.0);
        }

        private void BeginAnimation(DependencyProperty dp, double value, double ms)
        {
            if (ms <= 0.0)
            {
                BeginAnimation(dp, null);
                SetValue(dp, value);
            }
            else
            {
                var doubleAnimation = new DoubleAnimation();
                doubleAnimation.To = value;
                doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                doubleAnimation.EasingFunction = EaseTools.DefaultEase;
                BeginAnimation(dp, doubleAnimation);
            }
        }


        private void Transform_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            TransformChanged?.Invoke(this, e);
        }




        [MemberNotNull(nameof(_content))]
        private void AttachContent(IPageFrameContent content)
        {
            if (_content == content) return;

            Debug.Assert(content.Activity == this.Activity);

            DetachContent();

            _content = content;
            _content.TransformChanged += Content_TransformChanged;
            _content.ViewContentChanged += Content_ViewContentChanged;
            _content.ContentSizeChanged += Content_ContentSizeChanged;
            _contentControl.Content = _content.Content;

#if DEBUG
            _textBlock.Text = _content.FrameRange.ToString();
#endif

            UpdateFrame();

            ContentChanged?.Invoke(this, EventArgs.Empty);

            if (_content is PageFrameContent pageFrameContent)
            {
                var action = pageFrameContent.ViewContents.Select(e => e.State).Min().ToChangedAction();
                ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(action, pageFrameContent, pageFrameContent.ViewContents, pageFrameContent.ViewContentsDirection));
            }

            _content.OnAttached();
        }


        private void DetachContent()
        {
            if (_content is null) return;
            _content.TransformChanged -= Content_TransformChanged;
            _content.ViewContentChanged -= Content_ViewContentChanged;
            _content.ContentSizeChanged -= Content_ContentSizeChanged;
            _content.OnDetached();
            _content.Dispose();
        }

        private void Content_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            UpdateFrame();
            TransformChanged?.Invoke(this, e);
        }

        private void Content_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            if (_content is PageFrameContent pageFrameContent)
            {
                ViewContentChanged?.Invoke(this, e);
            }
        }

        private void Content_ContentSizeChanged(object? sender, EventArgs e)
        {
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 現在のコンテナの矩形を返す
        /// </summary>
        /// <returns>矩形</returns>
        public Rect GetContentRect()
        {
            return _content.GetContentRect();
        }

        /// <summary>
        /// 指定座標でのコンテナの大きさの矩形を返す
        /// </summary>
        /// <param name="center">コンテナ中心座標</param>
        /// <returns>矩形</returns>
        public Rect GetContentRect(Point center)
        {
            var rect  =_content.GetContentRect();
            return new Rect(center.X - rect.Width * 0.5, center.Y - rect.Height * 0.5, rect.Width, rect.Height);
        }

        public Point TranslateContentToCanvasPoint(Point point)
        {
            var x = X + Width * 0.5 + point.X;
            var y = Y + Height * 0.5 + point.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// フレームサイズの更新
        /// </summary>
        public void UpdateFrame()
        {
            // TODO: アンカーのみアニメ時間を適用しないようにする
            var duration = Duration; // 0.0

            if (_disposedValue) return;
            if (_content is null) return;

            var old = Rect;

            var size = _content.GetFrameSize();
            var horizontalDuration = size.Width > old.Width ? duration : 0.0;
            var verticalDuration = size.Height > old.Height ? duration : 0.0;
            SetWidth(size.Width, horizontalDuration);
            SetHeight(size.Height, verticalDuration);

            this.ClipToBounds = _content.IsStaticFrame;

            //_transformTextBlock.Text = $"{(int)_pageFrame.Angle}, {_pageFrame.Scale:f2}";
            //Debug.WriteLine($"Container.Update: {FrameRange}, {HorizontalAlignment}");

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    break;
                case HorizontalAlignment.Right:
                    SetX(old.Left - (Width - old.Width) * 1.0, horizontalDuration);
                    break;
                default:
                    SetX(old.Left - (Width - old.Width) * 0.5, horizontalDuration);
                    break;
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    break;
                case VerticalAlignment.Bottom:
                    SetY(old.Top - (Height - old.Height) * 1.0, verticalDuration);
                    break;
                default:
                    SetY(old.Top - (Height - old.Height) * 0.5, verticalDuration);
                    break;
            }
        }

        public void ResetLayout()
        {
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            SetX(-Width * 0.5, 0.0);
            SetY(-Height * 0.5, 0.0);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null, false);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            SetPoint(value, span, easeX, easeY, false);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY, bool inertia)
        {
            _viewScrollContext.AddScrollTime(this, inertia ? span : TimeSpan.Zero);
            Transform.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            SetPoint(Point + value, span, null, null, false);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            SetPoint(Point + value, span, easeX, easeY, false);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY, bool inertia)
        {
            SetPoint(Point + value, span, easeX, easeY, inertia);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            Transform.SetAngle(value, span);
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            Transform.SetScale(value, span, trigger);
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            Transform.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            Transform.SetFlipVertical(value, span);
        }

        public void CancelScroll()
        {
            SetPoint(ViewPoint, TimeSpan.Zero);
        }

        public void SetStable(bool stable)
        {
            IsStable = stable;
            this.Visibility = IsStable ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Wait Load ViewSource
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask WaitLoadAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            using var disposables = new DisposableCollection();
            var tcs = new TaskCompletionSource();
            disposables.Add(token.Register(() => tcs.TrySetCanceled()));
            disposables.Add(SubscribeViewContentChanged((s, e) => { if (IsViewSourceLoaded()) { tcs.TrySetResult(); } }));
            if (IsViewSourceLoaded()) return;
            await tcs.Task;
        }

        private bool IsViewSourceLoaded()
        {
            return _content is PageFrameContent content ? content.ViewContents.All(e => e.ViewSource.IsLoaded) : true;
        }
    }

}
