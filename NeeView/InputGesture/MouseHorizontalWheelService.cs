using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace NeeView
{
    public class MouseHorizontalWheelService
    {
        public static readonly RoutedEvent PreviewMouseHorizontalWheelEvent = EventManager.RegisterRoutedEvent("PreviewMouseHorizontalWheel", RoutingStrategy.Tunnel, typeof(MouseWheelEventHandler), typeof(MouseHorizontalWheelService));

        public static void AddPreviewMouseHorizontalWheelHandler(DependencyObject d, MouseWheelEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.AddHandler(PreviewMouseHorizontalWheelEvent, handler);
            }
        }

        public static void RemovePreviewMouseHorizontalWheelHandler(DependencyObject d, MouseWheelEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.RemoveHandler(PreviewMouseHorizontalWheelEvent, handler);
            }
        }


        public static readonly RoutedEvent MouseHorizontalWheelEvent = EventManager.RegisterRoutedEvent("MouseHorizontalWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(MouseHorizontalWheelService));

        public static void AddMouseHorizontalWheelHandler(DependencyObject d, MouseWheelEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.AddHandler(MouseHorizontalWheelEvent, handler);
            }
        }

        public static void RemoveMouseHorizontalWheelHandler(DependencyObject d, MouseWheelEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.RemoveHandler(MouseHorizontalWheelEvent, handler);
            }
        }


        public static void SubscribeHorizontalWheelEvent(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hwnd).AddHook(WndProc);
            }
            else
            {
                window.Loaded -= Window_Loaded;
                window.Loaded += Window_Loaded;
            }
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is Window, "The sender of the Loaded event should be a Window.");
            if (sender is not Window window) return;

            window.Loaded -= Window_Loaded;

            var hwnd = new WindowInteropHelper(window).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }


        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case PInvoke.WM_MOUSEHWHEEL:
                    try
                    {
                        var delta = PInvoke.GET_WHEEL_DELTA_WPARAM(new WPARAM((nuint)wParam));
                        handled = RaiseMouseHorizontalWheelEvent(delta);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private static bool RaiseMouseHorizontalWheelEvent(int delta)
        {
            if (Mouse.PrimaryDevice is null)
            {
                return false;
            }

            var target = Mouse.DirectlyOver as UIElement;
            if (target is null)
            {
                return false;
            }

            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta);
            args.RoutedEvent = PreviewMouseHorizontalWheelEvent;
            args.Source = target;
            target.RaiseEvent(args);

            if (args.Handled) return true;

            args.RoutedEvent = MouseHorizontalWheelEvent;
            target.RaiseEvent(args);

            return args.Handled;
        }
    }



    public static partial class UIElementExtensions
    {
        public static void AddPreviewMouseHorizontalWheelHandle(this UIElement element, MouseWheelEventHandler handler)
        {
            element.AddHandler(MouseHorizontalWheelService.PreviewMouseHorizontalWheelEvent, handler);
        }

        public static void RemovePreviewMouseHorizontalWheelHandle(this UIElement element, MouseWheelEventHandler handler)
        {
            element.RemoveHandler(MouseHorizontalWheelService.PreviewMouseHorizontalWheelEvent, handler);
        }

        public static void AddMouseHorizontalWheelHandle(this UIElement element, MouseWheelEventHandler handler)
        {
            element.AddHandler(MouseHorizontalWheelService.MouseHorizontalWheelEvent, handler);
        }

        public static void RemoveMouseHorizontalWheelHandle(this UIElement element, MouseWheelEventHandler handler)
        {
            element.RemoveHandler(MouseHorizontalWheelService.MouseHorizontalWheelEvent, handler);
        }
    }

}
