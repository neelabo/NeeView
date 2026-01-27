using NeeView.Windows.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// コンテキストメニューが開くタイミングを監視する
    /// </summary>
    /// <remarks>
    /// NOTE: ContextMenuOpeningでキャンセルされたときの動作に非対応
    /// </remarks>
    public static class ContextMenuWatcher
    {
        private static bool _isInitialized;
        private static DelayValue<UIElement?> _targetElement = new();


        public static event EventHandler<TargetElementChangedEventArgs>? TargetElementChanged;
        public static event EventHandler<TargetElementChangedEventArgs>? ContextMenuOpening;
        public static event EventHandler<TargetElementChangedEventArgs>? ContextMenuClosing;


        /// <summary>
        /// 最後に開いたコンテキストメニューターゲット。500msでクリアされる
        /// </summary>
        public static UIElement? TargetElement => _targetElement?.Value;


        public static void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            _targetElement = new DelayValue<UIElement?>();
            _targetElement.ValueChanged += (s, e) => TargetElementChanged?.Invoke(s, new TargetElementChangedEventArgs(_targetElement?.Value));

            EventManager.RegisterClassHandler(typeof(UIElement), ContextMenuService.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
            EventManager.RegisterClassHandler(typeof(UIElement), ContextMenuService.ContextMenuClosingEvent, new ContextMenuEventHandler(OnContextMenuClosing));
        }


        private static void OnContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target is null) return;

            SetTargetElement(target);
            ContextMenuOpening?.Invoke(sender, new TargetElementChangedEventArgs(target));
        }

        private static void OnContextMenuClosing(object? sender, ContextMenuEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target is null) return;

            SetTargetElement(null);
            ContextMenuClosing?.Invoke(sender, new TargetElementChangedEventArgs(target));
        }

        private static UIElement? GetContextMenuTarget(object? sender)
        {
            if (sender is not DependencyObject target) return null;

            var menu = ContextMenuService.GetContextMenu(target);
            if (menu is null) return null;

            return target as UIElement;
        }

        public static void RaiseContextMenuOpening(object sender, UIElement? element)
        {
            if (!_isInitialized) throw new InvalidOperationException();

            SetTargetElement(element);
            ContextMenuOpening?.Invoke(sender, new TargetElementChangedEventArgs(element));
        }

        public static void RaiseContextMenuClosing(object sender, UIElement? element)
        {
            if (!_isInitialized) throw new InvalidOperationException();

            SetTargetElement(null);
            ContextMenuClosing?.Invoke(sender, new TargetElementChangedEventArgs(element));
        }

        /// <summary>
        /// 直接コンテキストメニューターゲットを指定する。
        /// 独自操作でコンテキストメニューを開く場合等に使用する
        /// </summary>
        public static void SetTargetElement(UIElement? element)
        {
            if (!_isInitialized) throw new InvalidOperationException();

            _targetElement.SetValue(element, 0.0);

            if (element is not null)
            {
                _targetElement.SetValue(null, 500.0); // keep 500ms.
            }
        }

    }


    public class TargetElementChangedEventArgs : EventArgs
    {
        public TargetElementChangedEventArgs(UIElement? targetElement)
        {
            TargetElement = targetElement;
        }

        public UIElement? TargetElement { get; set; }
    }
}
