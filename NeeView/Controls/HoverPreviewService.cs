using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    public class HoverPreviewService : IDisposable
    {
        public static HoverPreviewService Current { get; } = new HoverPreviewService();

        private readonly DispatcherTimer _timer;
        private readonly FrameworkElement _root;
        private readonly ContentPresenter _content;
        private readonly Popup _popup;
        private bool _isEnabled = true;
        private bool _isOpen;
        private int _count;
        private int _timestamp;
        private bool _disposedValue;


        private HoverPreviewService()
        {
            _content = new ContentPresenter();

            _root = new Border()
            {
                BorderThickness = new Thickness(1.0),
                BorderBrush = Brushes.Gray,
                Background = Brushes.White,
                Padding = new Thickness(4.0),
                Child = _content
            };

            _popup = new Popup
            {
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade,
                Child = _root
            };

            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;

            ApplicationDisposer.Current.Add(this);
        }


        public PlacementMode Placement { get; set; } = PlacementMode.Left;
        public int ShowDelay { get; set; } = 1000;
        public int HideDelay { get; set; } = 100;
        public int BetweenMargin { get; set; } = 500;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (!_isEnabled)
                    {
                        Hide();
                    }
                }
            }
        }

        public bool IsOpen
        {
            get => _isOpen;
            private set
            {
                if (_isOpen != value)
                {
                    _timer.Stop();
                    _isOpen = value;
                    _timer.Interval = TimeSpan.FromMilliseconds(_isOpen ? (System.Environment.TickCount - _timestamp < BetweenMargin ? 0 : ShowDelay) : HideDelay);
                    _timer.Start();
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            _timer.Stop();

            // 閉じたときの時間を記憶
            if (_popup.IsOpen && !_isOpen)
            {
                _timestamp = System.Environment.TickCount;
            }

            _popup.IsOpen = _isOpen;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _popup.IsOpen = false;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Show(UIElement target, object? content)
        {
            if (_disposedValue) return;

            if (!_isEnabled)
            {
                return;
            }

            _count++;

            _root.Visibility = content is null ? Visibility.Collapsed : Visibility.Visible;

            _content.Content = content;

            _popup.PlacementTarget = target;
            _popup.Placement = Placement;
            // hack: update place
            _popup.HorizontalOffset = 1.0;
            _popup.HorizontalOffset = 0.0;

            IsOpen = true;
        }

        public void Hide()
        {
            if (_disposedValue) return;

            _count--;

            if (_count == 0)
            {
                IsOpen = false;
            }

            if (_count < 0)
            {
                _count = 0;
            }
        }

    }
}