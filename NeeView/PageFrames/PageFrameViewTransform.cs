//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [LocalDebug]
    public partial class PageFrameViewTransform : IPointControl, INotifyTransformChanged, IDisposable
    {
        private readonly MultiTransform _transform = new();
        private readonly LoupeTransformContext _loupeContext;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public PageFrameViewTransform(PageFrameContext context, LoupeTransformContext loupeContext)
        {
            _loupeContext = loupeContext;
            _disposables.Add(_loupeContext.SubscribeTransformChanged(LoupeContext_TransformChanged));
            _disposables.Add(context.SubscribePropertyChanged(nameof(context.PageMode), (s, e) => Update()));
            _disposables.Add(context.SubscribePropertyChanged(nameof(context.IsPanorama), (s, e) => Update()));

            TransformView.Changed += TransformView_Changed;
            _disposables.Add(() => TransformView.Changed -= TransformView_Changed);

            Update();
        }

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;

        [Subscribable]
        public event EventHandler? ViewPointChanged;


        public TransformGroup TransformView { get; private set; } = new();
        public TransformGroup TransformCalc { get; private set; } = new();

        public Point Point => _transform.Point;

        public Point ViewPoint => new Point(TransformView.Value.OffsetX, TransformView.Value.OffsetY);



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

        private void Update()
        {
            TransformView.Children.Clear();
            TransformView.Children.Add(_transform.TransformView);
            TransformView.Children.Add(_loupeContext.GetCanvasTransform());

            TransformCalc.Children.Clear();
            TransformCalc.Children.Add(_transform.TransformCalc);
            TransformCalc.Children.Add(_loupeContext.GetCanvasTransform());
        }


        private void TransformView_Changed(object? sender, EventArgs e)
        {
            Debug.Assert(!_disposedValue);

            ViewPointChanged?.Invoke(this, EventArgs.Empty);
        }


        private void LoupeContext_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            Debug.Assert(!_disposedValue);

            TransformChanged?.Invoke(sender, e);
        }

        public TransformGroup GetTransform(TransformSelect select)
        {
            return select == TransformSelect.View ? TransformView : TransformCalc;
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (_disposedValue) return;

            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (_disposedValue) return;

            // 座標の小数点以下は切り捨てる（描画品質の向上のため）
            value = TransformTools.RoundPoint(value);

            if (_transform.Point != value)
            {
                LocalDebug.WriteLine($"$$ {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
                _transform.SetPoint(value, span, easeX, easeY);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.View, TransformAction.Point));
            }
        }

        public Vector GetVelocity()
        {
            return _transform.GetVelocity();
        }

        public void ResetVelocity()
        {
            if (_disposedValue) return;

            _transform.ResetVelocity();
        }

        public void Flush()
        {
            if (_disposedValue) return;

            _transform.Flush();
        }

    }
}
