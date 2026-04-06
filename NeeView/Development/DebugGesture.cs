using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    public static class DebugGesture
    {
        /// <summary>
        /// Ctrl+F12をトリガーにしたデバッグコマンド実行
        /// </summary>
        [Conditional("DEBUG")]
        public static void Initialize(Window window)
        {
            window.PreviewKeyDown += OnPreviewKeyDown;

            void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                // trigger is Ctrl+F12
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
                {
                    CheckFocus(window);
                    //CheckMouseCapture();

                    e.Handled = true;
                }
                // Ctrl+Space
                else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Space)
                {
                    ThemeManager.Current.RefreshThemeColor();
                }
            }
            ;
        }

        [Conditional("DEBUG")]
        public static void RegisterFocusChanged(Window window)
        {
            EventManager.RegisterClassHandler(
                typeof(UIElement),
                Keyboard.PreviewGotKeyboardFocusEvent,
                (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus);

            void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                if (FocusManager.GetFocusedElement(window) is not Visual element)
                {
                    Debug.WriteLine($">> FocusLost:");
                    CheckFocus(window);
                }
            }
        }

        // 現在のフォーカスを取得
        [Conditional("DEBUG")]
        public static void CheckFocus(Window window)
        {
            Debug.WriteLine($"KeyboardFocus: {Keyboard.FocusedElement}");

            var element = FocusManager.GetFocusedElement(window) as Visual;
            ElementWalk(element);
            Debug.WriteLine(".");

            static void ElementWalk(Visual? e)
            {
                if (e == null) return;

                var isKeyboardFocused = e is FrameworkElement frameworkElement && frameworkElement.IsKeyboardFocused;

                var name = (e as FrameworkElement)?.Name;


                var typeName = e.GetType().ToString();
                var valueString = e.ToString();


                if (typeName == valueString)
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({typeName})");
                }
                else if (valueString is not null && valueString.StartsWith(typeName, StringComparison.Ordinal))
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({valueString})");
                }
                else
                {
                    Debug.WriteLine($"FocusTree: {isKeyboardFocused} {name} ({typeName}: {valueString})");
                }

                if (VisualTreeHelper.GetParent(e) is Visual parent)
                {
                    ElementWalk(parent);
                }
            }
        }

        // 現在のマウスキャプチャを取得
        public static void CheckMouseCapture()
        {
            MouseInputHelper.DumpMouseCaptureState();
        }

    }

}


