using Microsoft.Xaml.Behaviors;
using NeeView.Interop;
using NeeView.Windows.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeeView
{
    public class ListViewHorizontalWheelScrollBehavior : Behavior<ListView>
    {
        private ScrollViewer? _scrollViewer;
        private HwndSource? _hwndSource;
        private readonly MouseWheelDelta _mouseWheelDelta = new();

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _hwndSource?.RemoveHook(WndProc);
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
        {
            _scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(AssociatedObject);
            var parentWindow = Window.GetWindow(AssociatedObject);
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(parentWindow).Handle);
            _hwndSource.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WindowMessages)msg)
            {
                case WindowMessages.WM_MOUSEHWHEEL:
                    try
                    {
                        int delta = IntPtrMethods.GET_WHEEL_DELTA_WPARAM(wParam);
                        AppDispatcher.BeginInvoke(() => Scroll(delta));
                        handled = true;
                    }
                    catch
                    {
                        // NOP
                    }
                    break;

                default:
                    break;
            }

            return IntPtr.Zero;
        }

        private void Scroll(int delta)
        {
            if (_scrollViewer is null) return;
            if (!_scrollViewer.IsMouseOver) return;
            if (Mouse.PrimaryDevice is null) return;

            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta);
            var count = _mouseWheelDelta.NotchCount(args);
            while (count > 0)
            {
                _scrollViewer.LineRight();
                count--;
            }
            while (count < 0)
            {
                _scrollViewer.LineLeft();
                count++;
            }
        }
    }
}
