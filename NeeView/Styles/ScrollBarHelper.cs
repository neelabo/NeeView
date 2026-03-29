using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// tools for NVScrollBar 
    /// </summary>
    /// <remarks>
    /// Support auto-hide scroll bar.
    /// </remarks>
    public static class ScrollBarHelper
    {
        /// <summary>
        /// Global auto-hide setting change trigger for each ScrollBar.
        /// この値自体をローカル設定として使うのではなく、
        /// グローバル設定変更時に ScrollBar の見た目更新を発火させるための添付プロパティ。
        /// </summary>
        public static readonly DependencyProperty IsAutoHideProperty =
            DependencyProperty.RegisterAttached("IsAutoHide", typeof(bool), typeof(ScrollBarHelper),
                new PropertyMetadata(false, OnIsAutoHideChanged));

        public static void SetIsAutoHide(DependencyObject obj, bool value) => obj.SetValue(IsAutoHideProperty, value);
        public static bool GetIsAutoHide(DependencyObject obj) => (bool)obj.GetValue(IsAutoHideProperty);

        private static void OnIsAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollBar scrollBar)
            {
                if (scrollBar.IsLoaded)
                {
                    Update(scrollBar);
                }
                else
                {
                    scrollBar.Loaded -= ScrollBar_Loaded;
                    scrollBar.Loaded += ScrollBar_Loaded;
                }
            }
        }

        private static void ScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            scrollBar.Loaded -= ScrollBar_Loaded;
            Update(scrollBar);
        }

        private static void Update(ScrollBar scrollBar)
        {
            var track = scrollBar.Template?.FindName("PART_Track", scrollBar) as Track;
            if (track != null)
            {
                if (scrollBar.Orientation == Orientation.Vertical)
                {
                    VerticalScrollBarHelper.UpdateAnimation(track);
                }
                else
                {
                    HorizontalScrollBarHelper.UpdateAnimation(track);
                }
            }
        }
    }


    /// <summary>
    /// DependencyProperty for scroll bar thumb
    /// </summary>
    public static class ScrollBarThumbHelper
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ScrollBarThumbHelper),
                new PropertyMetadata(new CornerRadius(3)));

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    }


    /// <summary>
    /// DependencyProperty for vertical scroll bar
    /// </summary>
    public static class VerticalScrollBarHelper
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(VerticalScrollBarHelper),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static void SetIsActive(DependencyObject obj, bool value) => obj.SetValue(IsActiveProperty, value);
        public static bool GetIsActive(DependencyObject obj) => (bool)obj.GetValue(IsActiveProperty);


        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                UpdateAnimation(element, (bool)e.NewValue, false);
            }
        }

        public static void UpdateAnimation(FrameworkElement element)
        {
            UpdateAnimation(element, GetIsActive(element), true);
        }

        public static void UpdateAnimation(FrameworkElement element, bool isActive, bool now)
        {
            var anime = ScrollBarTools.CreateAnimation(element, isActive, now, element.ActualWidth);
            element.BeginAnimation(FrameworkElement.WidthProperty, anime);

            ScrollBarTools.UpdateThumbCornerRadius(element, isActive);
        }
    }


    /// <summary>
    /// DependencyProperty for horizontal scroll bar
    /// </summary>
    public static class HorizontalScrollBarHelper
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(HorizontalScrollBarHelper),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static void SetIsActive(DependencyObject obj, bool value) => obj.SetValue(IsActiveProperty, value);
        public static bool GetIsActive(DependencyObject obj) => (bool)obj.GetValue(IsActiveProperty);


        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                UpdateAnimation(element, (bool)e.NewValue, false);
            }
        }

        public static void UpdateAnimation(FrameworkElement element)
        {
            UpdateAnimation(element, GetIsActive(element), true);
        }

        public static void UpdateAnimation(FrameworkElement element, bool isActive, bool now)
        {
            var anime = ScrollBarTools.CreateAnimation(element, isActive, now, element.ActualHeight);
            element.BeginAnimation(FrameworkElement.HeightProperty, anime);

            ScrollBarTools.UpdateThumbCornerRadius(element, isActive);
        }
    }


    /// <summary>
    /// DependencyProperty for scroll bar button
    /// </summary>
    public static class ScrollBarButtonHelper
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(ScrollBarButtonHelper),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static void SetIsActive(DependencyObject obj, bool value) => obj.SetValue(IsActiveProperty, value);
        public static bool GetIsActive(DependencyObject obj) => (bool)obj.GetValue(IsActiveProperty);

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                bool isActive = (bool)e.NewValue;

                double target = isActive ? 1.0 : 0.0;

                var anime = new DoubleAnimation(target, TimeSpan.Zero);

                if (element.IsLoaded && Config.Current.Window.IsAutoHideScrollBar)
                {
                    anime.EasingFunction = isActive ? ScrollBarTools.EasingIn : ScrollBarTools.EasingOut;
                    anime.Duration = TimeSpan.FromSeconds(0.1);
                }
                else
                {
                    anime.From = target;
                }

                element.BeginAnimation(FrameworkElement.OpacityProperty, anime);
            }
        }
    }

    /// <summary>
    /// NVScrollBar animation tools
    /// </summary>
    public static class ScrollBarTools
    {
        public const double ActiveWidth = 12.0;
        public const double InactiveWidth = 6.0;

        public static CubicEase EasingIn { get; } = new CubicEase() { EasingMode = EasingMode.EaseIn };
        public static CubicEase EasingOut { get; } = new CubicEase() { EasingMode = EasingMode.EaseOut };

        public static double GetWidth(bool isAutoHide, bool isActive)
        {
            return !isAutoHide || isActive ? ActiveWidth : InactiveWidth;
        }

        public static DoubleAnimation CreateAnimation(FrameworkElement element, bool isActive, bool now, double current)
        {
            bool isAutoHide = ScrollBarSettings.Instance.IsAutoHide;

            double target = GetWidth(isAutoHide, isActive);

            var anime = new DoubleAnimation(target, TimeSpan.Zero);

            if (element.IsLoaded && isAutoHide && current > 0 && !now)
            {
                anime.From = current;
                anime.EasingFunction = isActive ? EasingIn : EasingOut;
                anime.Duration = TimeSpan.FromSeconds(0.1);
            }
            else
            {
                anime.From = target;
            }

            return anime;
        }

        public static void UpdateThumbCornerRadius(FrameworkElement element, bool isActive)
        {
            if (element is not Track { Thumb: not null } track) return;

            bool isAutoHide = ScrollBarSettings.Instance.IsAutoHide;
            var cornerRadius = new CornerRadius(isAutoHide && !isActive ? 0 : 3);
            ScrollBarThumbHelper.SetCornerRadius(track.Thumb, cornerRadius);
        }
    }


    /// <summary>
    /// global NVScrollBar settings
    /// </summary>
    public class ScrollBarSettings : BindableBase
    {
        private static readonly ScrollBarSettings _instance = new();
        public static ScrollBarSettings Instance => _instance;

        private bool _isAutoHide;

        public ScrollBarSettings()
        {
            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsAutoHideScrollBar),
                (s, e) => UpdateScrollBarSettings());

            UpdateScrollBarSettings();
        }

        public bool IsAutoHide
        {
            get => _isAutoHide;
            set => SetProperty(ref _isAutoHide, value);
        }

        private void UpdateScrollBarSettings()
        {
            IsAutoHide = Config.Current.Window.IsAutoHideScrollBar;
        }
    }
}
