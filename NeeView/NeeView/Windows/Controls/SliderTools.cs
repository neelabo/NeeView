using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    // https://stackoverflow.com/questions/2909862/slider-does-not-drag-in-combination-with-ismovetopointenabled-behaviour
    public class SliderTools : DependencyObject
    {
        public static bool GetMoveToPointOnDrag(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveToPointOnDragProperty);
        }

        public static void SetMoveToPointOnDrag(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveToPointOnDragProperty, value);
        }

        public static readonly DependencyProperty MoveToPointOnDragProperty =
            DependencyProperty.RegisterAttached("MoveToPointOnDrag", typeof(bool), typeof(SliderTools), new PropertyMetadata(false, MoveToPointOnDragPropertyChanged));


        private static void MoveToPointOnDragPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Slider slider) return;

            if ((bool)e.NewValue)
            {
                slider.PreviewMouseLeftButtonDown += Slider_PreviewMouseLeftButtonDown;
            }
            else
            {
                slider.PreviewMouseLeftButtonDown -= Slider_PreviewMouseLeftButtonDown;
            }
        }

        private static void Slider_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is not Slider slider) return;

            if (slider.Template.FindName("PART_Track", slider) is not Track track) return;

            var thumb = track.Thumb;
            if (thumb is null || thumb.IsMouseOver) return;

            // マウスポインターの位置に値を更新
            slider.Value = SnapToTick(slider, track.ValueFromPoint(e.GetPosition(track)));
            slider.UpdateLayout();

            // Thumbをドラッグしたのと同じ効果をさせる
            thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source,
            });

            e.Handled = true;
        }

        /// <summary>
        /// Snap to tick if IsSnapToTickEnabled
        /// </summary>
        /// <remarks>
        /// from https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/Slider.cs
        /// </remarks>
        /// <param name="slider">slider control</param>
        /// <param name="value">input value</param>
        /// <returns>snapped value</returns>
        private static double SnapToTick(Slider slider, double value)
        {
            if (slider.IsSnapToTickEnabled)
            {
                double previous = slider.Minimum;
                double next = slider.Maximum;

                DoubleCollection? ticks = slider.Ticks;

                // If ticks collection is available, use it.
                // Note that ticks may be unsorted.
                if ((ticks != null) && (ticks.Count > 0))
                {
                    for (int i = 0; i < ticks.Count; i++)
                    {
                        double tick = ticks[i];
                        if (DoubleUtil.AreClose(tick, value))
                        {
                            return value;
                        }

                        if (DoubleUtil.LessThan(tick, value) && DoubleUtil.GreaterThan(tick, previous))
                        {
                            previous = tick;
                        }
                        else if (DoubleUtil.GreaterThan(tick, value) && DoubleUtil.LessThan(tick, next))
                        {
                            next = tick;
                        }
                    }
                }
                else if (DoubleUtil.GreaterThanZero(slider.TickFrequency))
                {
                    previous = slider.Minimum + (Math.Round(((value - slider.Minimum) / slider.TickFrequency)) * slider.TickFrequency);
                    next = Math.Min(slider.Maximum, previous + slider.TickFrequency);
                }

                // Choose the closest value between previous and next. If tie, snap to 'next'.
                value = DoubleUtil.GreaterThanOrClose(value, (previous + next) * 0.5) ? next : previous;
            }

            return value;
        }


        // from https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/Shared/MS/Internal/DoubleUtil.cs
        private static class DoubleUtil
        {
            internal const double DBL_EPSILON = 2.2204460492503131e-016; /* smallest such that 1.0+DBL_EPSILON != 1.0 */
            internal const float FLT_MIN = 1.175494351e-38F; /* Number close to zero, where float.MinValue is -float.MaxValue */

            public static bool AreClose(double value1, double value2)
            {
                if (value1 == value2) return true;
                double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DBL_EPSILON;
                double delta = value1 - value2;
                return (-eps < delta) && (eps > delta);
            }

            public static bool LessThan(double value1, double value2)
            {
                return (value1 < value2) && !AreClose(value1, value2);
            }

            public static bool GreaterThan(double value1, double value2)
            {
                return (value1 > value2) && !AreClose(value1, value2);
            }

            public static bool GreaterThanZero(double value)
            {
                return value >= 10.0 * DBL_EPSILON;
            }

            public static bool GreaterThanOrClose(double value1, double value2)
            {
                return (value1 > value2) || AreClose(value1, value2);
            }
        }
    }

}
