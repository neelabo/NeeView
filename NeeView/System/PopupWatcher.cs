using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Popup監視
    /// </summary>
    public static class PopupWatcher
    {
        public static readonly Lock _lock = new();
        public static readonly List<UIElement> _elements = new();

        public static UIElement? TargetElement { get; private set; }

        public static event EventHandler<TargetElementChangedEventArgs>? TargetElementChanged;

        public static void AddTargetElement(object sender, UIElement targetElement)
        {
            if (targetElement is null) return;

            lock (_lock)
            {
                _elements.Remove(targetElement);
                _elements.Add(targetElement);
                TargetElement = _elements.LastOrDefault();
            }

            TargetElementChanged?.Invoke(sender, new TargetElementChangedEventArgs(targetElement));
        }

        public static void RemoveTargetElement(object sender, UIElement targetElement)
        {
            if (targetElement is null) return;

            lock (_lock)
            {
                _elements.Remove(targetElement);
                TargetElement = _elements.LastOrDefault();
            }

            TargetElementChanged?.Invoke(sender, new TargetElementChangedEventArgs(TargetElement));
        }
    }
}
