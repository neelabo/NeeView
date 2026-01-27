using NeeView.Interop;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeeView
{
    public class MouseHorizontalWheelService : INotifyMouseHorizontalWheelChanged
    {
        public static readonly RoutedEvent MouseHorizontalWheelEvent = EventManager.RegisterRoutedEvent("MouseHorizontalWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MouseHorizontalWheelService));

        private readonly Window _window;


        public MouseHorizontalWheelService(Window window)
        {
            _window = window;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hwnd).AddHook(WndProc);
            }
            else
            {
                _window.Loaded += Window_Loaded;
            }
        }


        public event MouseWheelEventHandler? MouseHorizontalWheelChanged;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _window.Loaded -= Window_Loaded;

            var hwnd = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WindowMessages)msg)
            {
                case WindowMessages.WM_MOUSEHWHEEL:
                    MouseWheelEventArgs? args = null;
                    try
                    {
                        // NOTE: デバイスが特定できないのでとりあえず Mouse.PrimaryDevice を使用
                        // NOTE: ReferenceSource を見る限り Mouse.PrimaryDevice が null になることはないようだが、念の為に確認している
                        if (Mouse.PrimaryDevice != null)
                        {
                            var delta = IntPtrMethods.GET_WHEEL_DELTA_WPARAM(wParam);
                            args = new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta) { RoutedEvent = MouseHorizontalWheelEvent };
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    if (args != null)
                    {
                        MouseHorizontalWheelChanged?.Invoke(_window, args);
                        handled = args.Handled;
                    }
                    break;
            }

            return IntPtr.Zero;
        }
    }

}
