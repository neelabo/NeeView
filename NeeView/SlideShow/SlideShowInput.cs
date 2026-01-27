using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class SlideShowInput : IDisposable
    {
        private readonly SlideShow _slideShow;
        private readonly FrameworkElement _element;
        private readonly MouseHorizontalWheelSource? _mouseHorizontalWheelSource;
        private bool _disposedValue;


        public SlideShowInput(FrameworkElement element, SlideShow slideShow)
        {
            _slideShow = slideShow;
            _element = element;
            _element.PreviewKeyDown += Element_PreviewKeyDown;
            _element.PreviewMouseDown += Element_PreviewMouseDown;
            _element.PreviewMouseMove += Element_PreviewMouseMove;
            _element.PreviewMouseWheel += Element_PreviewMouseWheel;

            if (Window.GetWindow(_element) is INotifyMouseHorizontalWheelChanged window)
            {
                ////Debug.WriteLine($"MouseInput.Sender: InitializeMouseHorizontalWheel()");
                _mouseHorizontalWheelSource = new MouseHorizontalWheelSource(_element, window);
                _mouseHorizontalWheelSource.MouseHorizontalWheelChanged += Element_MouseHorizontalWheelChanged;
            }
        }

        private void Element_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _slideShow.ResetTimer();
        }

        private void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _slideShow.ResetTimer();
        }

        private void Element_MouseHorizontalWheelChanged(object sender, MouseWheelEventArgs e)
        {
            _slideShow.ResetTimer();
        }

        private void Element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Config.Current.SlideShow.IsCancelSlideByMouseMove)
            {
                _slideShow.ResetTimer();
            }
        }

        private void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _slideShow.ResetTimer();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _element.PreviewKeyDown -= Element_PreviewKeyDown;
                    _element.PreviewMouseDown -= Element_PreviewMouseDown;
                    _element.PreviewMouseMove -= Element_PreviewMouseMove;
                    _element.PreviewMouseWheel -= Element_PreviewMouseWheel;
                    _mouseHorizontalWheelSource?.Dispose();
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
