using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    public class SmartSlider : Control
    {
        static SmartSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SmartSlider), new FrameworkPropertyMetadata(typeof(SmartSlider)));
        }


        private Track? _track;
        private Thumb? _thumb;


        public SmartSlider()
        {
            InitializeCommands();
        }


        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(SmartSlider));
        public event RoutedPropertyChangedEventHandler<double> ValueChanged { add { AddHandler(ValueChangedEvent, value); } remove { RemoveHandler(ValueChangedEvent, value); } }

        public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent("DragStarted", RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(SmartSlider));
        public event DragStartedEventHandler DragStarted { add { AddHandler(DragStartedEvent, value); } remove { RemoveHandler(DragStartedEvent, value); } }

        public static readonly RoutedEvent DragCompletedEvent = EventManager.RegisterRoutedEvent("DragCompleted", RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(SmartSlider));
        public event DragCompletedEventHandler DragCompleted { add { AddHandler(DragCompletedEvent, value); } remove { RemoveHandler(DragCompletedEvent, value); } }

        public static readonly RoutedEvent DragDeltaEvent = EventManager.RegisterRoutedEvent("DragDelta", RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(SmartSlider));
        public event DragDeltaEventHandler DragDelta { add { AddHandler(DragDeltaEvent, value); } remove { RemoveHandler(DragDeltaEvent, value); } }


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SmartSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValuePropertyChanged));

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SmartSlider control)
            {
                control.SyncValue();

                var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue);
                args.RoutedEvent = SmartSlider.ValueChangedEvent;
                control.RaiseEvent(args);
            }
        }


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SmartSlider), new PropertyMetadata(0.0, MinimumPropertyChanged));

        private static void MinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SmartSlider control)
            {
                if (control._track is null) return;
                control._track.Minimum = control.Minimum;
            }
        }


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SmartSlider), new PropertyMetadata(1.0, MaximumPropertyChanged));

        private static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SmartSlider control)
            {
                if (control._track is null) return;
                control._track.Maximum = control.Maximum;
            }
        }


        public bool IsDirectionReversed
        {
            get { return (bool)GetValue(IsDirectionReversedProperty); }
            set { SetValue(IsDirectionReversedProperty, value); }
        }

        public static readonly DependencyProperty IsDirectionReversedProperty =
            DependencyProperty.Register("IsDirectionReversed", typeof(bool), typeof(SmartSlider), new PropertyMetadata(false, IsDirectionReversedPropertyChanged));

        private static void IsDirectionReversedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SmartSlider control)
            {
                if (control._track is null) return;
                control._track.IsDirectionReversed = control.IsDirectionReversed;
                control._track.InvalidateVisual();
            }
        }


        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            set { SetValue(ThumbSizeProperty, value); }
        }

        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register("ThumbSize", typeof(double), typeof(SmartSlider), new PropertyMetadata(25.0));


        public Thickness ThumbThickness
        {
            get { return (Thickness)GetValue(ThumbThicknessProperty); }
            set { SetValue(ThumbThicknessProperty, value); }
        }

        public static readonly DependencyProperty ThumbThicknessProperty =
            DependencyProperty.Register("ThumbThickness", typeof(Thickness), typeof(SmartSlider), new PropertyMetadata(new Thickness(3.0)));


        public Brush ThumbFill
        {
            get { return (Brush)GetValue(ThumbFillProperty); }
            set { SetValue(ThumbFillProperty, value); }
        }

        public static readonly DependencyProperty ThumbFillProperty =
            DependencyProperty.Register("ThumbFill", typeof(Brush), typeof(SmartSlider), new PropertyMetadata(Brushes.Transparent));


        public Brush ThumbFillMouseOver
        {
            get { return (Brush)GetValue(ThumbFillMouseOverProperty); }
            set { SetValue(ThumbFillMouseOverProperty, value); }
        }

        public static readonly DependencyProperty ThumbFillMouseOverProperty =
            DependencyProperty.Register("ThumbFillMouseOver", typeof(Brush), typeof(SmartSlider), new PropertyMetadata(Brushes.Transparent));


        public Brush ThumbBorderBrush
        {
            get { return (Brush)GetValue(ThumbBorderBrushProperty); }
            set { SetValue(ThumbBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty ThumbBorderBrushProperty =
            DependencyProperty.Register("ThumbBorderBrush", typeof(Brush), typeof(SmartSlider), new PropertyMetadata(Brushes.SteelBlue));


        public Brush TrackDecreaseBrush
        {
            get { return (Brush)GetValue(TrackDecreaseBrushProperty); }
            set { SetValue(TrackDecreaseBrushProperty, value); }
        }

        public static readonly DependencyProperty TrackDecreaseBrushProperty =
            DependencyProperty.Register("TrackDecreaseBrush", typeof(Brush), typeof(SmartSlider), new PropertyMetadata(Brushes.Gray));


        public Brush TrackIncreaseBrush
        {
            get { return (Brush)GetValue(TrackIncreaseBrushProperty); }
            set { SetValue(TrackIncreaseBrushProperty, value); }
        }

        public static readonly DependencyProperty TrackIncreaseBrushProperty =
            DependencyProperty.Register("TrackIncreaseBrush", typeof(Brush), typeof(SmartSlider), new PropertyMetadata(Brushes.Gray));


        protected double TrackValue
        {
            get { return _track?.Value ?? 0.0; }
            set
            {
                if (_track is null) return;
                if (!IsDoubleFinite(value)) return;

                var newValue = Math.Max(this.Minimum, Math.Min(this.Maximum, value));
                if (_track.Value != newValue)
                {
                    _track.Value = newValue;
                    this.Value = newValue;
                }
            }
        }

        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(SmartSlider), new PropertyMetadata(0.02));


        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(SmartSlider), new PropertyMetadata(0.1));


        public static RoutedCommand IncreaseLargeCommand { get; } = new RoutedCommand(nameof(IncreaseLargeCommand), typeof(SmartSlider),
            new InputGestureCollection() { new SliderGesture(Key.PageUp, Key.PageDown, false) });
        public static RoutedCommand DecreaseLargeCommand { get; } = new RoutedCommand(nameof(DecreaseLargeCommand), typeof(SmartSlider),
            new InputGestureCollection() { new SliderGesture(Key.PageDown, Key.PageUp, false) });
        public static RoutedCommand IncreaseSmallCommand { get; } = new RoutedCommand(nameof(IncreaseSmallCommand), typeof(SmartSlider),
            new InputGestureCollection() { new SliderGesture(Key.Up, Key.Down, false), new SliderGesture(Key.Right, Key.Left, true) });
        public static RoutedCommand DecreaseSmallCommand { get; } = new RoutedCommand(nameof(DecreaseSmallCommand), typeof(SmartSlider),
            new InputGestureCollection() { new SliderGesture(Key.Down, Key.Up, false), new SliderGesture(Key.Left, Key.Right, true) });
        public static RoutedCommand MinimizeValueCommand { get; } = new RoutedCommand(nameof(MinimizeValueCommand), typeof(SmartSlider),
            new InputGestureCollection() { new KeyGesture(Key.Home) });
        public static RoutedCommand MaximizeValueCommand { get; } = new RoutedCommand(nameof(MaximizeValueCommand), typeof(SmartSlider),
            new InputGestureCollection() { new KeyGesture(Key.End) });


        private void InitializeCommands()
        {
            this.CommandBindings.Add(new CommandBinding(IncreaseLargeCommand, OnIncreaseLargeCommand, (s, e) => e.CanExecute = true));
            this.CommandBindings.Add(new CommandBinding(DecreaseLargeCommand, OnDecreaseLargeCommand, (s, e) => e.CanExecute = true));
            this.CommandBindings.Add(new CommandBinding(IncreaseSmallCommand, OnIncreaseSmallCommand, (s, e) => e.CanExecute = true));
            this.CommandBindings.Add(new CommandBinding(DecreaseSmallCommand, OnDecreaseSmallCommand, (s, e) => e.CanExecute = true));
            this.CommandBindings.Add(new CommandBinding(MinimizeValueCommand, OnMinimizeValueCommand, (s, e) => e.CanExecute = true));
            this.CommandBindings.Add(new CommandBinding(MaximizeValueCommand, OnMaximizeValueCommand, (s, e) => e.CanExecute = true));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _track = this.GetTemplateChild("PART_Track") as Track ?? throw new InvalidOperationException();
            _thumb = this.GetTemplateChild("PART_Thumb") as Thumb ?? throw new InvalidOperationException();

            _track.PreviewMouseLeftButtonDown += Track_MouseLeftButtonDown;

            _thumb.PreviewMouseLeftButtonDown += Thumb_MouseLeftButtonDown;
            _thumb.DragStarted += Thumb_DragStarted;
            _thumb.DragCompleted += Thumb_DragCompleted;
            _thumb.DragDelta += Thumb_DragDelta;
        }

        private void OnIncreaseLargeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Math.Min(Value + LargeChange, Maximum);
        }

        private void OnDecreaseLargeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Math.Min(Value - LargeChange, Maximum);
        }

        private void OnIncreaseSmallCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Math.Min(Value + SmallChange, Maximum);
        }

        private void OnDecreaseSmallCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Math.Min(Value - SmallChange, Maximum);
        }

        private void OnMinimizeValueCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Minimum;
        }

        private void OnMaximizeValueCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Value = Maximum;
        }

        private void Track_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var track = _track;
            if (track is null) return;

            var thumb = track.Thumb;
            if (thumb is null || thumb.IsMouseOver) return;

            // マウスポインターの位置に値を更新
            var value = track.ValueFromPoint(e.GetPosition(track));
            if (IsDoubleFinite(value))
            {
                track.Value = value;
                track.UpdateLayout();

                // Thumbをドラッグしたのと同じ効果をさせる
                thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = e.Source,
                });

                this.Value = track.Value;

                e.Handled = true;
            }
        }

        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var args = new DragStartedEventArgs(e.HorizontalOffset, e.VerticalOffset);
            args.RoutedEvent = SmartSlider.DragStartedEvent;
            RaiseEvent(args);
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SyncValue();

            var args = new DragCompletedEventArgs(e.HorizontalChange, e.VerticalChange, e.Canceled);
            args.RoutedEvent = SmartSlider.DragCompletedEvent;
            RaiseEvent(args);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_track is null) return;

            this.TrackValue += _track.ValueFromDistance(e.HorizontalChange, e.VerticalChange);

            var args = new DragDeltaEventArgs(e.HorizontalChange, e.VerticalChange);
            args.RoutedEvent = SmartSlider.DragDeltaEvent;
            RaiseEvent(args);
        }

        private void SyncValue()
        {
            if (_track is null) return;

            if (_track.Thumb.IsMouseCaptured) return;

            this.TrackValue = this.Value;
        }

        private static bool IsDoubleFinite(double d)
        {
            return !(double.IsInfinity(d) || double.IsNaN(d));
        }
    }



    /// <summary>
    /// from .net reference source
    /// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/Slider.cs,6532ecbe410bb4ae
    /// </summary>
    internal class SliderGesture : InputGesture
    {
        private readonly Key _normal;
        private readonly Key _inverted;
        private readonly bool _forHorizontal;

        public SliderGesture(Key normal, Key inverted, bool forHorizontal)
        {
            _normal = normal;
            _inverted = inverted;
            _forHorizontal = forHorizontal;
        }

        /// <summary>
        /// Sees if the InputGesture matches the input associated with the inputEventArgs
        /// </summary>
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is KeyEventArgs keyEventArgs && targetElement is SmartSlider slider && Keyboard.Modifiers == ModifierKeys.None)
            {
                var key = GetRealKey(keyEventArgs);
                if (_normal == key)
                {
                    return !IsInverted(slider);
                }
                if (_inverted == key)
                {
                    return IsInverted(slider);
                }
            }
            return false;
        }

        private bool IsInverted(SmartSlider slider)
        {
            if (_forHorizontal)
            {
                return slider.IsDirectionReversed != (slider.FlowDirection == FlowDirection.RightToLeft);
            }
            else
            {
                return slider.IsDirectionReversed;
            }
        }

        private static Key GetRealKey(KeyEventArgs keyEventArgs)
        {
            return keyEventArgs.Key switch
            {
                Key.ImeProcessed
                    => keyEventArgs.ImeProcessedKey,
                Key.System
                    => keyEventArgs.SystemKey,
                Key.DeadCharProcessed
                    => keyEventArgs.DeadCharProcessedKey,
                _
                    => keyEventArgs.Key,
            };
        }
    }


    public class ThicknessToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thickness = (Thickness)value;
            return thickness.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
