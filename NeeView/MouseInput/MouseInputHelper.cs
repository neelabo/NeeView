using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// MouseInput Helper
    /// </summary>
    public static class MouseInputHelper
    {
        // MouseCaptureのラップ。開発用

        [Conditional("DEBUG")]
        public static void DumpMouseCaptureState()
        {
            Debug.WriteLine($"> Mouse captured element: {FixedElementName(Mouse.Captured)}");
        }

        public static void CaptureMouse(object sender, IInputElement element)
        {
            //var id = _mouseCaptureSerial++;
            //Debug.WriteLine($"> {id}.MouseCapture: {FixedElementName(element)} by {sender}");
            element.CaptureMouse();
            //Debug.WriteLine($"> {id}.MouseCapture: done.");
        }

        public static void ReleaseMouseCapture(object sender, IInputElement element)
        {
            //var id = _mouseCaptureSerial++;
            //Debug.WriteLine($"> {id}.ReleaseMouseCapture: {FixedElementName(element)} by {sender}");
            //Debug.Assert(Mouse.Captured == element, "WARNING!! It's not caputured element.");
            element.ReleaseMouseCapture();
            //Debug.WriteLine($"> {id}.ReleaseMouseCapture: done.");
        }

        private static string? FixedElementName(IInputElement element)
        {
            if (element is FrameworkElement framweorkElement)
            {
                return framweorkElement.ToString() + (framweorkElement.Name != null ? $" ({framweorkElement.Name})" : "");
            }
            else
            {
                return element?.ToString();
            }
        }

    }
}
