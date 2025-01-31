using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace NeeView
{
    /// <summary>
    /// ProgressRing.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressRing : UserControl
    {
        public ProgressRing()
        {
            InitializeComponent();

            this.Arc.StrokeThickness = 4.0;
            this.Arc.Opacity = 0.0;

            this.Loaded += (s, e) => UpdateActivity();
            this.IsVisibleChanged += (s, e) => UpdateActivity();
            this.SizeChanged += ProgressRing_SizeChanged;
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


        private void ProgressRing_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var radius = Math.Max((Math.Min(e.NewSize.Width, e.NewSize.Height) - this.Arc.StrokeThickness) * 0.5, 1.0);
            this.Arc.Radius = radius;
        }

        private void UpdateActivity()
        {
            if (IsActive && IsVisible)
            {
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.0)) { BeginTime = TimeSpan.FromSeconds(0.0) };
                this.Arc.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(0.8);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.ArcRotate.BeginAnimation(RotateTransform.AngleProperty, aniRotate);

                var aniValue = new DoubleAnimation();
                aniValue.From = 0.0;
                aniValue.To = 1.0;
                aniValue.Duration = TimeSpan.FromSeconds(2.0);
                aniValue.RepeatBehavior = RepeatBehavior.Forever;
                this.BeginAnimation(ProgressRing.ValueProperty, aniValue);
            }
            else
            {
                this.Arc.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
                this.Arc.Opacity = 0.0;

                this.ArcRotate.BeginAnimation(RotateTransform.AngleProperty, null, HandoffBehavior.SnapshotAndReplace);

                this.BeginAnimation(ProgressRing.ValueProperty, null, HandoffBehavior.SnapshotAndReplace);
            }
        }

        private void UpdateValue()
        {
            // Arc.Angle vary between 90 and 270.
            double radian = Value * Math.PI * 2.0;
            var deltaAngle = 90.0 * Math.Sin(radian);
            this.Arc.Angle = 180.0 + deltaAngle;
        }

    }

}
