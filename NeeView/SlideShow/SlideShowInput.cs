using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class SlideShowInput : IDisposable
    {
        private readonly SlideShowConfig _config;
        private readonly SlideShow _slideShow;
        private readonly FrameworkElement _element;
        private bool _disposedValue;


        public SlideShowInput(FrameworkElement element, SlideShow slideShow)
        {
            _config = Config.Current.SlideShow;
            _slideShow = slideShow;
            _element = element;
            _element.PreviewKeyDown += Element_PreviewKeyDown;
            _element.PreviewMouseDown += Element_PreviewMouseDown;
            _element.PreviewMouseWheel += Element_PreviewMouseWheel;
            _element.PreviewMouseMove += Element_PreviewMouseMove;
            _element.AddPreviewMouseHorizontalWheelHandle(Element_PreviewMouseHorizontalWheelChanged);
        }

        private void Element_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_slideShow.IsPlaying) return;

            ResetTimer(SlideShowTimerResetGesture.InputAction);
        }

        private void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_slideShow.IsPlaying) return;

            ResetTimer(SlideShowTimerResetGesture.InputAction);
            _slideShow.CancelScroll();
        }

        private void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_slideShow.IsPlaying) return;

            ResetTimer(SlideShowTimerResetGesture.InputAction);
            _slideShow.CancelScroll();
        }

        private void Element_PreviewMouseHorizontalWheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (!_slideShow.IsPlaying) return;

            ResetTimer(SlideShowTimerResetGesture.InputAction);
            _slideShow.CancelScroll();
        }

        private void Element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_slideShow.IsPlaying) return;

            ResetTimer(SlideShowTimerResetGesture.MouseMove);
        }

        private void ResetTimer(SlideShowTimerResetGesture resetGesture)
        {
            if (!_config.IsPrioritizeTime && _config.TimerResetGesture >= resetGesture)
            {
                _slideShow.ResetTimer();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _element.PreviewKeyDown -= Element_PreviewKeyDown;
                    _element.PreviewMouseDown -= Element_PreviewMouseDown;
                    _element.PreviewMouseWheel -= Element_PreviewMouseWheel;
                    _element.PreviewMouseMove -= Element_PreviewMouseMove;
                    _element.RemovePreviewMouseHorizontalWheelHandle(Element_PreviewMouseHorizontalWheelChanged);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
