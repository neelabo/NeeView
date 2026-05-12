using NeeLaboratory.Generators;
using System;
using System.Windows.Media;

namespace NeeView.Windows.Media
{
    /// <summary>
    /// CompositionTarget 拡張
    /// </summary>
    /// <remarks>
    /// フレーム単位の描写に適したイベントを提供
    /// </remarks>
    public static partial class CompositionTargetEx
    {
        private static TimeSpan _last = TimeSpan.Zero;

        private static event EventHandler<RenderingDeltaEventArgs>? _frameUpdating;

        [Subscribable]
        public static event EventHandler<RenderingDeltaEventArgs>? Rendering
        {
            add
            {
                if (_frameUpdating == null)
                {
                    _last = TimeSpan.Zero;
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                _frameUpdating += value;
            }
            remove
            {
                _frameUpdating -= value;
                if (_frameUpdating == null)
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }

        private static void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (AppState.Current.IsSuspended)
            {
                return;
            }

            var renderingTime = ((RenderingEventArgs)e).RenderingTime;
            if (renderingTime == _last)
            {
                return;
            }
            var delta = _last != TimeSpan.Zero ? renderingTime - _last : TimeSpan.Zero;
            var args = new RenderingDeltaEventArgs(renderingTime, delta);
            _last = renderingTime;
            _frameUpdating?.Invoke(sender, args);
        }
    }


    public class RenderingDeltaEventArgs : EventArgs
    {
        public RenderingDeltaEventArgs(TimeSpan renderingTime, TimeSpan deltaTime)
        {
            RenderingTime = renderingTime;
            DeltaTime = deltaTime;
        }

        public TimeSpan RenderingTime { get; }
        public TimeSpan DeltaTime { get; }
    }
}
