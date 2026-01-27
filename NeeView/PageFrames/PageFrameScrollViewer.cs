using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameScrollViewer : Grid, IPointControl, INotifyPropertyChanged, IScrollable
    {
        private readonly PageFrameContext _context;
        private readonly Canvas _rootCanvas;
        private readonly PageFrameContainerCanvas _canvas;
        private readonly PageFrameViewTransform _transform;
        private bool _isAreaLimitEnabled;


        public PageFrameScrollViewer(PageFrameContext context, PageFrameContainerCanvas canvas, PageFrameViewTransform transform)
        {
            this.MinWidth = PageFrameProfile.MinWidth;
            this.MinHeight = PageFrameProfile.MinHeight;
            this.ClipToBounds = true;

            _context = context;
            _canvas = canvas;

            _transform = transform;
            _canvas.RenderTransform = transform.TransformView;

            _rootCanvas = new Canvas()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Children.Add(_rootCanvas);

            _rootCanvas.Children.Add(_canvas);

#if DEBUG
            // [DEV]
            {
                var textBox = new TextBox();
                textBox.HorizontalAlignment = HorizontalAlignment.Left;
                textBox.VerticalAlignment = VerticalAlignment.Bottom;
                var binding = new MultiBinding();
                binding.Bindings.Add(new Binding("Point.X") { Source = this });
                binding.Bindings.Add(new Binding("Point.Y") { Source = this });
                binding.Converter = new CanvasPositionConverter();
                textBox.SetBinding(TextBox.TextProperty, binding);
                textBox.Visibility = PageFrameDebug.Visibility;
                Children.Add(textBox);
            }
#endif
        }


#if DEBUG
        private class CanvasPositionConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var x = values[0] as double?; // NOTE: Undefined になる時がある対策
                var y = values[1] as double?;
                return $"Canvas: {x:f0},{y:f0}";
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
#endif

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public PageFrameViewTransform Transform => _transform;


        public Point Point
        {
            get { return _transform.Point; }
        }

        public Point ViewPoint
        {
            get { return _transform.ViewPoint; }
        }


        public IDisposable SubscribeSizeChanged(SizeChangedEventHandler handler)
        {
            SizeChanged += handler;
            return new AnonymousDisposable(() => SizeChanged -= handler);
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            SetPoint(default, TimeSpan.Zero);
        }

        public Point GetRealPoint()
        {
            return _transform.TransformView.Transform(new Point(0, 0));
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
            _isAreaLimitEnabled = inertia;

            _context.ViewScrollContext.AddScrollTime(this, inertia ? span : TimeSpan.Zero);

            if (Point == value) return;

            //Debug.WriteLine($"## {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds}): inertia={inertia}");
            _transform.SetPoint(value, span, easeX, easeY);
            RaisePropertyChanged(nameof(Point));
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

        public Vector GetVelocity()
        {
            return _transform.GetVelocity();
        }

        public void ResetVelocity()
        {
            _transform.ResetVelocity();
        }

        public void FlushScroll()
        {
            _transform.Flush();
        }

        public void ApplyAreaLimit(Point c0, Point c1)
        {
            if (!_isAreaLimitEnabled) return;
            if (!_context.ViewConfig.IsLimitMove) return;

            var vp = ViewPoint;
            var p0 = new Point(vp.X + c0.X, vp.Y + c0.Y);
            var p1 = new Point(vp.X + c1.X, vp.Y + c1.Y);

            if (_context.FrameOrientation == PageFrameOrientation.Horizontal)
            {
                if (p0.X > 0.0)
                {
                    var point = new Point(-c0.X, vp.Y);
                    SetPoint(point, TimeSpan.FromMilliseconds(200));
                }
                else if (p1.X < 0.0)
                {
                    var point = new Point(-c1.X, vp.Y);
                    SetPoint(point, TimeSpan.FromMilliseconds(200));
                }
            }
            else
            {
                if (p0.Y > 0.0)
                {
                    var point = new Point(vp.X, -c0.Y);
                    SetPoint(point, TimeSpan.FromMilliseconds(200));
                }
                else if (p1.Y < 0.0)
                {
                    var point = new Point(vp.X, -c1.Y);
                    SetPoint(point, TimeSpan.FromMilliseconds(200));
                }
            }
        }

        public void CancelScroll()
        {
            SetPoint(ViewPoint, TimeSpan.Zero);
        }
    }




    public interface IScrollable
    {
        void CancelScroll();
    }
}
