using NeeView.PageFrames;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MouseWheelScroll
    {
        private readonly MouseConfig _mouseConfig;
        private readonly IDragTransformContextFactory _dragTransformContextFactory;


        public MouseWheelScroll(IDragTransformContextFactory dragTransformContextFactory)
        {
            _mouseConfig = Config.Current.Mouse;
            _dragTransformContextFactory = dragTransformContextFactory;
        }


        public void OnMouseVerticalWheel(object? sender, MouseWheelEventArgs e)
        {
            if (CanMouseWheelScroll(e))
            {
                var delta = e.Delta * _mouseConfig.MouseWheelScrollSensitivity;
                var duration = TimeSpan.FromSeconds(_mouseConfig.MouseWheelScrollDuration);
                DoMove(new Vector(0.0, delta), duration);
                e.Handled = true;
            }
        }

        public void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            if (CanMouseWheelScroll(e))
            {
                var delta = -e.Delta * _mouseConfig.MouseWheelScrollSensitivity;
                var duration = TimeSpan.FromSeconds(_mouseConfig.MouseWheelScrollDuration);
                DoMove(new Vector(delta, 0.0), duration);
                e.Handled = true;
            }
        }

        private bool CanMouseWheelScroll(MouseWheelEventArgs e)
        {
            return _mouseConfig.IsMouseWheelScrollEnabled && Keyboard.Modifiers == ModifierKeys.None && MouseButtonBitsExtensions.Create(e) == MouseButtonBits.None;
        }

        private void DoMove(Vector delta, TimeSpan duration)
        {
            var transformControl = CreateTransformControl();
            if (transformControl is null) return;

            transformControl.DoMove(delta, duration, EaseTools.LinearEase, EaseTools.LinearEase);
        }

        private DragTransform? CreateTransformControl()
        {
            var transformContext = _dragTransformContextFactory?.CreateContentDragTransformContext(false);
            if (transformContext is null) return null;

            return new DragTransform(transformContext);
        }
    }
}
