using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// CompositionTarget.Rendering イベントによる慣性移動処理
    /// </summary>
    /// <remarks>
    /// どうしてもAnimationのように滑らかにならないので未使用
    /// </remarks>
    [Obsolete]
    public class InertiaComponent : IDisposable
    {
        private bool _disposedValue;
        private Vector _speed;
        private int _tickCount;
        private readonly ITransformControl _transformControl;

        public InertiaComponent(ITransformControl transformControl)
        {
            _transformControl = transformControl;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_disposedValue) return;

            var length = _speed.Length;
            if (length <= 0.01)
            {
                _speed = default;
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                return;
            }

            var tickCount = System.Environment.TickCount;
            var span = tickCount - _tickCount;
            if (span <= 0) return;

            var v = _speed;
            var a = _speed * (0.01 / length);
            var t0 = v.Length / a.Length;
            var t = Math.Min(span, t0);
            var s = v * t - a * t * t * 0.5;

            Debug.WriteLine($">>> Add = {s:f2}, Span={span}");

            var p0 = _transformControl.Point + s;
            var p1a = _transformControl.Point + s;

            // add s to position
            //_transformControl.AddPoint(s, TimeSpan.FromMilliseconds(span));
            _transformControl.AddPoint(s, TimeSpan.Zero);

            _tickCount = tickCount;
            _speed = v - a * t;

            var p1b = _transformControl.Point;
            if (p1a != p1b)
            {
                if (p1a.X != p1b.X)
                {
                    Debug.WriteLine($">>> Hit.X");
                    _speed.X = 0.0;
                }
                if (p1a.Y != p1b.Y)
                {
                    Debug.WriteLine($">>> Hit.Y");
                    _speed.Y = 0.0;
                }
            }
        }

        public void Start(Vector speed)
        {
            if (_disposedValue) return;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            _speed = speed;
            _tickCount = System.Environment.TickCount;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
    }
}
