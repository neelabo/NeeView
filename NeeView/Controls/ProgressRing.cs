using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    public class ProgressRing : Control
    {
        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }


        private Grid? _root;
        private ArcSegmentShape? _arc;
        private RotateTransform? _arcRotate;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = this.GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            _arc = this.GetTemplateChild("PART_Arc") as ArcSegmentShape ?? throw new InvalidOperationException();
            _arcRotate = this.GetTemplateChild("PART_ArcRotate") as RotateTransform ?? throw new InvalidOperationException();

            _arc.Radius = Radius;
            _arc.StrokeThickness = 4.0;
            _arc.Opacity = 0.0;

            this.Loaded += (s, e) => UpdateActivity();
            this.IsVisibleChanged += (s, e) => UpdateActivity();
        }


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(true, IsActiveProperty_Changed));

        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressRing control)
            {
                control.UpdateActivity();
            }
        }


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressRing), new PropertyMetadata(0.0, ValueProperty_Changed));

        private static void ValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressRing control)
            {
                control.UpdateValue();
            }
        }


        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(ProgressRing), new PropertyMetadata(16.0, RadiusProperty_Changed));

        private static void RadiusProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressRing control)
            {
                control.UpdateRadius();
            }
        }


        private void UpdateRadius()
        {
            if (_root is null) return;
            if (_arc is null) return;

            _arc.Radius = Radius;
            _root.Width = Radius * 2.0 + _arc.StrokeThickness;
            _root.Height = Radius * 2.0 + _arc.StrokeThickness;
        }

        private void UpdateActivity()
        {
            if (_arc is null) return;
            if (_arcRotate is null) return;

            if (IsActive && IsVisible)
            {
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.0)) { BeginTime = TimeSpan.FromSeconds(0.0) };
                _arc.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(0.8);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                _arcRotate.BeginAnimation(RotateTransform.AngleProperty, aniRotate);

                var aniValue = new DoubleAnimation();
                aniValue.From = 0.0;
                aniValue.To = 1.0;
                aniValue.Duration = TimeSpan.FromSeconds(2.0);
                aniValue.RepeatBehavior = RepeatBehavior.Forever;
                this.BeginAnimation(ProgressRing.ValueProperty, aniValue);
            }
            else
            {
                _arc.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
                _arc.Opacity = 0.0;

                _arcRotate.BeginAnimation(RotateTransform.AngleProperty, null, HandoffBehavior.SnapshotAndReplace);

                this.BeginAnimation(ProgressRing.ValueProperty, null, HandoffBehavior.SnapshotAndReplace);
            }
        }

        private void UpdateValue()
        {
            if (_arc is null) return;

            // Arc.Angle vary between 90 and 270.
            double radian = Value * Math.PI * 2.0;
            var deltaAngle = 90.0 * Math.Sin(radian);
            _arc.Angle = 180.0 + deltaAngle;
        }

    }

}
