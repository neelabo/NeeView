using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView.PageFrames
{
    // Canvas座標系での ViewRect を管理
    // TODO: Canvas座標系なのにViewRectって名前でいいの？
    public partial class PageFrameContainerViewBox : IDisposable
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameScrollViewer _view;
        private Rect _rect;
        private Size _size;
        private bool _disposedValue;

        public PageFrameContainerViewBox(PageFrameContext context, PageFrameScrollViewer view)
        {
            _context = context;
            _view = view;

            var width = Math.Max(_view.ActualWidth, _view.MinWidth);
            var height = Math.Max(_view.ActualHeight, _view.MinHeight);
            _size = new Size(width, height);
            _rect = CreateViewRect(_view.Point, new Size(width, height));

            // TODO: Dispose
            _context.SizeChanged += View_SizeChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _context.SizeChanged -= View_SizeChanged;
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
        public event EventHandler<RectChangeEventArgs>? RectChanging;

        [Subscribable]
        public event EventHandler<RectChangeEventArgs>? RectChanged;


        /// <summary>
        /// 計算上のビュー矩形
        /// </summary>
        public Rect Rect
        {
            get { return _rect; }
            private set
            {
                if (_disposedValue) return;
                if (_rect != value)
                {
                    var args = new RectChangeEventArgs(value, _rect);
                    RectChanging?.Invoke(this, args);
                    _rect = value;
                    RectChanged?.Invoke(this, args);
                }
            }
        }

        /// <summary>
        /// アニメーションを考慮したビュー矩形
        /// </summary>
        public Rect ViewingRect
        {
            get => Rect.Union(_rect, GetViewRect(TransformSelect.View));
        }



        private void View_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            Debug.Assert(!_disposedValue);

            _size = e.NewSize;
            UpdateViewRect();
        }

        public void UpdateViewRect()
        {
            if (_disposedValue) return;

            Rect = GetViewRect(TransformSelect.Calc);
        }

        private Rect GetViewRect(TransformSelect select)
        {
            return _view.Transform.GetTransform(select).Inverse?.TransformBounds(_size.ToRect()) ?? new Rect();
        }

        private Rect CreateViewRect(Point center, Size size)
        {
            return new Rect(-center.X - size.Width * 0.5, -center.Y - size.Height * 0.5, size.Width, size.Height);
        }

    }

    public enum TransformSelect
    {
        Calc,
        View,
    }

}
